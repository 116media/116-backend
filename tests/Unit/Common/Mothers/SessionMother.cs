using _116.Identity.Domain.Entities;
using _116.Identity.Domain.Enums;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="SessionEntity"/> scenarios for testing.
/// </summary>
public static class SessionMother
{
    /// <summary>
    /// An active desktop session with Chrome browser.
    /// </summary>
    public static SessionEntity ActiveDesktop() =>
        new SessionBuilder()
            .AsDesktopSession()
            .WithIpAddress(TestConstants.Session.ValidIpAddress)
            .WithUserAgent(TestConstants.Session.ValidUserAgent)
            .Build();

    /// <summary>
    /// An active desktop session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static SessionEntity ActiveDesktop(Guid userId) =>
        new SessionBuilder()
            .WithUserId(userId)
            .AsDesktopSession()
            .WithIpAddress(TestConstants.Session.ValidIpAddress)
            .WithUserAgent(TestConstants.Session.ValidUserAgent)
            .Build();

    /// <summary>
    /// An active mobile session.
    /// </summary>
    public static SessionEntity ActiveMobile() =>
        new SessionBuilder().AsMobileSession().WithIpAddress(TestConstants.Session.ValidIpAddress).Build();

    /// <summary>
    /// An active mobile session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static SessionEntity ActiveMobile(Guid userId) =>
        new SessionBuilder()
            .WithUserId(userId)
            .AsMobileSession()
            .WithIpAddress(TestConstants.Session.ValidIpAddress)
            .Build();

    /// <summary>
    /// An expired session.
    /// </summary>
    public static SessionEntity Expired() => new SessionBuilder().AsExpired().Build();

    /// <summary>
    /// An expired session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static SessionEntity Expired(Guid userId) => new SessionBuilder().WithUserId(userId).AsExpired().Build();

    /// <summary>
    /// A revoked session (logged out).
    /// </summary>
    public static SessionEntity Revoked() => new SessionBuilder().AsRevoked().Build();

    /// <summary>
    /// A revoked session for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static SessionEntity Revoked(Guid userId) => new SessionBuilder().WithUserId(userId).AsRevoked().Build();

    /// <summary>
    /// A session about to expire (5 minutes remaining).
    /// </summary>
    public static SessionEntity AboutToExpire() =>
        new SessionBuilder().WithExpiresAt(DateTime.UtcNow.AddMinutes(5)).Build();

    /// <summary>
    /// A session about to expire for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static SessionEntity AboutToExpire(Guid userId) =>
        new SessionBuilder().WithUserId(userId).WithExpiresAt(DateTime.UtcNow.AddMinutes(5)).Build();

    /// <summary>
    /// A session from a tablet device.
    /// </summary>
    public static SessionEntity Tablet() =>
        new SessionBuilder()
            .WithDevice(EnumDevice.Tablet)
            .WithPlatform(EnumPlatform.IpadOs)
            .WithBrowser(EnumBrowser.Safari)
            .WithClient(EnumClient.MobileApp)
            .Build();

    /// <summary>
    /// A session from an unknown device.
    /// </summary>
    public static SessionEntity Unknown() =>
        new SessionBuilder()
            .WithDevice(EnumDevice.Unknown)
            .WithPlatform(EnumPlatform.Unknown)
            .WithBrowser(EnumBrowser.Unknown)
            .WithClient(EnumClient.Unknown)
            .Build();

    /// <summary>
    /// A valid session with known test data.
    /// </summary>
    public static SessionEntity Valid() =>
        new SessionBuilder()
            .WithDeviceId(TestConstants.Session.ValidDeviceId)
            .WithIpAddress(TestConstants.Session.ValidIpAddress)
            .WithUserAgent(TestConstants.Session.ValidUserAgent)
            .AsDesktopSession()
            .Build();

    /// <summary>
    /// A random session for general testing.
    /// </summary>
    public static SessionEntity Random() => new SessionBuilder().Build();

    /// <summary>
    /// Multiple active sessions for a user (simulating multi-device login).
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public static List<SessionEntity> MultiDeviceSessions(Guid userId) =>
        [
            new SessionBuilder().WithUserId(userId).AsDesktopSession().WithDeviceId("desktop-device-001").Build(),
            new SessionBuilder().WithUserId(userId).AsMobileSession().WithDeviceId("mobile-device-001").Build(),
            new SessionBuilder()
                .WithUserId(userId)
                .WithDevice(EnumDevice.Tablet)
                .WithPlatform(EnumPlatform.IpadOs)
                .WithDeviceId("tablet-device-001")
                .Build(),
        ];
}
