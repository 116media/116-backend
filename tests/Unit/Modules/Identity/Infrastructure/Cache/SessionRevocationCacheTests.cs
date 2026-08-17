using _116.Identity.Infrastructure.Cache;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Cache;

/// <summary>
/// Unit tests for <see cref="SessionRevocationCache" />. The denylist is a presence set: an entry
/// exists for the access-token lifetime and disappears on its own afterwards.
/// </summary>
public class SessionRevocationCacheTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly SessionRevocationCache _cache;

    public SessionRevocationCacheTests()
    {
        _cache = new SessionRevocationCache(cache: _memoryCache);
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void IsRevoked_ForAnUnknownSession_ShouldBeFalse()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        bool revoked = _cache.IsRevoked(sessionId: sessionId);

        // Assert
        revoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_AfterRevoke_ShouldBeTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _cache.Revoke(sessionId: sessionId, ttl: TimeSpan.FromMinutes(60));

        // Act
        bool revoked = _cache.IsRevoked(sessionId: sessionId);

        // Assert
        revoked.Should().BeTrue();
    }

    [Fact]
    public void IsRevoked_ShouldTrackEachSessionIndependently()
    {
        // Arrange
        var revokedSessionId = Guid.NewGuid();
        var untouchedSessionId = Guid.NewGuid();
        _cache.Revoke(sessionId: revokedSessionId, ttl: TimeSpan.FromMinutes(60));

        // Act & Assert
        _cache.IsRevoked(sessionId: revokedSessionId).Should().BeTrue();
        _cache.IsRevoked(sessionId: untouchedSessionId).Should().BeFalse();
    }

    [Fact]
    public async Task IsRevoked_AfterTheTtlElapses_ShouldBeFalseAgain()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _cache.Revoke(sessionId: sessionId, ttl: TimeSpan.FromMilliseconds(30));

        // Act — wait comfortably past the TTL so the entry has expired on read
        await Task.Delay(millisecondsDelay: 150);
        bool revoked = _cache.IsRevoked(sessionId: sessionId);

        // Assert
        revoked.Should().BeFalse();
    }
}
