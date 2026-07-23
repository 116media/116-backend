namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for authenticated content-contribution endpoints (translations,
/// revisions, votes, submissions). Uses Fixed Window algorithm.
/// Stricter than <see cref="ContentBrowsingRateLimitConstants"/> since these are write paths open
/// to any signed-in user, not just read traffic.
/// </summary>
public static class ContentContributionRateLimitConstants
{
    /// <summary>
    /// Maximum number of content-contribution requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 20;

    /// <summary>
    /// Content-contribution time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
