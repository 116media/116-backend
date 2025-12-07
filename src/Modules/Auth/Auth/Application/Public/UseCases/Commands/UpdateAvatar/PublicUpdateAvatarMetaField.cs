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
        summary: "Update user avatar via file upload",
        description: """
             Updates the authenticated user's avatar by uploading an image file.\n
             This endpoint accepts multipart/form-data file uploads and stores the image in Cloudinary cloud storage.
             The system will automatically delete any previous avatar when a new one is uploaded.
             Only verified users can update their avatar to maintain profile quality and security.
             \n
             **Authentication Requirements:**\n
             - User must be logged in (JWT token required)\n
             - Account must be active and verified\n
             \n
             **Request Requirements:**\n
             - Content-Type: multipart/form-data\n
             - Form field name: "avatarFile"\n
             - Allowed file types: JPEG, PNG, GIF, WebP\n
             - Maximum file size: 1MB\n
             - File must be a valid image\n
             \n
             **Avatar Management:**\n
             - Previous avatar is automatically deleted from cloud storage\n
             - Images are stored in Cloudinary with automatic optimization\n
             - Secure HTTPS URLs are generated for accessing avatars\n
             - Smart quality optimization and format conversion\n
             \n
             **Response Codes:**\n
             - Returns 200 OK with updated user information including new avatar\n
             - Returns 400 Bad Request for invalid file type, size, or missing file\n
             - Returns 401 Unauthorized for unauthenticated requests\n
             - Returns 403 Forbidden for inactive or unverified accounts\n
             - Returns 404 Not Found when user doesn't exist\n
             \n
             **Security Features:**\n
             - Only the authenticated user can update their own avatar\n
             - Account verification required (verified accounts only)\n
             - File type and size validation\n
             - Automatic cleanup of old avatar files from cloud storage\n
             - Secure signed uploads to Cloudinary\n
             \n
             **Process Flow:**\n
             1. Validates user authentication and account status\n
             2. Validates the uploaded file (type, size, format)\n
             3. Uploads the new avatar to Cloudinary cloud storage\n
             4. Deletes the previous avatar from cloud storage (if exists)\n
             5. Updates user record with new avatar reference\n
             6. Returns updated user information with avatar details\n
             \n
             **Example cURL Request:**\n
             ```
             curl -X PATCH https://api.example.com/api/v1/public/profile/avatar \
               -H "Authorization: Bearer YOUR_JWT_TOKEN" \
               -F "avatarFile=@/path/to/image.jpg"
             ```
         """
    );
}
