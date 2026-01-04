using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
using _116.Identity.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Represents an active login session for a user.
/// Sessions track when and where users are logged in - each device/browser gets its own session.
/// </summary>
public class SessionEntity : Aggregate<Guid>
{
    /// <summary>
    /// Which user this session belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Hashed refresh token. Never store the raw token - hash it first!
    /// </summary>
    [MaxLength(length: SessionConstants.MaxRefreshTokenHashLength)]
    public string RefreshTokenHash { get; private set; } = null!;

    /// <summary>
    /// When this session expires. After this time, the user needs to log in again.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// IP address where the login happened. Useful for security monitoring.
    /// </summary>
    [MaxLength(length: SessionConstants.MaxIpAddressLength)]
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Raw user agent string from the browser/device.
    /// </summary>
    [MaxLength(length: SessionConstants.MaxUserAgentLength)]
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Browser type detected from the user agent (e.g., Chrome, Firefox, Safari).
    /// </summary>
    public EnumBrowser Browser { get; private set; }

    /// <summary>
    /// Device type detected from the user agent (e.g., Desktop, Mobile, Tablet).
    /// </summary>
    public EnumDevice Device { get; private set; }

    /// <summary>
    /// Platform/OS detected from the user agent (e.g., Windows, iOS, Android).
    /// </summary>
    public EnumPlatform Platform { get; private set; }

    /// <summary>
    /// Client application type that initiated the session (e.g., MobileApp, WebApp, Dashboard).
    /// Sent from the application via Client-App header.
    /// </summary>
    public EnumClient Client { get; private set; }

    /// <summary>
    /// Whether this session has been deleted (logged out/revoked).
    /// Sessions are soft-deleted to keep historical data for analytics.
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// When this session was deleted (for logout/revocation). Null if not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; private set; }

    /// <summary>
    /// Navigation property back to the user.
    /// </summary>
    public UserEntity User { get; private set; } = null!;

    /// <summary>
    /// Creates a new session when a user logs in.
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
    /// Checks if this session is still valid (not expired and not deleted).
    /// </summary>
    public bool IsActive()
    {
        return ExpiresAt > DateTime.UtcNow && !IsDeleted;
    }

    /// <summary>
    /// Updates the refresh token (for token rotation).
    /// </summary>
    public void UpdateRefreshToken(string newRefreshTokenHash, DateTime newExpiresAt)
    {
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
    }

    /// <summary>
    /// Softly deletes this session (for logout).
    /// Keeps the session data for analytics but marks it as deleted.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
