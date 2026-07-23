using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RemoveSingleStreamingLink;

/// <summary>
/// Contains metadata information for the remove single streaming link route.
/// </summary>
public static class AdminRemoveSingleStreamingLinkMetaField
{
    public static readonly RouteMetadata RemoveSingleStreamingLink = new(
        "RemoveSingleStreamingLink",
        "Remove a standalone single's curated streaming link for a platform",
        """
            Removes the curated deep link URL for a standalone single's streaming platform slot,
            reverting that platform's public link back to the generated search-query fallback.
            A no-op if no curated link exists for the given lyrics page and platform.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have Admin or SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks Admin role\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
