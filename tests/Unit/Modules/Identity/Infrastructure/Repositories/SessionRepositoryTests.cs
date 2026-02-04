using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="SessionRepository"/>.
/// </summary>
public class SessionRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly SessionRepository _repository;

    public SessionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new SessionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    private SessionEntity CreateSessionWithCreatedAt(Guid? userId = null)
    {
        var session = new SessionBuilder().WithUserId(userId ?? Guid.NewGuid()).Build();

        // Set CreatedAt using reflection to bypass SessionBuilder limitation
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow);

        return session;
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_ShouldAddSessionToContext()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();

        // Act
        await _repository.CreateAsync(session);
        await _context.SaveChangesAsync();

        // Assert
        var savedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        savedSession.Should().NotBeNull();
        savedSession!.Id.Should().Be(session.Id);
    }

    #endregion

    #region GetByRefreshTokenHashAsync Tests

    [Fact]
    public async Task GetByRefreshTokenHashAsync_WhenSessionExists_ShouldReturnSessionWithUserAndRoles()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var role = new RoleBuilder().WithName("Admin").Build();
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        var session = CreateSessionWithCreatedAt(user.Id);

        _context.Users.Add(user);
        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.UserRoles.Add(userRole);
        _context.RolePermissions.Add(rolePermission);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRefreshTokenHashAsync(session.RefreshTokenHash);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.User.Should().NotBeNull();
        result.User.UserRoles.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByRefreshTokenHashAsync_WhenSessionDoesNotExist_ShouldReturnNull()
    {
        // Arrange & Act
        var result = await _repository.GetByRefreshTokenHashAsync("nonexistent-hash");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByRefreshTokenHashAsync_WhenSessionIsRevoked_ShouldReturnNull()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var session = CreateSessionWithCreatedAt(user.Id);
        session.Revoke();

        _context.Users.Add(user);
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRefreshTokenHashAsync(session.RefreshTokenHash);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RevokeAsync Tests

    [Fact]
    public async Task RevokeAsync_WhenSessionExists_ShouldRevokeSession()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RevokeAsync(session.Id);
        await _context.SaveChangesAsync();

        // Assert
        var revokedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        revokedSession.Should().NotBeNull();
        revokedSession!.IsRevoked.Should().BeTrue();
        revokedSession.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeAsync_WhenSessionAlreadyRevoked_ShouldNotChangeSession()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        session.Revoke();
        var originalRevokedAt = session.RevokedAt;

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        await _repository.RevokeAsync(session.Id);
        await _context.SaveChangesAsync();

        // Assert
        var revokedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        revokedSession.Should().NotBeNull();
        revokedSession!.RevokedAt.Should().Be(originalRevokedAt);
    }

    [Fact]
    public async Task RevokeAsync_WhenSessionDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.RevokeAsync(nonExistentId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region DeleteAllByUserIdAsync Tests

    [Fact]
    public async Task DeleteAllByUserIdAsync_WhenUserHasActiveSessions_ShouldRevokeAllSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = CreateSessionWithCreatedAt(userId);
        var session2 = CreateSessionWithCreatedAt(userId);
        var session3 = CreateSessionWithCreatedAt(Guid.NewGuid()); // Different user

        _context.Sessions.AddRange(session1, session2, session3);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAllByUserIdAsync(userId);
        await _context.SaveChangesAsync();

        // Assert
        var userSessions = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync();
        userSessions.Should().HaveCount(2);
        userSessions.Should().AllSatisfy(s => s.IsRevoked.Should().BeTrue());

        var otherUserSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session3.Id);
        otherUserSession!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllByUserIdAsync_WhenUserHasNoSessions_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.DeleteAllByUserIdAsync(userId);

        // Assert
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region DeleteExpiredSessionsAsync Tests

    [Fact]
    public async Task DeleteExpiredSessionsAsync_WhenExpiredSessionsExist_ShouldRevokeThemAndReturnCount()
    {
        // Arrange
        var expiredSession1 = new SessionBuilder().AsExpired().Build();
        var expiredSession2 = new SessionBuilder().AsExpired().Build();
        var activeSession = CreateSessionWithCreatedAt();

        // Set CreatedAt for expired sessions
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(expiredSession1, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(expiredSession2, DateTime.UtcNow);

        _context.Sessions.AddRange(expiredSession1, expiredSession2, activeSession);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.DeleteExpiredSessionsAsync();
        await _context.SaveChangesAsync();

        // Assert
        count.Should().Be(2);

        var expired1 = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == expiredSession1.Id);
        expired1!.IsRevoked.Should().BeTrue();

        var expired2 = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == expiredSession2.Id);
        expired2!.IsRevoked.Should().BeTrue();

        var active = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == activeSession.Id);
        active!.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExpiredSessionsAsync_WhenNoExpiredSessions_ShouldReturnZero()
    {
        // Arrange
        var activeSession = CreateSessionWithCreatedAt();
        _context.Sessions.Add(activeSession);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.DeleteExpiredSessionsAsync();

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteExpiredSessionsAsync_ShouldNotRevokeAlreadyRevokedSessions()
    {
        // Arrange
        var expiredSession = new SessionBuilder().AsExpired().Build();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(expiredSession, DateTime.UtcNow);
        expiredSession.Revoke();

        _context.Sessions.Add(expiredSession);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.DeleteExpiredSessionsAsync();

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WhenSessionExists_ShouldReturnSession()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(session.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenSessionIsRevoked_ShouldReturnNull()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        session.Revoke();

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(session.Id);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetActiveSessionByUserIdAndDeviceIdAsync Tests

    [Fact]
    public async Task GetActiveSessionByUserIdAndDeviceIdAsync_WhenSessionExists_ShouldReturnSession()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device-123";
        var session = new SessionBuilder().WithUserId(userId).WithDeviceId(deviceId).Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow);

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionByUserIdAndDeviceIdAsync(userId, deviceId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
        result.UserId.Should().Be(userId);
        result.DeviceId.Should().Be(deviceId);
    }

    [Fact]
    public async Task GetActiveSessionByUserIdAndDeviceIdAsync_WhenSessionDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceId = "device-123";

        // Act
        var result = await _repository.GetActiveSessionByUserIdAndDeviceIdAsync(userId, deviceId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetUserSessionsAsync Tests

    [Fact]
    public async Task GetUserSessionsAsync_WithNoFilters_ShouldReturnAllUserSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = CreateSessionWithCreatedAt(userId);
        var session2 = CreateSessionWithCreatedAt(userId);
        var otherUserSession = CreateSessionWithCreatedAt(Guid.NewGuid());

        _context.Sessions.AddRange(session1, session2, otherUserSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserSessionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(s => s.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetUserSessionsAsync_WithActiveFilter_ShouldReturnOnlyActiveSessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeSession = CreateSessionWithCreatedAt(userId);
        var revokedSession = CreateSessionWithCreatedAt(userId);
        revokedSession.Revoke();

        _context.Sessions.AddRange(activeSession, revokedSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserSessionsAsync(userId, isActive: true);

        // Assert
        result.Should().HaveCount(1);
        result.First().Id.Should().Be(activeSession.Id);
    }

    [Fact]
    public async Task GetUserSessionsAsync_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = CreateSessionWithCreatedAt(userId);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session1, DateTime.UtcNow.AddMinutes(-10));

        var session2 = CreateSessionWithCreatedAt(userId);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session2, DateTime.UtcNow);

        _context.Sessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserSessionsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.First().Id.Should().Be(session2.Id);
    }

    #endregion

    #region GetAllWithPaginationAsync Tests

    [Fact]
    public async Task GetAllWithPaginationAsync_WithNoFilters_ShouldReturnAllSessions()
    {
        // Arrange
        var session1 = CreateSessionWithCreatedAt();
        var session2 = CreateSessionWithCreatedAt();
        var session3 = CreateSessionWithCreatedAt();

        _context.Sessions.AddRange(session1, session2, session3);
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        sessions.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            var session = CreateSessionWithCreatedAt();
            _context.Sessions.Add(session);
        }
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 2, pageSize: 2);

        // Assert
        sessions.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        var activeSession = CreateSessionWithCreatedAt();
        var expiredSession = new SessionBuilder().AsExpired().Build();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(expiredSession, DateTime.UtcNow);

        _context.Sessions.AddRange(activeSession, expiredSession);
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            status: "active"
        );

        // Assert
        sessions.Should().HaveCount(1);
        totalCount.Should().Be(1);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithUserIdFilter_ShouldFilterByUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userSession = CreateSessionWithCreatedAt(userId);
        var otherSession = CreateSessionWithCreatedAt(Guid.NewGuid());

        _context.Sessions.AddRange(userSession, otherSession);
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10, userId: userId);

        // Assert
        sessions.Should().HaveCount(1);
        totalCount.Should().Be(1);
        sessions.First().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithIpAddressFilter_ShouldFilterByIpAddress()
    {
        // Arrange
        var session1 = new SessionBuilder().WithIpAddress("192.168.1.1").Build();
        var session2 = new SessionBuilder().WithIpAddress("192.168.1.2").Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session1, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(session2, DateTime.UtcNow);

        _context.Sessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            ipAddress: "192.168.1.1"
        );

        // Assert
        sessions.Should().HaveCount(1);
        totalCount.Should().Be(1);
        sessions.First().IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithDateRangeFilter_ShouldFilterByDateRange()
    {
        // Arrange
        var oldSession = CreateSessionWithCreatedAt();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(oldSession, DateTime.UtcNow.AddDays(-10));

        var recentSession = CreateSessionWithCreatedAt();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(recentSession, DateTime.UtcNow);

        _context.Sessions.AddRange(oldSession, recentSession);
        await _context.SaveChangesAsync();

        // Act
        var (sessions, totalCount) = await _repository.GetAllWithPaginationAsync(
            page: 1,
            pageSize: 10,
            fromDate: DateTime.UtcNow.AddDays(-5),
            toDate: DateTime.UtcNow.AddDays(1)
        );

        // Assert
        sessions.Should().HaveCount(1);
        totalCount.Should().Be(1);
    }

    #endregion

    #region GetActiveSessionCountByBrowserAsync Tests

    [Fact]
    public async Task GetActiveSessionCountByBrowserAsync_ShouldReturnCountsByBrowser()
    {
        // Arrange
        var chromeSession1 = new SessionBuilder().WithBrowser(EnumBrowser.Chrome).Build();
        var chromeSession2 = new SessionBuilder().WithBrowser(EnumBrowser.Chrome).Build();
        var firefoxSession = new SessionBuilder().WithBrowser(EnumBrowser.Firefox).Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(chromeSession1, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(chromeSession2, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(firefoxSession, DateTime.UtcNow);

        _context.Sessions.AddRange(chromeSession1, chromeSession2, firefoxSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionCountByBrowserAsync();

        // Assert
        result.Should().ContainKey(EnumBrowser.Chrome);
        result[EnumBrowser.Chrome].Should().Be(2);
        result.Should().ContainKey(EnumBrowser.Firefox);
        result[EnumBrowser.Firefox].Should().Be(1);
    }

    [Fact]
    public async Task GetActiveSessionCountByBrowserAsync_ShouldNotIncludeRevokedSessions()
    {
        // Arrange
        var activeSession = new SessionBuilder().WithBrowser(EnumBrowser.Chrome).Build();
        var revokedSession = new SessionBuilder().WithBrowser(EnumBrowser.Chrome).Build();
        revokedSession.Revoke();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(activeSession, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(revokedSession, DateTime.UtcNow);

        _context.Sessions.AddRange(activeSession, revokedSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionCountByBrowserAsync();

        // Assert
        result[EnumBrowser.Chrome].Should().Be(1);
    }

    #endregion

    #region GetActiveSessionCountByDeviceAsync Tests

    [Fact]
    public async Task GetActiveSessionCountByDeviceAsync_ShouldReturnCountsByDevice()
    {
        // Arrange
        var desktopSession = new SessionBuilder().WithDevice(EnumDevice.Desktop).Build();
        var mobileSession = new SessionBuilder().WithDevice(EnumDevice.Mobile).Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(desktopSession, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(mobileSession, DateTime.UtcNow);

        _context.Sessions.AddRange(desktopSession, mobileSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionCountByDeviceAsync();

        // Assert
        result.Should().ContainKey(EnumDevice.Desktop);
        result[EnumDevice.Desktop].Should().Be(1);
        result.Should().ContainKey(EnumDevice.Mobile);
        result[EnumDevice.Mobile].Should().Be(1);
    }

    #endregion

    #region GetActiveSessionCountByPlatformAsync Tests

    [Fact]
    public async Task GetActiveSessionCountByPlatformAsync_ShouldReturnCountsByPlatform()
    {
        // Arrange
        var windowsSession = new SessionBuilder().WithPlatform(EnumPlatform.Windows).Build();
        var iosSession = new SessionBuilder().WithPlatform(EnumPlatform.Ios).Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(windowsSession, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(iosSession, DateTime.UtcNow);

        _context.Sessions.AddRange(windowsSession, iosSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionCountByPlatformAsync();

        // Assert
        result.Should().ContainKey(EnumPlatform.Windows);
        result[EnumPlatform.Windows].Should().Be(1);
        result.Should().ContainKey(EnumPlatform.Ios);
        result[EnumPlatform.Ios].Should().Be(1);
    }

    #endregion

    #region GetActiveSessionCountByClientAsync Tests

    [Fact]
    public async Task GetActiveSessionCountByClientAsync_ShouldReturnCountsByClient()
    {
        // Arrange
        var webAppSession = new SessionBuilder().WithClient(EnumClient.WebApp).Build();
        var mobileAppSession = new SessionBuilder().WithClient(EnumClient.MobileApp).Build();

        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(webAppSession, DateTime.UtcNow);
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(mobileAppSession, DateTime.UtcNow);

        _context.Sessions.AddRange(webAppSession, mobileAppSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveSessionCountByClientAsync();

        // Assert
        result.Should().ContainKey(EnumClient.WebApp);
        result[EnumClient.WebApp].Should().Be(1);
        result.Should().ContainKey(EnumClient.MobileApp);
        result[EnumClient.MobileApp].Should().Be(1);
    }

    #endregion

    #region GetTotalActiveSessionsCountAsync Tests

    [Fact]
    public async Task GetTotalActiveSessionsCountAsync_ShouldReturnCountOfActiveSessions()
    {
        // Arrange
        var activeSession1 = CreateSessionWithCreatedAt();
        var activeSession2 = CreateSessionWithCreatedAt();
        var revokedSession = CreateSessionWithCreatedAt();
        revokedSession.Revoke();

        _context.Sessions.AddRange(activeSession1, activeSession2, revokedSession);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetTotalActiveSessionsCountAsync();

        // Assert
        count.Should().Be(2);
    }

    #endregion

    #region GetTotalActiveUsersCountAsync Tests

    [Fact]
    public async Task GetTotalActiveUsersCountAsync_ShouldReturnCountOfDistinctActiveUsers()
    {
        // Arrange
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        var user1Session1 = CreateSessionWithCreatedAt(user1Id);
        var user1Session2 = CreateSessionWithCreatedAt(user1Id);
        var user2Session = CreateSessionWithCreatedAt(user2Id);

        _context.Sessions.AddRange(user1Session1, user1Session2, user2Session);
        await _context.SaveChangesAsync();

        // Act
        var count = await _repository.GetTotalActiveUsersCountAsync();

        // Assert
        count.Should().Be(2);
    }

    #endregion

    #region GetSessionsForExportAsync Tests

    [Fact]
    public async Task GetSessionsForExportAsync_WithNoFilters_ShouldReturnAllSessions()
    {
        // Arrange
        var session1 = CreateSessionWithCreatedAt();
        var session2 = CreateSessionWithCreatedAt();

        _context.Sessions.AddRange(session1, session2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionsForExportAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSessionsForExportAsync_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        var activeSession = CreateSessionWithCreatedAt();
        var revokedSession = CreateSessionWithCreatedAt();
        revokedSession.Revoke();

        _context.Sessions.AddRange(activeSession, revokedSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionsForExportAsync(status: "active");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSessionsForExportAsync_WithDateRangeFilter_ShouldFilterByDateRange()
    {
        // Arrange
        var oldSession = CreateSessionWithCreatedAt();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(oldSession, DateTime.UtcNow.AddDays(-10));

        var recentSession = CreateSessionWithCreatedAt();
        typeof(SessionEntity).GetProperty("CreatedAt")!.SetValue(recentSession, DateTime.UtcNow);

        _context.Sessions.AddRange(oldSession, recentSession);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetSessionsForExportAsync(
            fromDate: DateTime.UtcNow.AddDays(-5),
            toDate: DateTime.UtcNow.AddDays(1)
        );

        // Assert
        result.Should().HaveCount(1);
    }

    #endregion

    #region UpdateRefreshTokenAsync Tests

    [Fact]
    public async Task UpdateRefreshTokenAsync_WhenSessionExists_ShouldUpdateRefreshToken()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        var newHash = "new-hash";
        var newExpiresAt = DateTime.UtcNow.AddDays(60);

        // Act
        await _repository.UpdateRefreshTokenAsync(session.Id, newHash, newExpiresAt);
        await _context.SaveChangesAsync();

        // Assert
        var updatedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        updatedSession.Should().NotBeNull();
        updatedSession!.RefreshTokenHash.Should().Be(newHash);
        updatedSession.ExpiresAt.Should().BeCloseTo(newExpiresAt, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WhenSessionDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        Func<Task> act = async () =>
            await _repository.UpdateRefreshTokenAsync(nonExistentId, "new-hash", DateTime.UtcNow.AddDays(30));

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateRefreshTokenAsync_WhenSessionIsRevoked_ShouldNotUpdate()
    {
        // Arrange
        var session = CreateSessionWithCreatedAt();
        session.Revoke();
        var originalHash = session.RefreshTokenHash;

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateRefreshTokenAsync(session.Id, "new-hash", DateTime.UtcNow.AddDays(30));
        await _context.SaveChangesAsync();

        // Assert
        var updatedSession = await _context.Sessions.FirstOrDefaultAsync(s => s.Id == session.Id);
        updatedSession!.RefreshTokenHash.Should().Be(originalHash);
    }

    #endregion
}
