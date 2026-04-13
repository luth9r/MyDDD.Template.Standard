using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyDDD.Template.Api.Extensions;

internal static class JsonOptionsExtensions
{
    public static IServiceCollection ConfigureJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;

            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());

            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        return services;
    }
}
