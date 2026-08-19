using _116.Identity.Application.Shared.Cache;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories.Identity;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="UserTokenStateRepository" />. The marker bumps are
/// set-based ExecuteUpdateAsync statements that only run against a relational
/// provider, so they are covered by the integration suite; unit owns the reads,
/// the get-or-create path, and the empty-role early return.
/// </summary>
public class UserTokenStateRepositoryTests
{
    private readonly IdentityDbContext _context;
    private readonly Mock<IUserSecurityStateCache> _cacheMock = new();
    private readonly UserTokenStateRepository _repository;

    public UserTokenStateRepositoryTests()
    {
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new IdentityDbContext(options);
        _repository = new UserTokenStateRepository(_context, _cacheMock.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistTheState()
    {
        // Arrange
        var state = UserTokenStateEntity.Create(Guid.NewGuid());

        // Act
        await _repository.AddAsync(state, CancellationToken.None);
        await _context.SaveChangesAsync();

        // Assert
        (await _context.UserTokenStates.FindAsync(state.Id))
            .Should()
            .NotBeNull();
    }

    [Fact]
    public async Task GetAsync_ForAnUnknownUser_ShouldReturnNull()
    {
        // Act
        UserSecurityState? state = await _repository.GetAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        state.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_ForAKnownUser_ShouldProjectTheStoredMarkers()
    {
        // Arrange
        var record = UserTokenStateEntity.Create(Guid.NewGuid());
        _context.UserTokenStates.Add(record);
        await _context.SaveChangesAsync();

        // Act
        UserSecurityState? state = await _repository.GetAsync(record.Id, CancellationToken.None);

        // Assert
        state.Should().NotBeNull();
        state!.Value.SecurityStamp.Should().Be(record.SecurityStamp);
        state.Value.TokenVersion.Should().Be(record.TokenVersion);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithAnExistingRecord_ShouldReturnItWithoutWritingTheCache()
    {
        // Arrange
        var record = UserTokenStateEntity.Create(Guid.NewGuid());
        _context.UserTokenStates.Add(record);
        await _context.SaveChangesAsync();

        // Act
        UserSecurityState state = await _repository.GetOrCreateAsync(record.Id, CancellationToken.None);

        // Assert
        state.SecurityStamp.Should().Be(record.SecurityStamp);
        _cacheMock.Verify(c => c.Set(It.IsAny<Guid>(), It.IsAny<UserSecurityState>()), Times.Never);
    }

    [Fact]
    public async Task GetOrCreateAsync_WithoutARecord_ShouldCreatePersistAndCacheIt()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        UserSecurityState state = await _repository.GetOrCreateAsync(userId, CancellationToken.None);

        // Assert
        UserTokenStateEntity? persisted = await _context.UserTokenStates.FindAsync(userId);
        persisted.Should().NotBeNull();
        persisted!.SecurityStamp.Should().Be(state.SecurityStamp);
        _cacheMock.Verify(c => c.Set(userId, state), Times.Once);
    }

    [Fact]
    public async Task BumpTokenVersionForRoleUsersAsync_WithNoUsersInTheRole_ShouldTouchNothing()
    {
        // Arrange — an unused role means there is no marker to bump and no cache entry to evict
        var record = UserTokenStateEntity.Create(Guid.NewGuid());
        _context.UserTokenStates.Add(record);
        await _context.SaveChangesAsync();

        // Act
        await _repository.BumpTokenVersionForRoleUsersAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        _cacheMock.Verify(c => c.Remove(It.IsAny<Guid>()), Times.Never);
        (await _context.UserTokenStates.FindAsync(record.Id))!.TokenVersion.Should().Be(record.TokenVersion);
    }
}
