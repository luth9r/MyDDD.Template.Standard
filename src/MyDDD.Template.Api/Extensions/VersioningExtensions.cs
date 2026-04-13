using Asp.Versioning;
using Asp.Versioning.Builder;

namespace MyDDD.Template.Api.Extensions;

internal static class VersioningExtensions
{
    public static ApiVersionSet GetApiVersionSet(this IEndpointRouteBuilder app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .HasApiVersion(new ApiVersion(2))
            .ReportApiVersions()
            .Build();
    }
}
