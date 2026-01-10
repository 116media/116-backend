namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for admin metrics and analytics endpoints.
/// Uses Fixed Window algorithm to prevent expensive query abuse.
/// </summary>
public static class AdminMetricsRateLimitConstants
{
    /// <summary>
    /// Maximum number of admin metrics requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 30;

    /// <summary>
    /// Admin metrics time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
