using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Contains metadata information for the create category route.
/// </summary>
public static class AdminCreateCategoryMetaField
{
    public static readonly RouteMetadata AdminCreateCategory = new(
        "AdminCreateCategory",
        "Create a new category",
        """
            Creates a new content category (e.g. "Artist Profile", "116 Le Focus", "Chronique Sale").
            \n
            Every article and video must belong to exactly one category. Categories determine whether
            content is free or paid — a free category skips the payment flow entirely and goes straight
            to editorial review.
            \n
            **Note:** The content type must already exist before creating a category.
            The slug must be unique, URL-safe, and lowercase (e.g. "artist-profile").
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 201 Created with category details on success\n
            - Returns 400 Bad Request if validation fails\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the content type does not exist\n
            - Returns 409 Conflict if the slug is already taken\n
        """
    );
}
