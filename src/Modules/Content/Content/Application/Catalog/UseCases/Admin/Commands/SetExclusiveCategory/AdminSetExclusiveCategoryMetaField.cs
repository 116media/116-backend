using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Contains metadata information for the set exclusive category route.
/// </summary>
public static class AdminSetExclusiveCategoryMetaField
{
    public static readonly RouteMetadata SetExclusiveCategory = new(
        "AdminSetExclusiveCategory",
        "Set a category as the exclusive show",
        """
            Marks a category as the exclusive show featured on the homepage.
            Only one category can be exclusive at a time — setting a new one
            automatically unsets the previous exclusive category.
            \n
            The exclusive show appears on the homepage after the promotion feed section
            as a two-column layout with the poster image, tag, title, description,
            and a list of video cards.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the updated category details on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the category does not exist\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
