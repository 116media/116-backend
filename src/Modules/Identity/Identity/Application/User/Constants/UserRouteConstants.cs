namespace _116.Identity.Application.User.Constants;

/// <summary>
/// Contains route path constants for user-related API endpoints.
/// Provides centralized string constants for URL segments used in user profile and avatar management.
/// </summary>
public static class UserRouteConstants
{
    /// <summary>
    /// Route segment for avatar management endpoints.
    /// Used for uploading and updating user profile images.
    /// Example: /api/v1/public/me/avatar.
    /// </summary>
    public const string Avatar = "avatar";

    /// <summary>
    /// Route segment for user profile management endpoints.
    /// Used for retrieving and updating user profile information.
    /// Example: /api/v1/public/me/profile.
    /// </summary>
    public const string Profile = "profile";

    /// <summary>
    /// The base endpoint path for all user routes.
    /// Combined with public/admin prefixes: /api/v1/public/user or /api/v1/admin/user.
    /// </summary>
    public const string Endpoint = "user";
}
