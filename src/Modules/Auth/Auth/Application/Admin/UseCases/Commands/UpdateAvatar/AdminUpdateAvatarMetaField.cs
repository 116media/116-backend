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

             Admin users only need to have active accounts (no verification requirement).

             **Authentication Requirements:**
             - Admin user must be logged in (JWT token required)
             - Must have Admin or SuperAdmin role
             - Account must be active

             **Request Requirements:**
             - Valid avatar URL (required)
             - URL must be accessible and point to a valid image
             - Maximum URL length: 2048 characters

             **Avatar Management:**
             - Previous avatar is automatically deleted when updating
             - New avatar is downloaded and stored in the system
             - Supports common image formats (JPEG, PNG, GIF, WebP)
             - Smart deduplication: if the same URL is provided again, no duplicate download occurs

             **Response Codes:**
             - Returns 200 OK with updated admin user information including new avatar
             - Returns 400 Bad Request for invalid avatar URL format
             - Returns 401 Unauthorized for unauthenticated requests
             - Returns 403 Forbidden for non-admin users or inactive accounts
             - Returns 404 Not Found when admin user doesn't exist

             **Security Features:**
             - Only authenticated admin users can update their own avatar
             - Role-based authorization (Admin/SuperAdmin required)
             - Account activity verification (active accounts only)
             - URL validation to ensure proper format
             - Automatic cleanup of old avatar files

             **Process Flow:**
             1. Validates admin authentication and account status
             2. Validates the provided avatar URL format
             3. Downloads the new avatar from the URL
             4. Deletes the previous avatar file (if exists)
             5. Updates admin user record with new avatar reference
             6. Returns updated admin user information with avatar details
         """
    );
}
