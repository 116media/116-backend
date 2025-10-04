using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Admin.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Contains metadata information for the admin update avatar route.
/// </summary>
public static class AdminUpdateAvatarMetaField
{
    /// <summary>
    /// Metadata describing the admin update avatar endpoint.
    /// </summary>
    public static readonly RouteMetadata UpdateAvatar = new(
        name: "AdminUpdateAvatar",
        summary: "Update admin user avatar",
        description: """
             Updates the authenticated admin user's avatar by providing a new avatar URL.
             This endpoint allows logged-in admin users to update their profile avatar from an external URL.
             The system will download and store the avatar file, and automatically delete any previous avatar.
             \n
             Admin users only need to have active accounts (no verification requirement).
             \n
             **Authentication Requirements:**\n
             - Admin user must be logged in (JWT token required)\n
             - Must have Admin or SuperAdmin role\n
             - Account must be active\n
             \n
             **Request Requirements:**\n
             - Valid avatar URL (required)\n
             - URL must be accessible and point to a valid image\n
             - Maximum URL length: 2048 characters\n
             \n
             **Avatar Management:**\n
             - Previous avatar is automatically deleted when updating\n
             - New avatar is downloaded and stored in the system\n
             - Supports common image formats (JPEG, PNG, GIF, WebP)\n
             - Smart deduplication: if the same URL is provided again, no duplicate download occurs\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with updated admin user information including new avatar\n
             - Returns 400 Bad Request for invalid avatar URL format\n
             - Returns 401 Unauthorized for unauthenticated requests\n
             - Returns 403 Forbidden for non-admin users or inactive accounts\n
             - Returns 404 Not Found when admin user doesn't exist\n
             \n
             **Security Features:**\n
             - Only authenticated admin users can update their own avatar\n
             - Role-based authorization (Admin/SuperAdmin required)\n
             - Account activity verification (active accounts only)\n
             - URL validation to ensure proper format\n
             - Automatic cleanup of old avatar files\n
             \n
             **Process Flow:**\n
             1. Validates admin authentication and account status\n
             2. Validates the provided avatar URL format\n
             3. Downloads the new avatar from the URL\n
             4. Deletes the previous avatar file (if exists)\n
             5. Updates admin user record with new avatar reference\n
             6. Returns updated admin user information with avatar details
         """
    );
}
