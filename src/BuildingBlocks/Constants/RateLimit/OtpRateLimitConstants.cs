namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for OTP endpoints (verify, resend).
/// Uses Sliding Window algorithm to prevent OTP brute force and enumeration attacks.
/// Most restrictive policy in the system.
/// </summary>
public static class OtpRateLimitConstants
{
    /// <summary>
    /// Maximum number of OTP requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 3;

    /// <summary>
    /// OTP time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;

    /// <summary>
    /// Number of segments per window for sliding calculation.
    /// </summary>
    public const int SegmentsPerWindow = 6;
}
