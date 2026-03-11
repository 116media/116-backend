using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArticleBySlug;

/// <summary>
/// Contains metadata information for the get article by slug route.
/// </summary>
public static class PublicGetArticleBySlugMetaField
{
    public static readonly RouteMetadata PublicGetArticleBySlug = new(
        "GetArticleBySlug",
        "Get published article by slug",
        """
            Retrieves the full details of a single published article by its URL slug.
            \n
            Returns the complete article including body content, cover image, SEO metadata,
            all associated images, and applied tags. Only published articles are accessible
            via this endpoint — drafts, pending review, and archived articles return 404.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with article details on success\n
            - Returns 404 Not Found if the article does not exist or is not published\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
