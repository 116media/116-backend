using System.ComponentModel.DataAnnotations;

using _116.BuildingBlocks.Constants;
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
    [MaxLength(SessionConstants.MaxRefreshTokenHashLength)]
    public string RefreshTokenHash { get; private set; } = null!;

    /// <summary>
    /// When this session expires. After this time, the user needs to log in again.
    /// </summary>
    public DateTime ExpiresAt { get; private set; }

    /// <summary>
    /// IP address where the login happened. Useful for security monitoring.
    /// </summary>
    [MaxLength(SessionConstants.MaxIpAddressLength)]
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Raw user agent string from the browser/device.
    /// </summary>
    [MaxLength(SessionConstants.MaxUserAgentLength)]
    public string? UserAgent { get; private set; }

    /// <summary>
    /// Friendly device name parsed from user agent (e.g., "Chrome on Windows", "Safari on iPhone").
    /// </summary>
    [MaxLength(SessionConstants.MaxDeviceNameLength)]
    public string? DeviceName { get; private set; }

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
        string? ipAddress = null,
        string? userAgent = null,
        string? deviceName = null
    )
    {
        return new SessionEntity
        {
            Id = id,
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceName = deviceName
        };
    }

    /// <summary>
    /// Checks if this session is still valid (not expired and not deleted).
    /// </summary>
    public bool IsActive() => ExpiresAt > DateTime.UtcNow && !IsDeleted;

    /// <summary>
    /// Updates the refresh token (for token rotation).
    /// </summary>
    public void UpdateRefreshToken(string newRefreshTokenHash, DateTime newExpiresAt)
    {
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
    }

    /// <summary>
    /// Soft deletes this session (for logout).
    /// Keeps the session data for analytics but marks it as deleted.
    /// </summary>
    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
