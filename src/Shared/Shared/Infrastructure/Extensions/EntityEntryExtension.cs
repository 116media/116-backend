using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace _116.Shared.Infrastructure.Extensions;

/// <summary>
/// Provides extension methods for <see cref="EntityEntry"/> to help with tracking owned entity changes.
/// </summary>
public static class EntityEntryExtension
{
    /// <summary>
    /// Determines whether the <see cref="EntityEntry{TEntity}"/> has any related owned entities that are newly added or modified.
    /// </summary>
    /// <param name="entry">The entity entry to inspect.</param>
    /// <returns><c>true</c> if any owned entities have been added or modified; otherwise, <c>false</c>.</returns>
    /// <example>
    /// <code>
    /// var entry = dbContext.Entry(someEntity);
    /// bool hasChanges = entry.HasChangedOwnedEntities();
    /// if (hasChanges)
    /// {
    ///     // Perform logic when owned entities have changed
    /// }
    /// </code>
    /// </example>
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null
            && r.TargetEntry.Metadata.IsOwned()
            && r.TargetEntry.State is EntityState.Added or EntityState.Modified
        );
}
