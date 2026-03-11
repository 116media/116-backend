using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveArticle;

/// <summary>
/// Contains metadata information for the archive article route.
/// </summary>
public static class AdminArchiveArticleMetaField
{
    public static readonly RouteMetadata AdminArchiveArticle = new(
        "ArchiveArticle",
        "Archive an article",
        """
            Archives an article, removing it from all public feeds without permanently deleting it.
            \n
            Archiving is a reversible operation — the article's Cloudinary image assets are
            <b>not</b> deleted. The article can be restored to a previous active status if needed.
            \n
            Use archiving instead of deletion when you want to temporarily hide content
            without losing it permanently.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
