using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteArticle;

/// <summary>
/// Contains metadata information for the delete article route.
/// </summary>
public static class DeleteArticleMetaField
{
    public static readonly RouteMetadata DeleteArticle = new(
        "DeleteArticle",
        "Permanently delete an article",
        """
            Permanently deletes an article and all its associated Cloudinary image assets.
            \n
            Only articles in <c>Draft</c> or <c>Rejected</c> status can be deleted.
            Attempting to delete an article in any other status (Published, Approved,
            PendingReview, PendingPayment) will return a 400 Bad Request.
            \n
            For published or approved articles, use the archive endpoint instead to
            remove the article from public feeds without permanently deleting it.
            \n
            This operation is <b>irreversible</b> — all Cloudinary assets are deleted
            from cloud storage before the database record is removed.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 204 No Content on success\n
            - Returns 400 Bad Request if the article is not in Draft or Rejected status\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
