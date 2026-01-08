namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for content browsing endpoints (articles, videos, shows).
/// Uses Fixed Window algorithm for general DoS protection.
/// Generous limit for legitimate browsing behavior.
/// </summary>
public static class ContentBrowsingRateLimitConstants
{
    /// <summary>
    /// Maximum number of content browsing requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 100;

    /// <summary>
    /// Content browsing time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
