using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyDDD.Template.Domain.Primitives;

namespace MyDDD.Template.Infrastructure.Persistence.Interceptors;

public sealed class UpdateAuditableEntitiesInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var entries = eventData.Context.ChangeTracker
            .Entries<IAuditable>();

        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(a => a.CreatedAtUtc).CurrentValue = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(a => a.ModifiedAtUtc).CurrentValue = utcNow;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
