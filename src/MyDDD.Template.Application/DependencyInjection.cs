using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MyDDD.Template.Application.Abstractions.Messaging;

namespace MyDDD.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            config.AddOpenBehavior(typeof(Behaviors.UserSynchronizationBehavior<,>));
            config.AddOpenBehavior(typeof(Behaviors.ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(Behaviors.QueryCachingBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
