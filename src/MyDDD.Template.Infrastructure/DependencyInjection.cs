using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Projects;
using MyDDD.Template.Domain.Users;
using MyDDD.Template.Infrastructure.Auth;
using MyDDD.Template.Infrastructure.Persistence;
using MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.Projects;
using MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.User;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MyDDD.Template.Application.Abstractions.Messaging;
using MyDDD.Template.Application.Users.Events;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Infrastructure.Persistence.Interceptors;
using Wolverine;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;
using Wolverine.EntityFrameworkCore;

namespace MyDDD.Template.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddRedisDistributedCache("cache");
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddSingleton(TimeProvider.System);

        var auditableInterceptor = new UpdateAuditableEntitiesInterceptor(TimeProvider.System);
        services.AddSingleton(auditableInterceptor);

        // Database - PostgreSQL (Aspire Component)
        builder.AddNpgsqlDbContext<ApplicationDbContext>("myddd-db", null, (options) =>
        {
            options.AddInterceptors(auditableInterceptor);
        });

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<IIdentityService, IdentityService>(client =>
        {
            var adminUrl = configuration["Keycloak:AdminUrl"]
                           ?? configuration["Keycloak:Authority"]!;
            client.BaseAddress = new Uri(adminUrl);
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        services.AddTransient<IEmailService, Email.EmailService>();

        // Auth - Keycloak (OIDC)
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
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

        // Telemetry - OpenTelemetry (Additional instrumentation)
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource("MyDDD.Template.Api")
                .AddEntityFrameworkCoreInstrumentation());

        // Wolverine - Messaging & Outbox
        builder.UseWolverine(opts =>
        {
            var rabbitMqConnectionString = configuration.GetConnectionString("rabbitmq");
            if (!string.IsNullOrEmpty(rabbitMqConnectionString))
            {
                opts.UseRabbitMq(new Uri(rabbitMqConnectionString))
                    .AutoProvision();
            }

            // Persistence for the outbox
            opts.PersistMessagesWithPostgresql(configuration.GetConnectionString("myddd-db")!);

            opts.UseEntityFrameworkCoreTransactions();

            opts.Policies.UseDurableLocalQueues();

            // Default routing for integration events
            opts.PublishMessage<UserRegisteredIntegrationEvent>()
                .ToRabbitQueue("integration-events");

            opts.ListenToRabbitQueue("integration-events");
        });

        return builder;
    }
}
