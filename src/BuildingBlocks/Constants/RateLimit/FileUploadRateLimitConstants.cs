namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for file upload endpoints.
/// Uses Token Bucket algorithm to allow bursts while preventing sustained upload abuse.
/// Allows bursts of 10 uploads, then sustained rate of 2 per minute.
/// </summary>
public static class FileUploadRateLimitConstants
{
    /// <summary>
    /// Maximum number of tokens (requests) in the bucket.
    /// </summary>
    public const int TokenLimit = 10;

    /// <summary>
    /// Number of tokens to add per replenishment period.
    /// </summary>
    public const int TokensPerPeriod = 2;

    /// <summary>
    /// Time period for token replenishment in seconds (1 minute).
    /// </summary>
    public const int ReplenishmentPeriodSeconds = 60;
}
