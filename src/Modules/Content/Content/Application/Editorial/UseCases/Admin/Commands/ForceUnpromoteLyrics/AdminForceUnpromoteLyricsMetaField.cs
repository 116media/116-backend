using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Contains metadata information for the force-unpromote lyrics route.
/// </summary>
public static class AdminForceUnpromoteLyricsMetaField
{
    public static readonly RouteMetadata ForceUnpromoteLyrics = new(
        "ForceUnpromoteLyrics",
        "Force-unpromote a promoted lyrics page (SuperAdmin only)",
        """
            Immediately removes the active paid promotion from a lyrics page, regardless of the
            original <c>PromotedUntil</c> expiry date.
            \n
            The operation records three audit fields on the lyrics page:
            <c>UnpromotedAt</c> (UTC timestamp), <c>UnpromotedBy</c> (SuperAdmin UUID), and
            <c>UnpromotedReason</c> (free-text justification up to 500 chars).
            These fields are the inputs required to compute the pro-rata refund amount.
            \n
            The endpoint will return 400 Bad Request if the lyrics page is not currently promoted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with LyricsId and UnpromotedAt on success\n
            - Returns 400 Bad Request if the lyrics page is not currently promoted\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the lyrics page does not exist\n
        """
    );
}
