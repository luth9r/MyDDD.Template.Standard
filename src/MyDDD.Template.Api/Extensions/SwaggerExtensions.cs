using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace MyDDD.Template.Api.Extensions;

internal static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var kc = configuration.GetSection("Keycloak");
        var authority = kc["Authority"]!;

        services.AddOpenApi(options =>
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

                var allowsAnonymous =
                    metadata.Any(m => m is Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute);

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

        return services;
    }

    public static void MapScalarUi(this IEndpointRouteBuilder app, IConfiguration configuration)
    {
        var clientId = configuration["Keycloak:ClientId"]!;

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
}
