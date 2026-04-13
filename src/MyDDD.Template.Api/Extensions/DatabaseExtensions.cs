using MyDDD.Template.Infrastructure.Persistence;

namespace MyDDD.Template.Api.Extensions;

internal static class DatabaseExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();
    }
}
