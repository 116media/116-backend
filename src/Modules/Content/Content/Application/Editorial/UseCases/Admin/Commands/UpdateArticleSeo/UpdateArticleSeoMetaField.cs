using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleSeo;

/// <summary>
/// Contains metadata information for the update article SEO route.
/// </summary>
public static class UpdateArticleSeoMetaField
{
    public static readonly RouteMetadata UpdateArticleSeo = new(
        "UpdateArticleSeo",
        "Update article SEO metadata",
        """
            Updates the SEO metadata of an article, including the custom meta title
            and meta description used for search engine optimization.
            \n
            The meta title falls back to the article's main title if not provided.
            The meta description falls back to a truncated headline if not provided.
            \n
            This endpoint can be called on an article in any status, allowing SEO
            improvements to be made to published articles without going through the
            full editorial workflow.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated article details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
