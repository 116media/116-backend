using _116.Identity.Domain.Enums;
using _116.Identity.Domain.Events;

namespace _116.Identity.Domain.Entities;

/// <summary>
/// Session behaviour of <see cref="SessionEntity" />. Its state lives in <c>Entities/SessionEntity.cs</c>.
/// </summary>
public partial class SessionEntity
{
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
