using _116.Identity.Application.Shared.Cache;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Infrastructure.Cache;
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Cache;

/// <summary>
/// Unit tests for <see cref="UserSecurityStateCache" />: read-through loads the state from the
/// repository once per TTL window, write-through makes a bump immediately visible, and a missing
/// row yields the fail-closed default without being cached.
/// </summary>
public class UserSecurityStateCacheTests : IDisposable
{
    private readonly MemoryCache _memoryCache = new(new MemoryCacheOptions());
    private readonly Mock<IUserTokenStateRepository> _repository = new();
    private readonly ServiceProvider _provider;
    private readonly UserSecurityStateCache _cache;

    public UserSecurityStateCacheTests()
    {
        // A minimal provider stands in for the host container the singleton cache scopes into.
        var services = new ServiceCollection();
        services.AddScoped<IUserTokenStateRepository>(_ => _repository.Object);
        _provider = services.BuildServiceProvider();

        _cache = new UserSecurityStateCache(
            cache: _memoryCache,
            scopeFactory: _provider.GetRequiredService<IServiceScopeFactory>()
        );
    }

    public void Dispose()
    {
        _provider.Dispose();
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetAsync_OnAMiss_ShouldLoadFromTheRepositoryAndCache()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var state = new UserSecurityState(SecurityStamp: Guid.NewGuid(), TokenVersion: 3);
        _repository.Setup(r => r.GetAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(state);

        // Act
        UserSecurityState first = await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);
        UserSecurityState second = await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert — the second read is served from memory, not the repository
        first.Should().Be(state);
        second.Should().Be(state);
        _repository.Verify(r => r.GetAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WhenTheRowIsMissing_ShouldReturnTheDefaultWithoutCachingIt()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repository
            .Setup(r => r.GetAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSecurityState?)null);

        // Act
        UserSecurityState first = await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);
        UserSecurityState second = await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert — the default (empty stamp) matches no real token, and the miss is retried
        first.Should().Be(default(UserSecurityState));
        second.Should().Be(default(UserSecurityState));
        _repository.Verify(r => r.GetAsync(userId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Set_ShouldBeObservedByTheNextGet_WithoutTouchingTheRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var state = new UserSecurityState(SecurityStamp: Guid.NewGuid(), TokenVersion: 7);

        // Act
        _cache.Set(userId: userId, state: state);
        UserSecurityState observed = await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert
        observed.Should().Be(state);
        _repository.Verify(r => r.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_ShouldDropTheEntry_SoTheNextGetReloads()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var state = new UserSecurityState(SecurityStamp: Guid.NewGuid(), TokenVersion: 1);
        _repository.Setup(r => r.GetAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(state);
        await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Act
        _cache.Remove(userId: userId);
        await _cache.GetAsync(userId: userId, cancellationToken: CancellationToken.None);

        // Assert — the reload after the removal is a second repository read
        _repository.Verify(r => r.GetAsync(userId, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
