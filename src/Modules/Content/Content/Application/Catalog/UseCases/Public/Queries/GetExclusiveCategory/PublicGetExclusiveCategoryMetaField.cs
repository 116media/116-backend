using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetExclusiveCategory;

/// <summary>
/// Contains metadata information for the public get exclusive category route.
/// </summary>
public static class PublicGetExclusiveCategoryMetaField
{
    /// <summary>
    /// Route metadata for the public exclusive category endpoint.
    /// </summary>
    public static readonly RouteMetadata GetExclusiveCategory = new(
        "PublicGetExclusiveCategory",
        "Get the exclusive category with videos",
        """
            Returns the currently exclusive category along with a paginated list of its published
            videos. The exclusive category is the featured show displayed on the homepage after the
            promotion feed.
            \n
            Only one category can be exclusive at a time, and it must be a video category. If no
            category is currently marked as exclusive, a 404 response is returned.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the exclusive category and its videos\n
            - Returns 404 Not Found if no exclusive category is set\n
        """
    );
}
