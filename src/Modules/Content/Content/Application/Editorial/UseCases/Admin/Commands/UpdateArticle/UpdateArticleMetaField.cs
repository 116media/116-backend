using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Contains metadata information for the update article route.
/// </summary>
public static class UpdateArticleMetaField
{
    public static readonly RouteMetadata UpdateArticle = new(
        "UpdateArticle",
        "Update article content",
        """
            Saves the full article content (step 2 of the two-step article creation flow).
            Also used when revising a <c>Rejected</c> article before resubmission.
            \n
            Only articles in <c>Draft</c> or <c>Rejected</c> status can be updated.
            Attempting to update an article in any other status will return a 400 Bad Request.
            \n
            The handler computes an image diff between the previous body and the new body.
            Any Cloudinary images that were removed from the body or cover are automatically
            deleted from Cloudinary storage and purged from the <c>article_images</c> table.
            \n
            **Headline requirements:** Minimum 100 characters, maximum 300 characters.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated article details on success\n
            - Returns 400 Bad Request if the article is not in Draft or Rejected status, or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
