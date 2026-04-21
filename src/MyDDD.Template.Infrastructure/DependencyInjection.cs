using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDDD.Template.Infrastructure.Extensions;
using MyDDD.Template.Infrastructure.Middleware;

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
