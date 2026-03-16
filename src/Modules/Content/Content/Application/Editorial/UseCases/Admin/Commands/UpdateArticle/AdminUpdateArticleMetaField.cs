using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Contains metadata information for the update article route.
/// </summary>
public static class AdminUpdateArticleMetaField
{
    public static readonly RouteMetadata AdminUpdateArticle = new(
        "UpdateArticle",
        "Update article",
        """
            Updates all editable fields of an article in a single call. This endpoint serves
            two purposes: (1) step 2 of the two-step article creation flow — after the admin
            clicks "Save Draft" (POST), this PUT call fills in the headline, body, and cover
            image before clicking "Submit"; (2) any subsequent edit while the article is still
            in a mutable status — for example correcting a typo in the title of a rejected article.
            \n
            Covers metadata (title, slug, category), content (headline, body, cover image),
            commerce fields (customer, order item), promotion flags (social boost, featured
            placement), and SEO metadata (meta title, meta description).
            \n
            Allowed when the article status is <c>Draft</c>, <c>PendingPayment</c>,
            <c>PendingReview</c>, or <c>Rejected</c>. Attempting to update an article
            in <c>Approved</c>, <c>Published</c>, or <c>Archived</c> status will return
            a 400 Bad Request.
            \n
            The handler computes an image diff between the previous body and the new body.
            Any Cloudinary images removed from the body or cover are automatically deleted
            from Cloudinary storage and purged from the <c>article_images</c> table after commit.
            \n
            **Slug uniqueness:** The slug must be unique across all articles. If the provided
            slug belongs to a different article, the request will return 409 Conflict.
            \n
            **Headline requirements:** Minimum 100 characters, maximum 300 characters.
            \n
            **Commerce fields:** <c>customerId</c> and <c>orderItemId</c> must be provided
            together or both omitted. Providing only one will return a 400 Bad Request.
            \n
            **Featured placement:** <c>featuredUntil</c> must be a future date when provided.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated article details on success\n
            - Returns 400 Bad Request if status is not editable, or validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 404 Not Found if the article or category does not exist\n
            - Returns 409 Conflict if the slug is already taken by another article\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
