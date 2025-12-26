using _116.Shared.Application.Metadata;

namespace _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Contains metadata information for the admin update avatar route.
/// </summary>
public static class AdminUpdateAvatarMetaField
{
    /// <summary>
    /// Metadata describing the admin update avatar endpoint.
    /// </summary>
    public static readonly RouteMetadata UpdateAvatar = new(
        "AdminUpdateAvatar",
        "Update admin user avatar via file upload",
        """
            Updates the authenticated admin user's avatar by uploading an image file.\n
            This endpoint accepts multipart/form-data file uploads and stores the image in Cloudinary cloud storage.
            The system will automatically delete any previous avatar when a new one is uploaded.
            \n
            Admin users only need to have active accounts (no verification requirement).
            \n
            **Authentication Requirements:**\n
            - Admin user must be logged in (JWT token required)\n
            - Must have Admin or SuperAdmin role\n
            - Account must be active\n
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
            - Returns 200 OK with updated admin user information including new avatar\n
            - Returns 400 Bad Request for invalid file type, size, or missing file\n
            - Returns 401 Unauthorized for unauthenticated requests\n
            - Returns 403 Forbidden for non-admin users or inactive accounts\n
            - Returns 404 Not Found when admin user doesn't exist\n
            \n
            **Security Features:**\n
            - Only authenticated admin users can update their own avatar\n
            - Role-based authorization (Admin/SuperAdmin required)\n
            - Account activity verification (active accounts only)\n
            - File type and size validation\n
            - Automatic cleanup of old avatar files from cloud storage\n
            - Secure signed uploads to Cloudinary\n
            \n
            **Process Flow:**\n
            1. Validates admin authentication and account status\n
            2. Validates the uploaded file (type, size, format)\n
            3. Uploads the new avatar to Cloudinary cloud storage\n
            4. Deletes the previous avatar from cloud storage (if exists)\n
            5. Updates admin user record with new avatar reference\n
            6. Returns updated admin user information with avatar details\n
            \n
            **Example cURL Request:**\n
            ```
            curl -X PATCH https://api.example.com/api/v1/admin/profile/avatar \
              -H "Authorization: Bearer ADMIN_JWT_TOKEN" \
              -F "avatarFile=@/path/to/image.jpg"
            ```
        """
    );
}
