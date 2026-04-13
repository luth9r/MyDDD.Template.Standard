using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace MyDDD.Template.Api.Extensions;

internal static class LoggingExtensions
{
    public static void AddSerilogConfig(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSerilog((ctx, cfg) => cfg
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .WriteTo.Console(formatProvider: System.Globalization.CultureInfo.InvariantCulture)
                .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341",
                    formatProvider: System.Globalization.CultureInfo.InvariantCulture),
            preserveStaticLogger: false,
            writeToProviders: true);
    }
}
