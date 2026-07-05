using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetAllTags;

/// <summary>
/// Contains metadata information for the public get all tags route.
/// </summary>
public static class PublicGetAllTagsMetaField
{
    public static readonly RouteMetadata GetAllTags = new(
        "PublicGetAllTags",
        "Get all tags",
        """
            Returns all available tags for browsing and filtering content.
            \n
            Supports optional search filtering via the `search` query parameter.
            The search performs a case-insensitive partial match on both tag name and slug.
            \n
            Supports optional content-type filtering via the `contentType` query parameter.
            When set, only tags associated with that content type are returned.
            \n
            Supports an optional `limit` query parameter that caps the number of tags
            returned after ordering by name.
            \n
            Unfiltered results (no `search` term) are cached server-side for 10 minutes,
            keyed by content type and limit; requests carrying a `search` term bypass the cache.
            \n
            **Query Parameters:**\n
            - `search` (optional): filter tags by name or slug (e.g. `?search=fally`)\n
            - `contentType` (optional): restrict to tags used by a content type; accepts
              `article` or `video` (e.g. `?contentType=article`). Omit to return all tags.
              Unrecognized values are ignored and all tags are returned.\n
            - `limit` (optional): maximum number of tags to return (e.g. `?limit=50`).
              Omit to return all matching tags.\n
            \n
            This endpoint is publicly accessible and does not require authentication.
            \n
            **Response Codes:**\n
            - Returns 200 OK with the matching list of tags (empty array if none match)\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
