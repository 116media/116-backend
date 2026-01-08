namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for password management endpoints (forgot, reset, change, set).
/// Uses Sliding Window algorithm to prevent password reset abuse and email flooding.
/// </summary>
public static class PasswordManagementRateLimitConstants
{
    /// <summary>
    /// Maximum number of password management requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 3;

    /// <summary>
    /// Password management time window duration in seconds (5 minutes).
    /// Longer window to prevent email flooding and password reset abuse.
    /// </summary>
    public const int WindowSeconds = 300;

    /// <summary>
    /// Number of segments per window for sliding calculation.
    /// 5 segments = check every 60 seconds.
    /// </summary>
    public const int SegmentsPerWindow = 5;
}
