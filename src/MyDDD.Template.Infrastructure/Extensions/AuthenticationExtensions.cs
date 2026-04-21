using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Infrastructure.Auth;

namespace MyDDD.Template.Infrastructure.Extensions;

internal static class AuthenticationExtensions
{
    public static IHostApplicationBuilder AddAuthenticationAndIdentity(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Keycloak
        services.AddOptions<KeycloakOptions>()
            .BindConfiguration(KeycloakOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // HTTP
        services.AddHttpClient<IIdentityService, IdentityService>(client =>
            {
                var adminUrl = configuration["Keycloak:AdminUrl"] ?? configuration["Keycloak:Authority"]!;
                client.BaseAddress = new Uri(adminUrl);
            })
            .AddStandardResilienceHandler();

        // User context
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        // JWT
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.MetadataAddress = configuration["Keycloak:MetadataAddress"]!;
                options.Audience = configuration["Keycloak:Audience"]!;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Keycloak:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Keycloak:Audience"],
                    ValidateLifetime = true,
                };
            });

        services.AddAuthorization();

        return builder;
    }
}
