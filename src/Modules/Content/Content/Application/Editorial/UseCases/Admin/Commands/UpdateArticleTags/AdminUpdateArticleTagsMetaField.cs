using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticleTags;

/// <summary>
/// Contains metadata information for the update article tags route.
/// </summary>
public static class AdminUpdateArticleTagsMetaField
{
    public static readonly RouteMetadata AdminUpdateArticleTags = new(
        "UpdateArticleTags",
        "Update article tags",
        """
            Replaces the complete set of tags assigned to an article.
            \n
            All existing tag associations are removed and replaced with the provided list of tag IDs.
            Passing an empty list will remove all tags from the article.
            \n
            All tag identifiers are validated to exist before any changes are applied.
            If any tag ID is not found, the operation returns 404 Not Found and no changes are made.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the article or any of the specified tags do not exist\n
        """
    );
}
