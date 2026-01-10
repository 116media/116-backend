namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for data export endpoints (CSV, Excel).
/// Uses Token Bucket algorithm to prevent resource exhaustion from bulk exports.
/// Allows small bursts while limiting expensive export operations.
/// </summary>
public static class DataExportRateLimitConstants
{
    /// <summary>
    /// Maximum number of tokens (requests) in the bucket.
    /// </summary>
    public const int TokenLimit = 5;

    /// <summary>
    /// Number of tokens to add per replenishment period.
    /// </summary>
    public const int TokensPerPeriod = 1;

    /// <summary>
    /// Time period for token replenishment in seconds (1 minute).
    /// </summary>
    public const int ReplenishmentPeriodSeconds = 60;
}
