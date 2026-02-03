using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;
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
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string deviceId = TestConstants.Session.ValidDeviceId;
        string refreshTokenHash = TestConstants.Session.DefaultRefreshTokenHash;
        DateTime expiresAt = DateTime.UtcNow.AddDays(TestConstants.Session.DefaultRefreshTokenExpirationDays);
        EnumBrowser browser = EnumBrowser.Chrome;
        EnumDevice device = EnumDevice.Desktop;
        EnumPlatform platform = EnumPlatform.Windows;
        EnumClient client = EnumClient.WebApp;
        string ipAddress = TestConstants.Session.ValidIpAddress;
        string userAgent = TestConstants.Session.ValidUserAgent;

        // Act
        SessionEntity session = SessionEntity.Create(
            id,
            userId,
            deviceId,
            refreshTokenHash,
            expiresAt,
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
        Guid id = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        string deviceId = TestConstants.Session.ValidDeviceId;
        string refreshTokenHash = TestConstants.Session.DefaultRefreshTokenHash;
        DateTime expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        SessionEntity session = SessionEntity.Create(
            id,
            userId,
            deviceId,
            refreshTokenHash,
            expiresAt,
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
        SessionEntity session = new SessionBuilder().Build();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().AsExpired().Build();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().AsRevoked().Build();

        // Act
        bool result = session.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenExpiredAndRevoked_ShouldReturnFalse()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().AsExpired().AsRevoked().Build();

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
        SessionEntity session = new SessionBuilder().Build();
        string newRefreshTokenHash = "new_refresh_token_hash_value";
        DateTime newExpiresAt = DateTime.UtcNow.AddDays(60);

        // Act
        session.UpdateRefreshToken(newRefreshTokenHash, newExpiresAt);

        // Assert
        session.RefreshTokenHash.Should().Be(newRefreshTokenHash);
        session.ExpiresAt.Should().Be(newExpiresAt);
    }

    [Fact]
    public void UpdateRefreshToken_ShouldExtendExpiredSession()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().AsExpired().Build();
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

    #region Revoke Tests

    [Fact]
    public void Revoke_ShouldSetIsRevokedAndRevokedAt()
    {
        // Arrange
        SessionEntity session = new SessionBuilder().Build();

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
        SessionEntity session = new SessionBuilder().AsRevoked().Build();
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
        SessionEntity session = new SessionBuilder().Build();
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
        SessionEntity session = new SessionBuilder().AsMobileSession().Build();

        // Assert
        session.Device.Should().Be(EnumDevice.Mobile);
        session.Client.Should().Be(EnumClient.MobileApp);
    }

    [Fact]
    public void Create_DesktopSession_ShouldHaveDesktopProperties()
    {
        // Arrange & Act
        SessionEntity session = new SessionBuilder().AsDesktopSession().Build();

        // Assert
        session.Device.Should().Be(EnumDevice.Desktop);
        session.Client.Should().Be(EnumClient.WebApp);
    }

    #endregion
}
