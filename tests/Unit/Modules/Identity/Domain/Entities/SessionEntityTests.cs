using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Tests.Fixtures.Builders.Entities.Identity;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Identity;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="SessionEntity"/>.
/// </summary>
public class SessionEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateSession()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string deviceId = TestConstants.Session.ValidDeviceId;
        string refreshTokenHash = TestConstants.Session.DefaultRefreshTokenHash;
        DateTime expiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultRefreshTokenExpirationDays);
        DateTime absoluteExpiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultAbsoluteLifetimeDays);
        var browser = EnumBrowser.Chrome;
        var device = EnumDevice.Desktop;
        var platform = EnumPlatform.Windows;
        var client = EnumClient.WebApp;
        string ipAddress = TestConstants.Session.ValidIpAddress;
        string userAgent = TestConstants.Session.ValidUserAgent;

        // Act
        var session = SessionEntity.Create(
            id,
            userId,
            deviceId,
            refreshTokenHash,
            expiresAt,
            absoluteExpiresAt,
            browser,
            device,
            platform,
            client,
            ipAddress,
            userAgent
        );

        // Assert
        session.Id.Should().Be(id);
        session.UserId.Should().Be(userId);
        session.DeviceId.Should().Be(deviceId);
        session.RefreshTokenHash.Should().Be(refreshTokenHash);
        session.ExpiresAt.Should().Be(expiresAt);
        session.AbsoluteExpiresAt.Should().Be(absoluteExpiresAt);
        session.Browser.Should().Be(browser);
        session.Device.Should().Be(device);
        session.Platform.Should().Be(platform);
        session.Client.Should().Be(client);
        session.IpAddress.Should().Be(ipAddress);
        session.UserAgent.Should().Be(userAgent);
        session.IsRevoked.Should().BeFalse();
        session.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithoutOptionalParameters_ShouldCreateSessionWithNulls()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        string deviceId = TestConstants.Session.ValidDeviceId;
        string refreshTokenHash = TestConstants.Session.DefaultRefreshTokenHash;
        DateTime expiresAt = DateTime.UtcNow.AddDays(30);
        DateTime absoluteExpiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultAbsoluteLifetimeDays);

        // Act
        var session = SessionEntity.Create(
            id,
            userId,
            deviceId,
            refreshTokenHash,
            expiresAt,
            absoluteExpiresAt,
            EnumBrowser.Chrome,
            EnumDevice.Desktop,
            EnumPlatform.Windows,
            EnumClient.WebApp
        );

        // Assert
        session.IpAddress.Should().BeNull();
        session.UserAgent.Should().BeNull();
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateExpired();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateRevoked();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpiredAndRevoked_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateExpired();
        session.Revoke();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateRefreshToken Tests

    [Fact]
    public void UpdateRefreshToken_ShouldUpdateTokenAndExpiration()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        string newRefreshTokenHash = "new_refresh_token_hash_value";
        DateTime newExpiresAt = DateTime.UtcNow.AddDays(60);
        DateTime originalAbsoluteExpiresAt = session.AbsoluteExpiresAt;

        // Act
        session.UpdateRefreshToken(newRefreshTokenHash, newExpiresAt);

        // Assert
        session.RefreshTokenHash.Should().Be(newRefreshTokenHash);
        session.ExpiresAt.Should().Be(newExpiresAt);
        session.AbsoluteExpiresAt.Should().Be(originalAbsoluteExpiresAt);
    }

    [Fact]
    public void UpdateRefreshToken_ShouldExtendExpiredSession()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateExpired();
        string newRefreshTokenHash = "new_refresh_token_hash";
        DateTime newExpiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        session.UpdateRefreshToken(newRefreshTokenHash, newExpiresAt);

        // Assert
        session.RefreshTokenHash.Should().Be(newRefreshTokenHash);
        session.ExpiresAt.Should().Be(newExpiresAt);
        // Note: A revoked session would still be revoked even with a new expiration
    }

    #endregion

    #region Absolute Expiry Tests

    [Fact]
    public void HasReachedAbsoluteExpiry_WhenCeilingIsInTheFuture_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().WithAbsoluteExpiresAt(DateTime.UtcNow.AddDays(1)).Build();

        // Act
        bool result = session.HasReachedAbsoluteExpiry();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasReachedAbsoluteExpiry_WhenCeilingHasPassed_ShouldReturnTrue()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().WithAbsoluteExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();

        // Act
        bool result = session.HasReachedAbsoluteExpiry();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Reactivate_ShouldResetAbsoluteExpiry()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().WithAbsoluteExpiresAt(DateTime.UtcNow.AddDays(-1)).Build();
        DateTime newAbsoluteExpiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultAbsoluteLifetimeDays);

        // Act
        session.Reactivate("new_hash", DateTime.UtcNow.AddDays(30), newAbsoluteExpiresAt);

        // Assert
        session.AbsoluteExpiresAt.Should().Be(newAbsoluteExpiresAt);
        session.HasReachedAbsoluteExpiry().Should().BeFalse();
    }

    #endregion

    #region Revoke Tests

    [Fact]
    public void Revoke_ShouldSetIsRevokedAndRevokedAt()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();

        // Act
        session.Revoke();

        // Assert
        session.IsRevoked.Should().BeTrue();
        session.RevokedAt.Should().NotBeNull();
        session.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ShouldUpdateRevokedAt()
    {
        // Arrange
        SessionEntity session = SessionFactory.CreateRevoked();
        DateTime? originalRevokedAt = session.RevokedAt;

        // Act
        session.Revoke();

        // Assert
        session.IsRevoked.Should().BeTrue();
        session.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Revoke_ShouldMakeSessionInactive()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.IsActive().Should().BeTrue(); // Pre-condition

        // Act
        session.Revoke();

        // Assert
        session.IsActive().Should().BeFalse();
    }

    #endregion

    #region Session Type Tests

    [Fact]
    public void Create_MobileSession_ShouldHaveMobileProperties()
    {
        // Arrange & Act
        SessionEntity session = SessionFactory.CreateMobile();

        // Assert
        session.Device.Should().Be(EnumDevice.Mobile);
        session.Client.Should().Be(EnumClient.MobileApp);
    }

    [Fact]
    public void Create_DesktopSession_ShouldHaveDesktopProperties()
    {
        // Arrange & Act
        SessionEntity session = SessionFactory.CreateDesktop();

        // Assert
        session.Device.Should().Be(EnumDevice.Desktop);
        session.Client.Should().Be(EnumClient.WebApp);
    }

    #endregion

    #region Domain Event Tests

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Create_ShouldRaiseSessionCreatedEventWithNewDeviceFlag(bool isNewDevice)
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // Act
        var session = SessionEntity.Create(
            id: id,
            userId: userId,
            deviceId: TestConstants.Session.ValidDeviceId,
            refreshTokenHash: TestConstants.Session.DefaultRefreshTokenHash,
            expiresAt: DateTime.UtcNow.AddDays(TestConstants.Session.DefaultRefreshTokenExpirationDays),
            absoluteExpiresAt: DateTime.UtcNow.AddDays(TestConstants.Session.DefaultAbsoluteLifetimeDays),
            browser: EnumBrowser.Chrome,
            device: EnumDevice.Desktop,
            platform: EnumPlatform.Windows,
            client: EnumClient.WebApp,
            isNewDevice: isNewDevice
        );

        // Assert
        SessionCreatedEvent raised = session.DomainEvents.OfType<SessionCreatedEvent>().Single();
        raised.SessionId.Should().Be(id);
        raised.UserId.Should().Be(userId);
        raised.IsNewDevice.Should().Be(isNewDevice);
    }

    [Fact]
    public void Revoke_ShouldRaiseSessionRevokedEventWithReason()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.ClearDomainEvents();

        // Act
        session.Revoke(EnumSessionRevokeReason.SecurityInvalidation);

        // Assert
        SessionRevokedEvent raised = session.DomainEvents.OfType<SessionRevokedEvent>().Single();
        raised.SessionId.Should().Be(session.Id);
        raised.UserId.Should().Be(session.UserId);
        raised.Reason.Should().Be(EnumSessionRevokeReason.SecurityInvalidation);
    }

    [Fact]
    public void Revoke_WithoutReason_ShouldDefaultToSelfSignOut()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.ClearDomainEvents();

        // Act
        session.Revoke();

        // Assert
        session
            .DomainEvents.OfType<SessionRevokedEvent>()
            .Single()
            .Reason.Should()
            .Be(EnumSessionRevokeReason.SelfSignOut);
    }

    [Fact]
    public void Reactivate_ShouldRaiseSessionReactivatedEvent()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.ClearDomainEvents();

        // Act
        session.Reactivate("new_hash", DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(60));

        // Assert
        SessionReactivatedEvent raised = session.DomainEvents.OfType<SessionReactivatedEvent>().Single();
        raised.SessionId.Should().Be(session.Id);
        raised.UserId.Should().Be(session.UserId);
    }

    [Fact]
    public void RecordRefreshTokenReplay_ShouldRaiseRefreshTokenReplayDetectedEvent()
    {
        // Arrange
        SessionEntity session = SessionFactory.Create();
        session.ClearDomainEvents();

        // Act
        session.RecordRefreshTokenReplay();

        // Assert
        RefreshTokenReplayDetectedEvent raised = session
            .DomainEvents.OfType<RefreshTokenReplayDetectedEvent>()
            .Single();
        raised.SessionId.Should().Be(session.Id);
        raised.UserId.Should().Be(session.UserId);
    }

    #endregion
}
