namespace _116.BuildingBlocks.Constants.RateLimit;

/// <summary>
/// Defines rate limiting policy names for different endpoint categories.
/// Policies implement three-tier strategy: Sliding Window (security), Token Bucket (resources), Fixed Window (general).
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Policy name for authentication endpoints (login, signup, social-login).
    /// Algorithm: Sliding Window. Prevents brute force and credential stuffing attacks.
    /// </summary>
    public const string Authentication = "Authentication";

    /// <summary>
    /// Policy name for OTP endpoints (verify, resend).
    /// Algorithm: Sliding Window. Prevents OTP enumeration and verification abuse.
    /// </summary>
    public const string Otp = "Otp";

    /// <summary>
    /// Policy name for password management endpoints (forgot, reset, change, set).
    /// Algorithm: Sliding Window. Prevents password reset abuse and email flooding.
    /// </summary>
    public const string PasswordManagement = "PasswordManagement";

    /// <summary>
    /// Policy name for file upload endpoints.
    /// Algorithm: Token Bucket. Allows bursts while preventing sustained upload abuse.
    /// </summary>
    public const string FileUpload = "FileUpload";

    /// <summary>
    /// Policy name for data export endpoints (CSV, Excel).
    /// Algorithm: Token Bucket. Prevents resource exhaustion from bulk exports.
    /// </summary>
    public const string DataExport = "DataExport";

    /// <summary>
    /// Policy name for content browsing endpoints (articles, videos, shows).
    /// Algorithm: Fixed Window. General DoS protection for read operations.
    /// </summary>
    public const string ContentBrowsing = "ContentBrowsing";

    /// <summary>
    /// Policy name for user profile management endpoints.
    /// Algorithm: Fixed Window. Moderate limits for user-specific operations.
    /// </summary>
    public const string UserProfile = "UserProfile";

    /// <summary>
    /// Policy name for session management endpoints.
    /// Algorithm: Fixed Window. Prevents session enumeration and abuse.
    /// </summary>
    public const string SessionManagement = "SessionManagement";

    /// <summary>
    /// Policy name for admin metrics and analytics endpoints.
    /// Algorithm: Fixed Window. Prevents expensive query abuse.
    /// </summary>
    public const string AdminMetrics = "AdminMetrics";
}
