using _116.Identity.Application.Session.Cache;
using Microsoft.Extensions.Caching.Memory;

namespace _116.Identity.Infrastructure.Cache;

/// <summary>
/// In-process <see cref="IMemoryCache" />-backed session denylist that self-trims once the last
/// token minted for a session has expired.
/// </summary>
/// <param name="cache">The process-wide memory cache holding the denylist entries.</param>
public sealed class SessionRevocationCache(IMemoryCache cache) : ISessionRevocationCache
{
    /// <summary>
    /// Builds the cache key for a revoked session entry.
    /// </summary>
    /// <param name="sessionId">The id of the revoked session.</param>
    /// <returns>The namespaced cache key.</returns>
    private static string Key(Guid sessionId)
    {
        return $"session-revoked:{sessionId}";
    }

    /// <inheritdoc />
    public void Revoke(Guid sessionId, TimeSpan ttl)
    {
        cache.Set(Key(sessionId), true, ttl);
    }

    /// <inheritdoc />
    public bool IsRevoked(Guid sessionId)
    {
        return cache.TryGetValue(Key(sessionId), out _);
    }
}
