namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for user profile management endpoints.
/// Uses Fixed Window algorithm for moderate limits on user-specific operations.
/// </summary>
public static class UserProfileRateLimitConstants
{
    /// <summary>
    /// Maximum number of user profile requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 60;

    /// <summary>
    /// User profile time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
