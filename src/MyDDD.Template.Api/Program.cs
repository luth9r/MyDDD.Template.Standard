using MyDDD.Template.Api.Extensions;
using MyDDD.Template.Application;
using MyDDD.Template.Infrastructure;
using Scalar.AspNetCore;
using Serilog;
using Asp.Versioning;
using Microsoft.OpenApi;

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    builder.AddServiceDefaults();

    builder.Host.UseSerilog((ctx, cfg) => cfg
        .ReadFrom.Configuration(ctx.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
        .WriteTo.Seq(ctx.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
            formatProvider: System.Globalization.CultureInfo.InvariantCulture),
        preserveStaticLogger: false,
        writeToProviders: true);

    builder.Services
        .AddApplication()
        .AddEndpoints();

    builder.AddInfrastructure();

    var kc = builder.Configuration.GetSection("Keycloak");
    var authority = kc["Authority"]!;
    var clientId = kc["ClientId"]!;

    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            document.Components.SecuritySchemes["oauth2"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            ["openid"] = "OpenID Connect",
                            ["profile"] = "User profile info",
                            ["email"] = "User email",
                        },
                    },
                },
            };

            document.Components.SecuritySchemes["jwt"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT token",
            };


            document.SetReferenceHostDocument();
            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            var metadata = context.Description.ActionDescriptor.EndpointMetadata;

            var allowsAnonymous = metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

            if (!allowsAnonymous)
            {
                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("oauth2")] = ["openid", "profile", "email"],
                    },
                    new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("jwt")] = [] },
                ];
            }
            else
            {
                operation.Security = [];
            }

            return Task.CompletedTask;
        });
    });

    var app = builder.Build();

    var apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .HasApiVersion(new ApiVersion(2))
        .ReportApiVersions()
        .Build();

    var versionedGroup = app
        .MapGroup("/api/v{version:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<MyDDD.Template.Infrastructure.Persistence.ApplicationDbContext>();
            // Note: In a production template, it's better to use a dedicated migration worker
            context.Database.EnsureCreated();
        }

        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options
                .WithTitle("MyDDD Template API")
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
                .AddPreferredSecuritySchemes("oauth2", "jwt")
                .AddAuthorizationCodeFlow("oauth2", flow =>
                {
                    flow.ClientId = clientId;
                    flow.Pkce = Pkce.Sha256;
                    flow.SelectedScopes = ["openid", "profile", "email"];
                });
        }).AllowAnonymous();
    }

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
