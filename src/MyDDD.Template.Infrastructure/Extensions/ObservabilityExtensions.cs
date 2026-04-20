using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace MyDDD.Template.Infrastructure.Extensions;

internal static class ObservabilityExtensions
{
    public static IHostApplicationBuilder AddObservability(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource("MyDDD.Template.Api")
                .AddSource("Wolverine")
                .AddEntityFrameworkCoreInstrumentation());

        return builder;
    }
}
