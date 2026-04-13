using FluentAssertions;
using MyDDD.Template.Domain.Projects;
using MyDDD.Template.Domain.Projects.DomainEvents;
using Xunit;

namespace MyDDD.Template.Domain.UnitTests.Projects;

public class ProjectTests
{
    [Fact]
    public void Create_Should_SetPropertiesAndRaiseEvent_When_ArgumentsAreValid()
    {
        // Arrange
        var name = "Test Project";
        var userId = Guid.NewGuid();

        // Act
        var project = Project.Create(name, userId);

        // Assert
        project.Name.Should().Be(name);
        project.UserId.Should().Be(userId);
        project.Id.Should().NotBeEmpty();

        var domainEvent = project.GetDomainEvents().OfType<ProjectCreatedDomainEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.Id.Should().Be(project.Id);
        domainEvent.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_Should_ThrowArgumentException_When_NameIsInvalid(string? name)
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Action act = () => Project.Create(name!, userId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Fact]
    public void UpdateName_Should_UpdateName_When_NewNameIsValid()
    {
        // Arrange
        var project = Project.Create("Old Name", Guid.NewGuid());
        var newName = "New Name";

        // Act
        project.UpdateName(newName);

        // Assert
        project.Name.Should().Be(newName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateName_Should_NotUpdateName_When_NewNameIsInvalid(string? newName)
    {
        // Arrange
        var initialName = "Initial Name";
        var project = Project.Create(initialName, Guid.NewGuid());

        // Act
        project.UpdateName(newName!);

        // Assert
        project.Name.Should().Be(initialName);
    }
}
