using Moq;
using FluentAssertions;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Application.Projects.CreateProject;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;
using Xunit;

namespace MyDDD.Template.UnitTests.Application;

public class CreateProjectTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUserContext> _userContextMock;

    public CreateProjectTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _userContextMock = new Mock<IUserContext>();
    }

    [Fact]
    public async Task Handle_Should_ReturnSuccessResult_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateProjectCommand("New Project");
        var userId = Guid.NewGuid();
        _userContextMock.Setup(x => x.GetUserIdAsync(It.IsAny<CancellationToken>())).ReturnsAsync(userId);
        _projectRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await CreateProjectCommandHandler.Handle(
            command,
            _projectRepositoryMock.Object,
            _userContextMock.Object,
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _projectRepositoryMock.Verify(
            x => x.Add(It.Is<Project>(p => p.Name == command.Name && p.UserId == userId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ReturnFailure_When_ProjectNameAlreadyExists()
    {
        // Arrange
        var command = new CreateProjectCommand("Existing Project");
        var existingProject = Project.Create(command.Name, Guid.NewGuid());

        _projectRepositoryMock.Setup(x => x.GetByNameAsync(command.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProject);

        // Act
        var result = await CreateProjectCommandHandler.Handle(
            command,
            _projectRepositoryMock.Object,
            _userContextMock.Object,
            default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Project.DuplicateName");

        _projectRepositoryMock.Verify(x => x.Add(It.IsAny<Project>()), Times.Never);
    }
}
