using _116.Shared.Application.Metadata;

namespace _116.Auth.Application.Public.UseCases.Commands.UpdateAvatar;

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
             Updates the authenticated user's avatar by providing a new avatar URL.\n
             This endpoint allows logged-in users to update their profile avatar from an external URL.
             The system will download and store the avatar file, and automatically delete any previous avatar.
             Only verified users can update their avatar to maintain profile quality and security.
             \n
             **Authentication Requirements:**\n
             - User must be logged in (JWT token required)\n
             - Account must be active and verified\n
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
             - Returns 200 OK with updated user information including new avatar\n
             - Returns 400 Bad Request for invalid avatar URL format\n
             - Returns 401 Unauthorized for unauthenticated requests\n
             - Returns 403 Forbidden for inactive or unverified accounts\n
             - Returns 404 Not Found when user doesn't exist\n
             \n
             **Security Features:**\n
             - Only the authenticated user can update their own avatar\n
             - Account verification required (verified accounts only)\n
             - URL validation to ensure proper format\n
             - Automatic cleanup of old avatar files\n
             \n
             **Process Flow:**\n
             1. Validates user authentication and account status\n
             2. Validates the provided avatar URL format\n
             3. Downloads the new avatar from the URL\n
             4. Deletes the previous avatar file (if exists)\n
             5. Updates user record with new avatar reference\n
             6. Returns updated user information with avatar details.
         """
    );
}
