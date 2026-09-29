using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Repositories;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Identity.Infrastructure.Cache;

/// <summary>
/// In-process <see cref="IMemoryCache" />-backed implementation of
/// <see cref="IUserSecurityStateCache" />. The singleton miss path resolves the scoped repository
/// from a fresh scope; entries carry a short absolute TTL.
/// </summary>
/// <param name="cache">The process-wide memory cache holding the per-user state entries.</param>
/// <param name="scopeFactory">Factory creating a scope to resolve the scoped repository on a miss.</param>
public sealed class UserSecurityStateCache(IMemoryCache cache, IServiceScopeFactory scopeFactory)
    : IUserSecurityStateCache
{
    /// <summary>
    /// How long a loaded state stays cached before the next read goes back to the database.
    /// </summary>
    private static readonly TimeSpan EntryTtl = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Builds the cache key for a user's security-state entry.
    /// </summary>
    /// <param name="userId">The user the state belongs to.</param>
    /// <returns>The namespaced cache key.</returns>
    private static string Key(Guid userId)
    {
        return $"user-security:{userId}";
    }

    /// <inheritdoc />
    public async Task<UserSecurityState> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (cache.TryGetValue(Key(userId), out UserSecurityState cached))
        {
            return cached;
        }

        using IServiceScope scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IUserTokenStateRepository>();
        UserSecurityState? loaded = await repository.GetAsync(userId, cancellationToken);

        // A missing row is not cached: the default state fails closed.
        if (loaded is null)
        {
            return default;
        }

        cache.Set(Key(userId), loaded.Value, EntryTtl);
        return loaded.Value;
    }

    /// <inheritdoc />
    public void Set(Guid userId, UserSecurityState state)
    {
        cache.Set(Key(userId), state, EntryTtl);
    }

    /// <inheritdoc />
    public void Remove(Guid userId)
    {
        cache.Remove(Key(userId));
    }
}
