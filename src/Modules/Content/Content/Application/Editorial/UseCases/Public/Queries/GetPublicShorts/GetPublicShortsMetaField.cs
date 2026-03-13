using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublicShorts;

/// <summary>
/// Contains metadata information for the get public shorts route.
/// </summary>
public static class GetPublicShortsMetaField
{
    public static readonly RouteMetadata GetPublicShorts = new(
        "GetPublicShorts",
        "List active short videos",
        """
            Retrieves a paginated list of active short video clips for public consumption.
            Only short videos with active status are returned.
            \n
            **Authentication Requirements:**\n
            - No authentication required (public endpoint)\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with paginated short video list on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
