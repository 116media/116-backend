namespace _116.Identity.Application.Shared.Cache;

/// <summary>
/// Tag names shared by the queries that cache a result and the event handlers that evict it.
/// </summary>
public static class IdentityCacheTags
{
    /// <summary>
    /// The role and permission lookup tables, including role-permission assignments.
    /// </summary>
    public const string Lookups = "identity:lookups";
}
