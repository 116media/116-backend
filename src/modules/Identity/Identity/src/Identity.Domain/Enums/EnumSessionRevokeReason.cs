namespace _116.Identity.Domain.Enums;

/// <summary>
/// Identifies why a session was revoked. Carried by the session revocation
/// event so future consumers (denylist, audit trail, push notifications) can
/// react per cause without re-deriving it.
/// </summary>
public enum EnumSessionRevokeReason
{
    /// <summary>
    /// The user ended the session themselves (sign-out or own-session revoke).
    /// </summary>
    SelfSignOut,

    /// <summary>
    /// An administrator terminated the session.
    /// </summary>
    AdminRevoke,

    /// <summary>
    /// A security reaction terminated the session after a credential or
    /// authorization change, or after suspicious token activity.
    /// </summary>
    SecurityInvalidation,

    /// <summary>
    /// The session passed its expiry and was revoked by cleanup.
    /// </summary>
    Expiry,
}
