using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Constants;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace _116.Integration.Tests.Common.Seeders;

/// <summary>
/// Test-host interceptor that gives every user inserted by a test a token-invalidation record
/// carrying the well-known security stamp the hand-minted test tokens emit. Users created
/// through real application paths already add their own record in the same save, and are skipped.
/// </summary>
public class UserTokenStateSeedingInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SeedMissingTokenStates(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        SeedMissingTokenStates(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Adds a well-known-stamped token-state record for every user being inserted without one.
    /// </summary>
    /// <param name="context">The context whose pending changes are inspected.</param>
    private static void SeedMissingTokenStates(DbContext? context)
    {
        if (context is not IdentityDbContext identityContext)
        {
            return;
        }

        List<Guid> userIdsBeingAdded = identityContext
            .ChangeTracker.Entries<UserEntity>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity.Id)
            .ToList();

        if (userIdsBeingAdded.Count == 0)
        {
            return;
        }

        HashSet<Guid> stateIdsBeingAdded = identityContext
            .ChangeTracker.Entries<UserTokenStateEntity>()
            .Select(entry => entry.Entity.Id)
            .ToHashSet();

        foreach (Guid userId in userIdsBeingAdded.Where(id => !stateIdsBeingAdded.Contains(id)))
        {
            UserTokenStateEntity state = UserTokenStateEntity.Create(userId: userId);
            identityContext.UserTokenStates.Add(state);
            identityContext.Entry(state).Property(s => s.SecurityStamp).CurrentValue = TestConstants
                .Jwt
                .WellKnownSecurityStamp;
        }
    }
}
