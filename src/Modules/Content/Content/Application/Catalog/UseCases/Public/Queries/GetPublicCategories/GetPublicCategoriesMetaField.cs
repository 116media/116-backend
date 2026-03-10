using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Catalog.UseCases.Public.Queries.GetPublicCategories;

/// <summary>
/// Contains metadata information for the public get categories route.
/// </summary>
public static class GetPublicCategoriesMetaField
{
    public static readonly RouteMetadata GetPublicCategories = new(
        "GetPublicCategories",
        "List public categories",
        """
            Returns the list of active categories available to public visitors.
            \n
            Powers the public-facing catalogue page so potential B2B clients can see what content
            formats 116 offers (e.g. "Artist Profile", "116 Le Focus", "Chronique Sale") before
            getting in touch. Also used by the frontend to build category filter tabs on the article
            and video feed pages.
            \n
            Only active categories are returned — deactivated formats are hidden from the public.
            An optional contentTypeId filter can narrow the list to a specific content type.
            \n
            **Authentication Requirements:**\n
            - No authentication required\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of active categories\n
        """
    );
}
