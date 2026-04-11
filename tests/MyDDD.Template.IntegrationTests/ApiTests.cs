using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Aspire.Hosting.Testing;
using FluentAssertions;

namespace MyDDD.Template.IntegrationTests;

public class ApiTests
{
    [Fact]
    public async Task Get_Alive_Should_ReturnOk()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyDDD_Template_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(client =>
        {
            client.AddStandardResilienceHandler();
        });

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");

        // Act
        var response = await httpClient.GetAsync("/alive");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_Health_Should_ReturnOk_In_Development()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyDDD_Template_AppHost>();
        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        var httpClient = app.CreateHttpClient("api");

        // Act
        var response = await httpClient.GetAsync("/health");

        // Assert
        // In the AppHost, api is configured with WaitFor(db, keycloak, seq),
        // so it might take a moment to be healthy if dependencies are starting.
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
