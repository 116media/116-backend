using _116.Shared.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace _116.Unit.Tests.Common.Helpers;

/// <summary>
/// Stamps <c>CreatedAt</c> on inserts for the bare in-memory contexts these repository tests
/// build, standing in for the auditing interceptor the application registers. A row whose
/// timestamp the arrangement already set keeps it, so tests that seed an ordering or a time
/// window still control their own data.
/// </summary>
public sealed class CreatedAtStampingInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void Stamp(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry<IEntity> entry in context.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.CreatedAt is null)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }
        }
    }
}
