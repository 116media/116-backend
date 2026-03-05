using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Contains metadata information for the update category route.
/// </summary>
public static class UpdateCategoryMetaField
{
    public static readonly RouteMetadata UpdateCategory = new(
        "UpdateCategory",
        "Update a category",
        """
            Updates a category's display name, URL slug, and description.
            \n
            **Note:** Slug changes take effect immediately on public category URLs.
            Only perform slug changes when the old URL can be redirected at the frontend.
            The content type and free/paid status cannot be changed via this endpoint.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with updated category details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 409 Conflict if the new slug is already taken\n
        """
    );
}
