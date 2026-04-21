using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace MyDDD.Template.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
