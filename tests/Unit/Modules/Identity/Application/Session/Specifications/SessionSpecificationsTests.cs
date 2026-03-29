using _116.Identity.Application.Session.Specifications;
using _116.Identity.Domain.Entities;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Session.Specifications;

/// <summary>
/// Unit tests for Session specifications.
/// </summary>
public class SessionSpecificationsTests
{
    #region SessionByUserIdSpecification Tests

    [Fact]
    public void SessionByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        SessionByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create(Guid.NewGuid());
        SessionByUserIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByRefreshTokenHashSpecification Tests

    [Fact]
    public void SessionByRefreshTokenHashSpecification_WithMatchingHash_ShouldReturnTrue()
    {
        // Arrange
        string refreshTokenHash = "token-hash-123";
        SessionEntity session = SessionFactory.CreateWithRefreshTokenHash(refreshTokenHash);
        SessionByRefreshTokenHashSpecification spec = new(refreshTokenHash);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByRefreshTokenHashSpecification_WithDifferentHash_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithRefreshTokenHash("token-hash-123");
        SessionByRefreshTokenHashSpecification spec = new("different-hash");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByIdSpecification Tests

    [Fact]
    public void SessionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateWithId(sessionId);
        SessionByIdSpecification spec = new(sessionId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithId(Guid.NewGuid());
        SessionByIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByDeviceIdSpecification Tests

    [Fact]
    public void SessionByDeviceIdSpecification_WithMatchingDeviceId_ShouldReturnTrue()
    {
        // Arrange
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(Guid.NewGuid(), deviceId);
        SessionByDeviceIdSpecification spec = new(deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByDeviceIdSpecification_WithDifferentDeviceId_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create(Guid.NewGuid(), "device-123");
        SessionByDeviceIdSpecification spec = new("device-456");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByUserIdAndDeviceIdSpecification Tests

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithMatchingUserIdAndDeviceId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(userId, deviceId);
        SessionByUserIdAndDeviceIdSpecification spec = new(userId, deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(Guid.NewGuid(), deviceId);
        SessionByUserIdAndDeviceIdSpecification spec = new(Guid.NewGuid(), deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithDifferentDeviceId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId, "device-123");
        SessionByUserIdAndDeviceIdSpecification spec = new(userId, "device-456");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithExpiredSession_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(userId, deviceId);
        session.GetType().GetProperty("ExpiresAt")!.SetValue(session, DateTime.UtcNow.AddDays(-1));
        SessionByUserIdAndDeviceIdSpecification spec = new(userId, deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithRevokedSession_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(userId, deviceId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        SessionByUserIdAndDeviceIdSpecification spec = new(userId, deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region SessionIsActiveByUserIdAndDeviceIdSpecification Tests

    [Fact]
    public void SessionIsActiveByUserIdAndDeviceIdSpecification_WithMatchingBoth_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(userId, deviceId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);

        SessionIsActiveByUserIdAndDeviceIdSpecification spec = new(userId, deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsActiveByUserIdAndDeviceIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        string deviceId = "device-123";
        SessionEntity session = SessionFactory.Create(Guid.NewGuid(), deviceId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);

        SessionIsActiveByUserIdAndDeviceIdSpecification spec = new(Guid.NewGuid(), deviceId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionIsRevokedSpecification Tests

    [Fact]
    public void SessionIsRevokedSpecification_WithRevokedSession_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        SessionIsRevokedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsRevokedSpecification_WithNonRevokedSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        SessionIsRevokedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionIsNotRevokedSpecification Tests

    [Fact]
    public void SessionIsNotRevokedSpecification_WithNonRevokedSession_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        SessionIsNotRevokedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsNotRevokedSpecification_WithRevokedSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        SessionIsNotRevokedSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionIsExpiredSpecification Tests

    [Fact]
    public void SessionIsExpiredSpecification_WithExpiredSession_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(-1));
        SessionIsExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsExpiredSpecification_WithNonExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        SessionIsExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionIsNotExpiredSpecification Tests

    [Fact]
    public void SessionIsNotExpiredSpecification_WithNonExpiredSession_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        SessionIsNotExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsNotExpiredSpecification_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(-1));
        SessionIsNotExpiredSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionIsActiveSpecification Tests

    [Fact]
    public void SessionIsActiveSpecification_WithActiveSession_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        SessionIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionIsActiveSpecification_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(-1));
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        SessionIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void SessionIsActiveSpecification_WithRevokedSession_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        SessionIsActiveSpecification spec = new();

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ValidRefreshTokenSessionSpecification Tests

    [Fact]
    public void ValidRefreshTokenSessionSpecification_WithValidSession_ShouldReturnTrue()
    {
        // Arrange
        string refreshTokenHash = "token-hash-123";
        SessionEntity session = SessionFactory.CreateWithRefreshTokenHash(refreshTokenHash);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ValidRefreshTokenSessionSpecification spec = new(refreshTokenHash);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidRefreshTokenSessionSpecification_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        string refreshTokenHash = "token-hash-123";
        SessionEntity session = SessionFactory.CreateExpiredWithRefreshTokenHash(refreshTokenHash);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ValidRefreshTokenSessionSpecification spec = new(refreshTokenHash);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidRefreshTokenSessionSpecification_WithRevokedSession_ShouldReturnFalse()
    {
        // Arrange
        string refreshTokenHash = "token-hash-123";
        SessionEntity session = SessionFactory.CreateWithRefreshTokenHash(refreshTokenHash);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        ValidRefreshTokenSessionSpecification spec = new(refreshTokenHash);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidRefreshTokenSessionSpecification_WithDifferentTokenHash_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithRefreshTokenHash("token-hash-123");
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ValidRefreshTokenSessionSpecification spec = new("different-hash");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionCreatedAfterSpecification Tests

    [Fact]
    public void SessionCreatedAfterSpecification_WithSessionCreatedAfterDate_ShouldReturnTrue()
    {
        // Arrange
        DateTime cutoffDate = DateTime.UtcNow.AddDays(-5);
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow.AddDays(-3));
        SessionCreatedAfterSpecification spec = new(cutoffDate);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionCreatedAfterSpecification_WithSessionCreatedBeforeDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime cutoffDate = DateTime.UtcNow.AddDays(-5);
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow.AddDays(-10));
        SessionCreatedAfterSpecification spec = new(cutoffDate);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionCreatedBeforeSpecification Tests

    [Fact]
    public void SessionCreatedBeforeSpecification_WithSessionCreatedBeforeDate_ShouldReturnTrue()
    {
        // Arrange
        DateTime cutoffDate = DateTime.UtcNow.AddDays(-5);
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow.AddDays(-10));
        SessionCreatedBeforeSpecification spec = new(cutoffDate);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionCreatedBeforeSpecification_WithSessionCreatedAfterDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime cutoffDate = DateTime.UtcNow.AddDays(-5);
        SessionEntity session = SessionFactory.Create();
        session.GetType().GetProperty("CreatedAt")!.SetValue(session, DateTime.UtcNow.AddDays(-3));
        SessionCreatedBeforeSpecification spec = new(cutoffDate);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByIpAddressSpecification Tests

    [Fact]
    public void SessionByIpAddressSpecification_WithMatchingIpAddress_ShouldReturnTrue()
    {
        // Arrange
        string ipAddress = "192.168.1.100";
        SessionEntity session = SessionFactory.CreateWithIpAddress(ipAddress);
        SessionByIpAddressSpecification spec = new(ipAddress);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByIpAddressSpecification_WithPartialMatch_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithIpAddress("192.168.1.100");
        SessionByIpAddressSpecification spec = new("192.168");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void SessionByIpAddressSpecification_WithDifferentIpAddress_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateWithIpAddress("192.168.1.100");
        SessionByIpAddressSpecification spec = new("10.0.0.1");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActiveSessionsByUserIdSpecification Tests

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithActiveSession_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create(Guid.NewGuid());
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ActiveSessionsByUserIdSpecification spec = new(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithRevokedSession_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.Create(userId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, true);
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithExpiredSession_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SessionEntity session = SessionFactory.CreateExpired(userId);
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByUserIdAndDeviceIdSpecification LINQ Integration Tests

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithLinq_ShouldReturnSessionsRegardlessOfStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        string deviceId = "device-123";

        SessionEntity activeSession = SessionFactory.Create(userId, deviceId);
        activeSession.GetType().GetProperty("IsRevoked")!.SetValue(activeSession, false);

        SessionEntity expiredSession = SessionFactory.Create(userId, deviceId);
        expiredSession.GetType().GetProperty("ExpiresAt")!.SetValue(expiredSession, DateTime.UtcNow.AddDays(-1));
        expiredSession.GetType().GetProperty("IsRevoked")!.SetValue(expiredSession, false);

        SessionEntity revokedSession = SessionFactory.Create(userId, deviceId);
        revokedSession.GetType().GetProperty("IsRevoked")!.SetValue(revokedSession, true);

        SessionEntity otherDeviceSession = SessionFactory.Create(userId, "device-other");
        SessionEntity otherUserSession = SessionFactory.Create(Guid.NewGuid(), deviceId);

        List<SessionEntity> sessions =
        [
            activeSession,
            expiredSession,
            revokedSession,
            otherDeviceSession,
            otherUserSession,
        ];
        SessionByUserIdAndDeviceIdSpecification spec = new(userId, deviceId);

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(3);
        filtered.Should().OnlyContain(s => s.UserId == userId && s.DeviceId == deviceId);
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void SessionByUserIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        var targetUserId = Guid.NewGuid();
        List<SessionEntity> sessions =
        [
            SessionFactory.Create(targetUserId),
            SessionFactory.Create(targetUserId),
            SessionFactory.Create(Guid.NewGuid()),
        ];

        SessionByUserIdSpecification spec = new(targetUserId);

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(s => s.UserId == targetUserId);
    }

    [Fact]
    public void SessionIsActiveSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        SessionEntity activeSession = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        activeSession.GetType().GetProperty("IsRevoked")!.SetValue(activeSession, false);

        SessionEntity expiredSession = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(-1));
        expiredSession.GetType().GetProperty("IsRevoked")!.SetValue(expiredSession, false);

        SessionEntity revokedSession = SessionFactory.CreateWithExpiresAt(DateTime.UtcNow.AddDays(1));
        revokedSession.GetType().GetProperty("IsRevoked")!.SetValue(revokedSession, true);

        List<SessionEntity> sessions = [activeSession, expiredSession, revokedSession];
        SessionIsActiveSpecification spec = new();

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
    }

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();

        SessionEntity validSession = SessionFactory.Create(userId);
        validSession.GetType().GetProperty("IsRevoked")!.SetValue(validSession, false);

        SessionEntity otherUserSession = SessionFactory.Create(Guid.NewGuid());
        otherUserSession.GetType().GetProperty("IsRevoked")!.SetValue(otherUserSession, false);

        SessionEntity revokedSession = SessionFactory.Create(userId);
        revokedSession.GetType().GetProperty("IsRevoked")!.SetValue(revokedSession, true);

        List<SessionEntity> sessions = [validSession, otherUserSession, revokedSession];
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().ContainSingle();
        filtered[0].UserId.Should().Be(userId);
    }

    #endregion
}
