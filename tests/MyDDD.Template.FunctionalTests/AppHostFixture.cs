using System.Net.Http;
using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace MyDDD.Template.FunctionalTests;

public class AppHostFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var appHostBuilder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MyDDD_Template_AppHost>([ "--environment", "Testing" ]);
        
        _app = await appHostBuilder.BuildAsync();
        
        // Start the AppHost and wait for it to be ready
        await _app.StartAsync();
        
        // "api" is the name of the API referencing AppHost project builder
        var httpClient = _app.CreateHttpClient("api");
        HttpClient = httpClient;
    }

    public async Task DisposeAsync()
    {
        if (_app != null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
