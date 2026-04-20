using Moq;
using FluentAssertions;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Projects;
using MyDDD.Template.Application.Projects.GetProjectById;
using MyDDD.Template.Domain.Projects;
using Xunit;

namespace MyDDD.Template.Application.UnitTests.Projects;

public class GetProjectByIdTests
{
    private readonly Mock<IProjectQueries> _projectQueriesMock = new();
    private readonly Mock<IUserContext> _userContextMock = new();

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult_When_ProjectExistsAndBelongsToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var project = CreateProjectResponse.Create(Guid.NewGuid(), "Test Project");
        var query = new GetProjectByIdQuery(project.Id);

        _userContextMock.Setup(x => x.GetUserIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _projectQueriesMock.Setup(x => x.GetByIdAsync(project.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectQueriesMock.Object,
            _userContextMock.Object,
            CancellationToken.None);

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

        _projectQueriesMock.Setup(x => x.GetByIdAsync(query.Id, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProjectResponse?)null);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectQueriesMock.Object,
            _userContextMock.Object,
            CancellationToken.None);

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
        var project = CreateProjectResponse.Create(Guid.NewGuid(), "Test Project");
        var query = new GetProjectByIdQuery(project.Id);

        _userContextMock.Setup(x => x.GetUserIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _projectQueriesMock.Setup(x => x.GetByIdAsync(project.Id, otherUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        // Act
        var result = await GetProjectByIdQueryHandler.Handle(
            query,
            _projectQueriesMock.Object,
            _userContextMock.Object,
            CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.NotFound");
    }

    private static class CreateProjectResponse
    {
        public static ProjectResponse Create(Guid id, string name)
        {
            return new ProjectResponse(id, name);
        }
    }
}
