using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Contains metadata information for the upload artist avatar route.
/// </summary>
public static class AdminUploadArtistAvatarMetaField
{
    public static readonly RouteMetadata UploadArtistAvatar = new(
        "UploadArtistAvatar",
        "Upload an artist profile avatar image",
        """
            Uploads or replaces the artist profile's avatar image.
            \n
            If the artist profile already has an avatar, the old Cloudinary asset is deleted
            after the new avatar is uploaded successfully.
            \n
            Accepts multipart/form-data with a single image file.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with avatar URL and storage key on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the artist profile does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
