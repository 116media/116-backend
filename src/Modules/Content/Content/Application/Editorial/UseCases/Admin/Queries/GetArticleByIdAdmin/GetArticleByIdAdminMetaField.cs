using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleByIdAdmin;

/// <summary>
/// Contains metadata information for the get article by id admin route.
/// </summary>
public static class GetArticleByIdAdminMetaField
{
    public static readonly RouteMetadata GetArticleByIdAdmin = new(
        "GetArticleByIdAdmin",
        "Get article details",
        """
            Retrieves the full details of a single article by its unique identifier.
            \n
            Returns the complete article including body content, cover image, SEO metadata,
            all associated images, and applied tags. Suitable for the article editing view.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with article details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the article does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
