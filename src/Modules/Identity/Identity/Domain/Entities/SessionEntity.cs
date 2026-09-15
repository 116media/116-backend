using System.ComponentModel.DataAnnotations;
using _116.BuildingBlocks.Constants;
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
public class SessionEntity : Aggregate<Guid>
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

    /// <summary>
    /// Determines whether the session is active at the supplied instant: not expired, not revoked.
    /// </summary>
    /// <param name="now">The current UTC instant.</param>
    public bool IsActive(DateTime now)
    {
        return ExpiresAt > now && !IsRevoked;
    }

    /// <summary>
    /// Rotates the refresh token and updates the session expiration. The absolute expiry is
    /// deliberately left untouched.
    /// </summary>
    public void UpdateRefreshToken(string newRefreshTokenHash, DateTime newExpiresAt)
    {
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
    }

    /// <summary>
    /// Determines whether the session has reached its absolute lifetime ceiling.
    /// </summary>
    /// <param name="now">The current UTC instant.</param>
    /// <returns>True once the absolute expiry has passed.</returns>
    public bool HasReachedAbsoluteExpiry(DateTime now)
    {
        return now >= AbsoluteExpiresAt;
    }

    /// <summary>
    /// Revokes this session and raises <see cref="SessionRevokedEvent" /> carrying the cause.
    /// Idempotent: a session already revoked reports <c>false</c>, keeps its original stamp and
    /// raises nothing.
    /// </summary>
    /// <param name="reason">Why the session is being revoked.</param>
    /// <param name="now">The current UTC instant, stamped as the revocation time.</param>
    /// <returns><c>true</c> if the session transitioned; <c>false</c> if already revoked.</returns>
    public bool Revoke(EnumSessionRevokeReason reason, DateTime now)
    {
        if (IsRevoked)
        {
            return false;
        }

        IsRevoked = true;
        RevokedAt = now;

        AddDomainEvent(new SessionRevokedEvent(UserId: UserId, SessionId: Id, Reason: reason));
        return true;
    }

    /// <summary>
    /// Renews the session's lease with a new refresh token, expiry and absolute expiry, reusing
    /// the row to avoid unique-constraint violations on (user_id, device_id). Raises
    /// <see cref="SessionReactivatedEvent" /> only when this genuinely revived a revoked session;
    /// renewing an active session reports <c>false</c> and raises nothing.
    /// </summary>
    /// <returns><c>true</c> if a revoked session was revived; <c>false</c> for a plain renewal.</returns>
    public bool Reactivate(string newRefreshTokenHash, DateTime newExpiresAt, DateTime newAbsoluteExpiresAt)
    {
        RefreshTokenHash = newRefreshTokenHash;
        ExpiresAt = newExpiresAt;
        AbsoluteExpiresAt = newAbsoluteExpiresAt;

        if (!IsRevoked)
        {
            return false;
        }

        IsRevoked = false;
        RevokedAt = null;

        AddDomainEvent(new SessionReactivatedEvent(SessionId: Id, UserId: UserId));
        return true;
    }

    /// <summary>
    /// Records that a refresh token belonging to this already-revoked session was presented again
    /// by raising <see cref="RefreshTokenReplayDetectedEvent" />. Consumers revoke the account's
    /// remaining sessions and alert the owner; the caller still rejects the refresh attempt.
    /// </summary>
    public void RecordRefreshTokenReplay()
    {
        AddDomainEvent(new RefreshTokenReplayDetectedEvent(UserId: UserId, SessionId: Id));
    }
}
