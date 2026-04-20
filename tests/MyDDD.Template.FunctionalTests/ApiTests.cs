using System.Net;
using FluentAssertions;

namespace MyDDD.Template.FunctionalTests;

public class ApiTests(AppHostFixture fixture) : IClassFixture<AppHostFixture>
{
    [Fact]
    public async Task HealthCheck_ShouldReturnOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApi_ShouldReturnOk()
    {
        // Act
        var response = await fixture.HttpClient.GetAsync("/openapi/v1.json");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
