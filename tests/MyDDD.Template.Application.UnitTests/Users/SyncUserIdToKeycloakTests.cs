using Moq;
using FluentAssertions;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Exceptions;
using MyDDD.Template.Application.Users.LoginUser;
using MyDDD.Template.Domain.Primitives;
using Xunit;

namespace MyDDD.Template.Application.UnitTests.Users;

public class SyncUserIdToKeycloakTests
{
    private readonly Mock<IIdentityService> _identityServiceMock;

    public SyncUserIdToKeycloakTests()
    {
        _identityServiceMock = new Mock<IIdentityService>();
    }

    [Fact]
    public async Task Handle_Should_CallUpdateUserAttributes_When_MessageIsValid()
    {
        // Arrange
        var message = new SyncUserIdToKeycloak("identity-id", Guid.NewGuid());
        _identityServiceMock.Setup(x => x.UpdateUserAttributesAsync(
                message.IdentityId,
                It.Is<Dictionary<string, string[]>>(d => d["userId"][0] == message.UserId.ToString()),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        // Act
        await SyncUserIdToKeycloakHandler.Handle(message, _identityServiceMock.Object, default);

        // Assert
        _identityServiceMock.Verify(x => x.UpdateUserAttributesAsync(
            message.IdentityId,
            It.IsAny<Dictionary<string, string[]>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowIdentitySyncException_When_UpdateFails()
    {
        // Arrange
        var message = new SyncUserIdToKeycloak("identity-id", Guid.NewGuid());
        var error = MyError.Failure("Sync.Error", "Sync failed");
        _identityServiceMock.Setup(x => x.UpdateUserAttributesAsync(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, string[]>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(error));

        // Act
        var act = async () => await SyncUserIdToKeycloakHandler.Handle(message, _identityServiceMock.Object, default);

        // Assert
        await act.Should().ThrowAsync<IdentitySyncException>()
            .WithMessage("*Failed to sync: Sync failed*");
    }
}
