namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Rate limiting configuration for session management endpoints.
/// Uses Fixed Window algorithm to prevent session enumeration and abuse.
/// </summary>
public static class SessionManagementRateLimitConstants
{
    /// <summary>
    /// Maximum number of session management requests allowed in the time window.
    /// </summary>
    public const int PermitLimit = 30;

    /// <summary>
    /// Session management time window duration in seconds (1 minute).
    /// </summary>
    public const int WindowSeconds = 60;
}
