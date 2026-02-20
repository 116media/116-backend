using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Contains metadata information for the public get all tags route.
/// </summary>
public static class GetAllTagsMetaField
{
    public static readonly RouteMetadata GetAllTags = new(
        "GetAllTags",
        "Get all tags",
        """
            Returns all available tags for browsing and filtering content.
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of tags on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
