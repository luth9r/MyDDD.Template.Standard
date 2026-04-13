using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application;
using MyDDD.Template.Infrastructure;
using Serilog;
using MyDDD.Template.Api.Middleware;
try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.AddSerilogConfig();

    builder.AddInfrastructure();

    builder.Services
        .AddSwaggerDocumentation(builder.Configuration)
        .AddApplication()
        .AddEndpoints();

    var app = builder.Build();

    var apiVersionSet = app.GetApiVersionSet();

    var versionedGroup = app
        .MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.ApplyMigrations();
        app.MapOpenApi();
        app.MapScalarUi(builder.Configuration);
    }

    app.UseExceptionHandler();

    app.UseAuthentication();
    app.UseAuthorization();

    versionedGroup.MapEndpoints();
    app.MapDefaultEndpoints();

    app.Run();
    Console.WriteLine("=== API STARTED ===");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
