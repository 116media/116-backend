namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for authentication endpoints (login, signup, social-login).
/// Uses Sliding Window algorithm to prevent brute force and credential stuffing attacks.
/// </summary>
public static class AuthenticationRateLimitConstants
{
    /// <summary>
    /// Maximum number of authentication requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 5;

    /// <summary>
    /// Authentication time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;

    /// <summary>
    /// Number of segments per window for sliding calculation.
    /// 6 segments = check every 10 seconds for smooth sliding behavior.
    /// </summary>
    public const int SegmentsPerWindow = 6;
}
