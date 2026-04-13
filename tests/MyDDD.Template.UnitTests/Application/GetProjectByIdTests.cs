using Moq;
using FluentAssertions;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Projects.GetProjectById;
using MyDDD.Template.Domain.Projects;
using Xunit;

namespace MyDDD.Template.UnitTests.Application;

public class GetProjectByIdTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;

    public GetProjectByIdTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userContextMock = new Mock<IUserContext>();
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult_When_ProjectExistsAndBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var project = Project.Create("Test Project", userId);
        var query = new GetProjectByIdQuery(project.Id);

        _userContextMock.Setup(x => x.GetUserIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _projectRepositoryMock.Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectRepositoryMock.Object,
            _userContextMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(project.Id);
        result.Value.Name.Should().Be(project.Name);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ProjectDoesNotExist()
    {
        // Arrange
        var query = new GetProjectByIdQuery(Guid.NewGuid());

        _projectRepositoryMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectRepositoryMock.Object,
            _userContextMock.Object,
            default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NotFound");
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ProjectBelongsToAnotherUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var project = Project.Create("Other User's Project", otherUserId);
        var query = new GetProjectByIdQuery(project.Id);

        _userContextMock.Setup(x => x.GetUserIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _projectRepositoryMock.Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectRepositoryMock.Object,
            _userContextMock.Object,
            default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NotFound");
    }
}
