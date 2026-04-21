using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using OpenTelemetry.Trace;

namespace MyDDD.Template.Infrastructure.Extensions;

internal static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(this IHostApplicationBuilder builder)
    {
        builder.Services.ConfigureOpenTelemetryTracerProvider((sp, tracing) =>
        {
            tracing
                .AddSource(builder.Environment.ApplicationName)
                .AddSource("Wolverine")
                .AddEntityFrameworkCoreInstrumentation()
                .AddNpgsql();
        });

        return builder;
    }
}
