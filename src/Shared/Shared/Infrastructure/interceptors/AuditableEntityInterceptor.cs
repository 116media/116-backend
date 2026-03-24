using _116.Shared.Application.Services;
using _116.Shared.Domain;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace _116.Shared.Infrastructure.interceptors;

/// <summary>
/// Intercepts EF Core save changes operations to automatically update audit fields
/// such as <c>CreatedBy</c>, <c>CreatedAt</c>, <c>UpdatedBy</c>, and <c>UpdatedAt</c>
/// on entities implementing <see cref="IEntity"/>.
/// </summary>
public class AuditableEntityInterceptor(ICurrentActor currentActor) : SaveChangesInterceptor
{
    /// <summary>
    /// Called synchronously before <see cref="DbContext.SaveChanges()"/> executes,
    /// to update audit fields on tracked entities.
    /// </summary>
    /// <param name="eventData">The event data providing context about the <see cref="DbContext"/>.</param>
    /// <param name="result">The current interception result.</param>
    /// <returns>The interception result.</returns>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <summary>
    /// Called asynchronously before <see cref="DbContext.SaveChangesAsync(CancellationToken)"/> executes,
    /// to update audit fields on tracked entities.
    /// </summary>
    /// <param name="eventData">The event data providing context about the <see cref="DbContext"/>.</param>
    /// <param name="result">The current interception result.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The interception result as a <see cref="ValueTask{TResult}"/>.</returns>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates audit fields (<c>CreatedBy</c>, <c>CreatedAt</c>, <c>UpdatedBy</c>, <c>UpdatedAt</c>)
    /// for entities tracked by the <see cref="DbContext"/> that implement <see cref="IEntity"/>.
    /// </summary>
    /// <param name="eventDataContext">
    /// The <see cref="DbContext"/> instance tracking the entity changes.
    /// If <c>null</c>, the method exits early.
    /// </param>
    private void UpdateEntities(DbContext? eventDataContext)
    {
        if (eventDataContext == null)
        {
            return;
        }

        string actor = ResolveActor();

        foreach (EntityEntry<IEntity> entry in eventDataContext.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = actor;
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            bool isNewOrUpdated = entry.State is EntityState.Added or EntityState.Modified;
            if (!isNewOrUpdated && !entry.HasChangedOwnedEntities())
            {
                continue;
            }

            entry.Entity.UpdatedBy = actor;
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Resolves the audit actor label for the current operation.
    /// Returns the authenticated user ID, <see cref="EnumAuditActor.Anonymous"/> for
    /// unauthenticated HTTP requests, or <see cref="EnumAuditActor.System"/> for
    /// background jobs and seeders.
    /// </summary>
    private string ResolveActor()
    {
        if (currentActor.UserId is not null)
        {
            return currentActor.UserId;
        }

        return currentActor.HasHttpContext ? nameof(EnumAuditActor.Anonymous) : nameof(EnumAuditActor.System);
    }
}
