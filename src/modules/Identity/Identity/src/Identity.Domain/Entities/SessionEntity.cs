using System.ComponentModel.DataAnnotations;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a login session for a user.
/// A session corresponds to a single authenticated device or browser instance.
/// Sessions can expire naturally or be explicitly revoked (e.i. logout, security action).
/// </summary>
public partial class SessionEntity : Aggregate<Guid>
{
    /// <summary>
    /// A unique identifier of the user this session belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Unique identifier of the device or browser instance.
    /// Generated once per installation and used to prevent multiple sessions per device.
    /// Supports GUID, UUID, or NanoID formats.
    /// </summary>
    [MaxLength(SessionConstants.MaxDeviceIdLength)]
    public string DeviceId { get; private set; } = null!;

    /// <summary>
    /// Hashed refresh token associated with this session.
    /// The raw refresh token must never be stored.
    /// </summary>
    [MaxLength(SessionConstants.MaxRefreshTokenHashLength)]
    public string RefreshTokenHash { get; private set; } = null!;

    /// <summary>
    /// UTC timestamp indicating when this session expires.
    /// After expiration, the session is no longer considered valid.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// UTC timestamp beyond which token refreshes can no longer extend this session.
    /// </summary>
    public DateTime AbsoluteExpiresAt { get; private set; }

    /// <summary>
    /// IP address from which the session was created.
    /// Useful for security auditing and anomaly detection.
    /// </summary>
    [MaxLength(SessionConstants.MaxIpAddressLength)]
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Raw user-agent string reported by the client.
    /// </summary>
    [MaxLength(SessionConstants.MaxUserAgentLength)]
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Browser detected from the user agent (e.g., Chrome, Firefox, Safari).
    /// </summary>
    public EnumBrowser Browser { get; private set; }

    /// <summary>
    /// Device category associated with this session (e.g., Desktop, Mobile, Tablet).
    /// </summary>
    public EnumDevice Device { get; private set; }

    /// <summary>
    /// Operating system or platform detected for this session (e.g., Windows, iOS, Android).
    /// </summary>
    public EnumPlatform Platform { get; private set; }

    /// <summary>
    /// Client application that initiated the session
    /// (e.g., MobileApp, WebApp, Dashboard).
    /// </summary>
    public Client Client { get; private set; } = null!;

    /// <summary>
    /// Indicates whether this session has been explicitly revoked.
    /// A revoked session is no longer valid even if it has not expired.
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// UTC timestamp indicating when the session was revoked.
    /// Null if the session has not been revoked.
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// Navigation property to the owning user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    /// <summary>
    /// Creates a new session when a user successfully logs in.
    /// Raises <see cref="SessionCreatedEvent" /> carrying the new-device flag computed where the
    /// reuse-or-create decision is made; a session row is only ever created when no row exists for
    /// the device, so the flag defaults to true.
    /// </summary>
    public static SessionEntity Create(
        Guid id,
        Guid userId,
        string deviceId,
        string refreshTokenHash,
        DateTime expiresAt,
        DateTime absoluteExpiresAt,
        EnumBrowser browser,
        EnumDevice device,
        EnumPlatform platform,
        EnumClient client,
        string? ipAddress = null,
        string? userAgent = null,
        bool isNewDevice = true
    )
    {
        var session = new SessionEntity
        {
            Id = id,
            UserId = userId,
            DeviceId = deviceId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
            AbsoluteExpiresAt = absoluteExpiresAt,
            Browser = browser,
            Device = device,
            Platform = platform,
            Client = new Client(value: client),
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };

        session.AddDomainEvent(new SessionCreatedEvent(SessionId: id, UserId: userId, IsNewDevice: isNewDevice));

        return session;
    }
}
