using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDDD.Template.Api.Middleware;
using MyDDD.Template.Application.Abstractions;
using MyDDD.Template.Infrastructure.Extensions;

namespace MyDDD.Template.Infrastructure;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddPersistence();
        builder.AddAuthenticationAndIdentity();
        builder.AddMessaging();
        builder.AddObservability();

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        return builder;
    }
}
