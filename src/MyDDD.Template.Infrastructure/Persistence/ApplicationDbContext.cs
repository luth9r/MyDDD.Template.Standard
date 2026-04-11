using Microsoft.EntityFrameworkCore;
using MyDDD.Template.Domain.Primitives;
using MyDDD.Template.Domain.Projects;
using MyDDD.Template.Domain.Users;
using MyDDD.Template.Infrastructure.Persistence.Interceptors;

namespace MyDDD.Template.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();

    public DbSet<User> Users => Set<User>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<UtcValueConverter>();

        configurationBuilder
            .Properties<DateTime?>()
            .HaveConversion<UtcNullableValueConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;

            if (typeof(AggregateRoot).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).Ignore(nameof(AggregateRoot.GetDomainEvents));
            }

            if (typeof(IAuditable).IsAssignableFrom(clrType))
            {
                modelBuilder.Entity(clrType).Property(nameof(IAuditable.CreatedAtUtc)).IsRequired();
                modelBuilder.Entity(clrType).Property(nameof(IAuditable.ModifiedAtUtc));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private class UtcValueConverter()
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private class UtcNullableValueConverter()
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
}
