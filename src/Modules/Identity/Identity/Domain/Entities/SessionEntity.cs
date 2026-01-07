using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents a login session for a user.
/// A session corresponds to a single authenticated device or browser instance.
/// Sessions can expire naturally or be explicitly revoked (e.i. logout, security action).
/// </summary>
public class SessionEntity : Aggregate<Guid>
{
    /// <summary>
    /// Identifier of the user this session belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

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
    public EnumClient Client { get; private set; }

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
    /// </summary>
    public static SessionEntity Create(
        Guid id,
        Guid userId,
        string refreshTokenHash,
        DateTime expiresAt,
        EnumBrowser browser,
        EnumDevice device,
        EnumPlatform platform,
        EnumClient client,
        string? ipAddress = null,
        string? userAgent = null
    )
    {
        return new SessionEntity
        {
            Id = id,
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
            Browser = browser,
            Device = device,
            Platform = platform,
            Client = client,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };
    }

    /// <summary>
    /// Determines whether the session is currently active.
    /// A session is active if it has not expired and has not been revoked.
    /// </summary>
    public bool IsActive()
    {
        return ExpiresAt > DateTime.UtcNow && !IsRevoked;
    }

    /// <summary>
    /// Rotates the refresh token and updates the session expiration.
    /// Typically called during token refresh.
    /// </summary>
    public void UpdateRefreshToken(string newRefreshTokenHash, DateTime newExpiresAt)
    {
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
    }

    /// <summary>
    /// Revokes this session.
    /// Used when a user logs out, a device is invalidated,
    /// or a security event requires terminating the session.
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}
