using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Users.LoginUser;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Users;
using Xunit;

namespace MyDDD.Template.Application.UnitTests.Users;

public class LoginUserTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ILogger<LoginUserCommand>> _loggerMock;

    public LoginUserTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<LoginUserCommand>>();
    }

    private static AccessTokenResponse CreateToken()
    {
        return new AccessTokenResponse("access", "refresh", 3600, 3600, "Bearer", "id", "session", "scope");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_LoginAsyncFails()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var error = MyError.Validation("Auth.InvalidCredentials", "Invalid credentials");
        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<AccessTokenResponse>(error));

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        syncMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_EmailIsNotVerified()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var token = CreateToken();
        var userInfo = new UserJwtInfo("id", "test@test.com", false, null, "John", "Doe");

        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));
        _identityServiceMock.Setup(x => x.GetUserJwtInfo(token)).Returns(userInfo);

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.EmailNotVerified");
        syncMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessAndNoSync_When_UserExistsAndIsSynced()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var token = CreateToken();
        var userId = Guid.NewGuid();
        var userInfo = new UserJwtInfo("id", "test@test.com", true, userId, "John", "Doe");
        var user = User.Create(userInfo.IdentityId, userInfo.Email, userInfo.FirstName, userInfo.LastName, userId);

        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));
        _identityServiceMock.Setup(x => x.GetUserJwtInfo(token)).Returns(userInfo);
        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(userInfo.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        syncMessage.Should().BeNull();
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessAndSync_When_UserExistsButMissingInToken()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var token = CreateToken();
        var userId = Guid.NewGuid();
        var userInfo = new UserJwtInfo("id", "test@test.com", true, null, "John", "Doe");
        var user = User.Create(userInfo.IdentityId, userInfo.Email, userInfo.FirstName, userInfo.LastName, userId);

        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));
        _identityServiceMock.Setup(x => x.GetUserJwtInfo(token)).Returns(userInfo);
        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(userInfo.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        syncMessage.Should().NotBeNull();
        syncMessage!.UserId.Should().Be(userId);
        syncMessage.IdentityId.Should().Be(userInfo.IdentityId);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessAndNoSync_When_GhostUserDetectedAndRecovered()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var token = CreateToken();
        var oldUserId = Guid.NewGuid();
        var userInfo = new UserJwtInfo("id", "test@test.com", true, oldUserId, "John", "Doe");

        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));
        _identityServiceMock.Setup(x => x.GetUserJwtInfo(token)).Returns(userInfo);
        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(userInfo.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(oldUserId);
        syncMessage.Should().BeNull();
        _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u => u.Id == oldUserId)), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessAndSync_When_NewUserCreated()
    {
        // Arrange
        var command = new LoginUserCommand("test@test.com", "password");
        var token = CreateToken();
        var userInfo = new UserJwtInfo("id", "test@test.com", true, null, "John", "Doe");

        _identityServiceMock.Setup(x => x.LoginAsync(command.Email, command.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(token));
        _identityServiceMock.Setup(x => x.GetUserJwtInfo(token)).Returns(userInfo);
        _userRepositoryMock.Setup(x => x.GetByIdentityIdAsync(userInfo.IdentityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var (result, syncMessage) = await LoginUserCommandHandler.Handle(
            command,
            _identityServiceMock.Object,
            _userRepositoryMock.Object,
            _loggerMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var newUserId = result.Value.UserId;
        syncMessage.Should().NotBeNull();
        syncMessage!.UserId.Should().Be(newUserId);
        _userRepositoryMock.Verify(x => x.Add(It.Is<User>(u => u.Id == newUserId)), Times.Once);
    }
}
