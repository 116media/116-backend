using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllContentTypes;

/// <summary>
/// Contains metadata information for the public get all content types route.
/// </summary>
public static class PublicGetAllContentTypesMetaField
{
    public static readonly RouteMetadata GetAllContentTypes = new(
        "PublicGetAllContentTypes",
        "Get all content types",
        """
            Returns all content types available on the platform (e.g. Article, Video).
            \n
            Used by the frontend to resolve a content type identifier by name before
            fetching categories scoped to that content type.
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the list of content types on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
