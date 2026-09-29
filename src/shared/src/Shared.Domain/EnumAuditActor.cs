namespace _116.Shared.Domain;

/// <summary>
/// Represents the non-user actors that can be written to audit fields
/// (<c>CreatedBy</c> / <c>UpdatedBy</c>) when no authenticated user is present.
/// </summary>
public enum EnumAuditActor
{
    /// <summary>
    /// The change originated from a background job, seeder, or migration —
    /// no HTTP context was available.
    /// </summary>
    System,

    /// <summary>
    /// The change originated from an unauthenticated HTTP request
    /// (e.i. public signup, public login).
    /// </summary>
    Anonymous,
}
