using _116.Identity.Application.Session.Specifications;
using _116.Identity.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
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
        Guid userId = Guid.NewGuid();
        SessionEntity session = new SessionBuilder().WithUserId(userId).Build();
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
        SessionEntity session = new SessionBuilder().WithUserId(Guid.NewGuid()).Build();
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
        SessionEntity session = new SessionBuilder().WithRefreshTokenHash(refreshTokenHash).Build();
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
        SessionEntity session = new SessionBuilder().WithRefreshTokenHash("token-hash-123").Build();
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
        Guid sessionId = Guid.NewGuid();
        SessionEntity session = new SessionBuilder().WithId(sessionId).Build();
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
        SessionEntity session = new SessionBuilder().WithId(Guid.NewGuid()).Build();
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
        SessionEntity session = new SessionBuilder().WithDeviceId(deviceId).Build();
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
        SessionEntity session = new SessionBuilder().WithDeviceId("device-123").Build();
        SessionByDeviceIdSpecification spec = new("device-456");

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SessionByUserIdAndDeviceIdSpecification Tests

    [Fact]
    public void SessionByUserIdAndDeviceIdSpecification_WithMatchingBoth_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string deviceId = "device-123";
        SessionEntity session = new SessionBuilder()
            .WithUserId(userId)
            .WithDeviceId(deviceId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);

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
        SessionEntity session = new SessionBuilder()
            .WithUserId(Guid.NewGuid())
            .WithDeviceId(deviceId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);

        SessionByUserIdAndDeviceIdSpecification spec = new(Guid.NewGuid(), deviceId);

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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
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
        SessionEntity session = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
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
        SessionEntity session = new SessionBuilder()
            .WithRefreshTokenHash(refreshTokenHash)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        SessionEntity session = new SessionBuilder()
            .WithRefreshTokenHash(refreshTokenHash)
            .WithExpiresAt(DateTime.UtcNow.AddDays(-1))
            .Build();
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
        SessionEntity session = new SessionBuilder()
            .WithRefreshTokenHash(refreshTokenHash)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        SessionEntity session = new SessionBuilder()
            .WithRefreshTokenHash("token-hash-123")
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().WithIpAddress(ipAddress).Build();
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
        SessionEntity session = new SessionBuilder().WithIpAddress("192.168.1.100").Build();
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
        SessionEntity session = new SessionBuilder().WithIpAddress("192.168.1.100").Build();
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
        Guid userId = Guid.NewGuid();
        SessionEntity session = new SessionBuilder()
            .WithUserId(userId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        SessionEntity session = new SessionBuilder()
            .WithUserId(Guid.NewGuid())
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        Guid userId = Guid.NewGuid();
        SessionEntity session = new SessionBuilder()
            .WithUserId(userId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
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
        Guid userId = Guid.NewGuid();
        SessionEntity session = new SessionBuilder()
            .WithUserId(userId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(-1))
            .Build();
        session.GetType().GetProperty("IsRevoked")!.SetValue(session, false);
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        bool result = spec.IsSatisfiedBy(session);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LINQ Integration Tests

    [Fact]
    public void SessionByUserIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid targetUserId = Guid.NewGuid();
        List<SessionEntity> sessions =
        [
            new SessionBuilder().WithUserId(targetUserId).Build(),
            new SessionBuilder().WithUserId(targetUserId).Build(),
            new SessionBuilder().WithUserId(Guid.NewGuid()).Build(),
        ];

        SessionByUserIdSpecification spec = new(targetUserId);

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.All(s => s.UserId == targetUserId).Should().BeTrue();
    }

    [Fact]
    public void SessionIsActiveSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        SessionEntity activeSession = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
        activeSession.GetType().GetProperty("IsRevoked")!.SetValue(activeSession, false);

        SessionEntity expiredSession = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
        expiredSession.GetType().GetProperty("IsRevoked")!.SetValue(expiredSession, false);

        SessionEntity revokedSession = new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddDays(1)).Build();
        revokedSession.GetType().GetProperty("IsRevoked")!.SetValue(revokedSession, true);

        List<SessionEntity> sessions = [activeSession, expiredSession, revokedSession];
        SessionIsActiveSpecification spec = new();

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
    }

    [Fact]
    public void ActiveSessionsByUserIdSpecification_WithLinq_ShouldFilterCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        SessionEntity validSession = new SessionBuilder()
            .WithUserId(userId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
        validSession.GetType().GetProperty("IsRevoked")!.SetValue(validSession, false);

        SessionEntity otherUserSession = new SessionBuilder()
            .WithUserId(Guid.NewGuid())
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
        otherUserSession.GetType().GetProperty("IsRevoked")!.SetValue(otherUserSession, false);

        SessionEntity revokedSession = new SessionBuilder()
            .WithUserId(userId)
            .WithExpiresAt(DateTime.UtcNow.AddDays(1))
            .Build();
        revokedSession.GetType().GetProperty("IsRevoked")!.SetValue(revokedSession, true);

        List<SessionEntity> sessions = [validSession, otherUserSession, revokedSession];
        ActiveSessionsByUserIdSpecification spec = new(userId);

        // Act
        List<SessionEntity> filtered = sessions.Where(spec.ToExpression().Compile()).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered[0].UserId.Should().Be(userId);
    }

    #endregion
}
