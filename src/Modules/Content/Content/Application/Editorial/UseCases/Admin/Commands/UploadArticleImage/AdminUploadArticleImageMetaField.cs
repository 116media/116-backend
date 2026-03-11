using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Contains metadata information for the upload article image route.
/// </summary>
public static class AdminUploadArticleImageMetaField
{
    public static readonly RouteMetadata AdminUploadArticleImage = new(
        "UploadArticleImage",
        "Upload an image for an article",
        """
            Uploads an image for an article and creates an article image tracking record.
            \n
            Images can be of type <c>Cover</c> (the article's primary cover image) or
            <c>Body</c> (an inline image embedded in the article's rich-text body).
            \n
            The returned <c>url</c> should be used in the article body HTML for body images,
            or passed as <c>coverImageUrl</c> when updating the article content for cover images.
            \n
            Accepts multipart/form-data with an image file and an image type.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with image details and location header on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 429 Too Many Requests if the rate limit is exceeded\n
        """
    );
}
