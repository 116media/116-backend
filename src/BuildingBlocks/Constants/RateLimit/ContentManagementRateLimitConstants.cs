namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for admin content-management writes (create, update, publish,
/// delete). Uses Fixed Window algorithm.
/// Looser than <see cref="ContentContributionRateLimitConstants"/> because editors work in bursts
/// behind an admin role, and tighter than <see cref="ContentBrowsingRateLimitConstants"/> because
/// each request mutates state.
/// </summary>
public static class ContentManagementRateLimitConstants
{
    /// <summary>
    /// Maximum number of admin content-management requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 60;

    /// <summary>
    /// Admin content-management time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
