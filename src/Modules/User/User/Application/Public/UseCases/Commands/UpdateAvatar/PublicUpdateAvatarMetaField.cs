using _116.Shared.Application.Metadata;

namespace _116.User.Application.Public.UseCases.Commands.UpdateAvatar;

/// <summary>
/// Contains metadata information for the public update avatar route.
/// </summary>
public static class PublicUpdateAvatarMetaField
{
    /// <summary>
    /// Metadata describing the public update avatar endpoint.
    /// </summary>
    public static readonly RouteMetadata UpdateAvatar = new(
        name: "PublicUpdateAvatar",
        summary: "Update user avatar",
        description: """
             Updates the authenticated user's avatar by providing a new avatar URL.

             This endpoint allows logged-in users to update their profile avatar from an external URL.
             The system will download and store the avatar file, and automatically delete any previous avatar.

             **Authentication Requirements:**
             - User must be logged in (JWT token required)
             - Account must be active and verified

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
             - Returns 200 OK with updated user information including new avatar
             - Returns 400 Bad Request for invalid avatar URL format
             - Returns 401 Unauthorized for unauthenticated requests
             - Returns 403 Forbidden for inactive or unverified accounts
             - Returns 404 Not Found when user doesn't exist

             **Security Features:**
             - Only the authenticated user can update their own avatar
             - Account verification required (verified accounts only)
             - URL validation to ensure proper format
             - Automatic cleanup of old avatar files

             **Process Flow:**
             1. Validates user authentication and account status
             2. Validates the provided avatar URL format
             3. Downloads the new avatar from the URL
             4. Deletes the previous avatar file (if exists)
             5. Updates user record with new avatar reference
             6. Returns updated user information with avatar details

             Only verified users can update their avatar to maintain profile quality and security.
         """
    );
}
