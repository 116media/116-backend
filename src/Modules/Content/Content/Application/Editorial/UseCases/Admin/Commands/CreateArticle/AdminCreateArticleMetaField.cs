using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Contains metadata information for the create article route.
/// </summary>
public static class AdminCreateArticleMetaField
{
    public static readonly RouteMetadata AdminCreateArticle = new(
        "CreateArticle",
        "Create a new article draft",
        """
            Creates a new article shell (step 1 of the two-step article creation flow).
            \n
            The article is created with a title, slug, and category only. The body and headline
            default to empty strings and must be filled in via
            <c>PUT /api/v1/admin/articles/{id}</c> (step 2) before the article can be submitted.
            \n
            The returned <c>articleId</c> must be stored by the frontend and used as the upload
            target for inline body images via the image upload endpoint.
            \n
            For paid (commissioned) articles, both <c>customerId</c> and <c>orderItemId</c>
            must be provided together. For free editorial content, both must be omitted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with article details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the specified category does not exist\n
            - Returns 409 Conflict if an article with the same slug already exists\n
        """
    );
}
