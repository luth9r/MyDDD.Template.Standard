using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyDDD.Template.Domain;
using MyDDD.Template.Domain.Projects;
using MyDDD.Template.Domain.Users;
using MyDDD.Template.Infrastructure.Persistence;
using MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.Projects;
using MyDDD.Template.Infrastructure.Persistence.Configurations.Domain.User;
using MyDDD.Template.Infrastructure.Persistence.Interceptors;
using Wolverine.EntityFrameworkCore;

namespace MyDDD.Template.Infrastructure;

internal static class PersistenceExtensions
{
    public static IHostApplicationBuilder AddPersistence(this IHostApplicationBuilder builder)
    {
        builder.AddRedisDistributedCache("cache");

        builder.Services.AddSingleton(TimeProvider.System);

        var auditableInterceptor = new UpdateAuditableEntitiesInterceptor(TimeProvider.System);
        builder.Services.AddSingleton(auditableInterceptor);

        var connectionString = builder.Configuration.GetConnectionString("myddd-db")!;

        builder.Services.AddDbContextWithWolverineIntegration<ApplicationDbContext>(opts =>
        {
            opts.UseNpgsql(connectionString)
                .UseSnakeCaseNamingConvention();
            opts.AddInterceptors(auditableInterceptor);
        });

        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        return builder;
    }
}
