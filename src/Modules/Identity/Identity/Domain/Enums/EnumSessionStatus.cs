namespace _116.Identity.Domain.Enums;

/// <summary>
/// Defines the possible status values for a session.
/// </summary>
public enum EnumSessionStatus
{
    /// <summary>
    /// Represents an active session (not deleted and not expired).
    /// </summary>
    Active,

    /// <summary>
    /// Represents an expired session (past expiration date).
    /// </summary>
    Expired,
}
