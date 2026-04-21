using FluentAssertions;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Infrastructure;
using NetArchTest.Rules;

namespace MyDDD.Template.ArchitectureTests;

public class ArchitectureTests
{
    private const string ApplicationNamespace = "MyDDD.Template.Application";
    private const string InfrastructureNamespace = "MyDDD.Template.Infrastructure";
    private const string ApiNamespace = "MyDDD.Template.Api";

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
    {
        // Arrange
        var assembly = typeof(Entity).Assembly;

        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace,
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(otherProjects)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure_Or_Api()
    {
        // Arrange
        var assembly = typeof(IUnitOfWork).Assembly;

        var otherProjects = new[]
        {
            InfrastructureNamespace,
            ApiNamespace,
        };

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAll(otherProjects)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Have_Dependency_On_Api()
    {
        // Arrange
        var assembly = typeof(DependencyInjection).Assembly;

        // Act
        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
