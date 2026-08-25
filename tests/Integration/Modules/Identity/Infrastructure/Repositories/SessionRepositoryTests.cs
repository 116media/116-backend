using _116.Identity.Application.Session.Repositories;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="ISessionRepository"/> verifying
/// session lifecycle operations, querying, pagination, and cleanup
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class SessionRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task CreateAsync_ShouldPersistSessionToDatabase()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ISessionRepository, IdentityDbContext>();
        var session = SessionFactory.Create(user.Id);

        await repo.CreateAsync(session);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.Sessions.FindAsync(session.Id);

        saved.Should().NotBeNull();
        saved!.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingSession_ShouldReturnSession()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var session = SessionFactory.Create(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var result = await repo.GetByIdAsync(session.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ShouldReturnNull()
    {
        var repo = Resolve<ISessionRepository>();

        var result = await repo.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByRefreshTokenHashAsync_MatchingHash_ShouldReturnSession()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var tokenHash = "test-refresh-token-hash-abc123";
        var session = SessionFactory.CreateWithRefreshTokenHash(user.Id, tokenHash);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var result = await repo.GetByRefreshTokenHashAsync(tokenHash);

        result.Should().NotBeNull();
        result!.RefreshTokenHash.Should().Be(tokenHash);
        result.UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task RevokeAsync_ExistingSession_ShouldSetIsRevokedAndRevokedAt()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var session = SessionFactory.Create(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ISessionRepository, IdentityDbContext>();

        await repo.RevokeAsync(session.Id);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var revoked = await verifyContext.Sessions.FindAsync(session.Id);

        revoked.Should().NotBeNull();
        revoked!.IsRevoked.Should().BeTrue();
        revoked.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_ShouldRevokeAllUserSessions()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var sessions = SessionFactory.CreateMany(user.Id, 3);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ISessionRepository, IdentityDbContext>();

        await repo.DeleteAllByUserIdAsync(user.Id);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var remaining = await verifyContext.Sessions.Where(s => s.UserId == user.Id).ToListAsync();

        remaining.Should().OnlyContain(s => s.IsRevoked);
    }

    [Fact]
    public async Task GetActiveSessionByUserIdAndDeviceIdAsync_ActiveSession_ShouldReturnSession()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var deviceId = "test-device-id-xyz";
        var session = SessionFactory.Create(user.Id, deviceId);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var result = await repo.GetActiveSessionByUserIdAndDeviceIdAsync(user.Id, deviceId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(user.Id);
        result.DeviceId.Should().Be(deviceId);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ShouldReturnAllSessionsForUser()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        var otherUser = UserFactory.CreateVerifiedActive();
        seedContext.Users.AddRange(user, otherUser);
        var userSessions = SessionFactory.CreateMany(user.Id, 3);
        var otherSession = SessionFactory.Create(otherUser.Id);
        seedContext.Sessions.AddRange(userSessions);
        seedContext.Sessions.Add(otherSession);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var result = await repo.GetUserSessionsAsync(user.Id);

        result.Should().HaveCount(3);
        result.Should().OnlyContain(s => s.UserId == user.Id);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_ShouldReturnPaginatedResults()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var sessions = SessionFactory.CreateMany(user.Id, 5);
        seedContext.Sessions.AddRange(sessions);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var (result, totalCount) = await repo.GetAllWithPaginationAsync(1, 3);

        totalCount.Should().Be(5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task DeleteExpiredSessionsAsync_ShouldRevokeOnlyExpiredSessions()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var activeSession = SessionFactory.Create(user.Id);
        var expiredSession = SessionFactory.CreateExpired(user.Id);
        seedContext.Sessions.AddRange(activeSession, expiredSession);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ISessionRepository, IdentityDbContext>();

        var deletedCount = await repo.DeleteExpiredSessionsAsync();
        await db.SaveChangesAsync();

        deletedCount.Should().Be(1);

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var active = await verifyContext.Sessions.FindAsync(activeSession.Id);
        active.Should().NotBeNull();
        active!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_ShouldUpdateTokenHashAndExpiry()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var session = SessionFactory.Create(user.Id);
        seedContext.Sessions.Add(session);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<ISessionRepository, IdentityDbContext>();
        var newHash = "updated-refresh-token-hash-999";
        var newExpiry = DateTime.UtcNow.AddDays(30);

        await repo.UpdateRefreshTokenAsync(session.Id, newHash, newExpiry);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var updated = await verifyContext.Sessions.FindAsync(session.Id);

        updated.Should().NotBeNull();
        updated!.RefreshTokenHash.Should().Be(newHash);
        updated.ExpiresAt.Should().BeCloseTo(newExpiry, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetTotalActiveSessionsCountAsync_ShouldReturnCorrectCount()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var user = UserFactory.CreateVerifiedActive();
        seedContext.Users.Add(user);
        var activeSessions = SessionFactory.CreateMany(user.Id, 3);
        var revokedSession = SessionFactory.CreateRevoked(user.Id);
        var expiredSession = SessionFactory.CreateExpired(user.Id);
        seedContext.Sessions.AddRange(activeSessions);
        seedContext.Sessions.AddRange(revokedSession, expiredSession);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<ISessionRepository>();

        var count = await repo.GetTotalActiveSessionsCountAsync();

        count.Should().Be(3);
    }
}
