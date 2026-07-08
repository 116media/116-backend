using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Contains metadata information for the upload category poster route.
/// </summary>
public static class AdminUploadCategoryPosterMetaField
{
    public static readonly RouteMetadata UploadCategoryPoster = new(
        "AdminUploadCategoryPoster",
        "Upload a poster image for a category",
        """
            Uploads or replaces the poster image for a content category (show).
            The poster is displayed on the homepage exclusive section alongside the show's
            title, description, and video list.
            \n
            If the category already has a poster, the previous file is soft-deleted
            and replaced by the new upload.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated category details on success\n
            - Returns 400 Bad Request if validation fails or no file is provided\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
