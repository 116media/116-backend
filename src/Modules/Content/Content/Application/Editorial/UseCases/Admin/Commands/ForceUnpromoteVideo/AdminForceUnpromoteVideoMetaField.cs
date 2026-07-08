using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Contains metadata information for the force-unpromote video route.
/// </summary>
public static class AdminForceUnpromoteVideoMetaField
{
    public static readonly RouteMetadata ForceUnpromoteVideo = new(
        "ForceUnpromoteVideo",
        "Force-unpromote a promoted video (SuperAdmin only)",
        """
            Immediately removes the active paid promotion from a video, regardless of the
            original <c>PromotedUntil</c> expiry date.
            \n
            The operation records three audit fields on the video:
            <c>UnpromotedAt</c> (UTC timestamp), <c>UnpromotedBy</c> (SuperAdmin UUID), and
            <c>UnpromotedReason</c> (free-text justification up to 500 chars).
            These fields are the inputs required to compute the pro-rata refund amount:
            <c>refund = PromoPriceSnapshotUsd × (PromotedUntil − UnpromotedAt) / DurationDays</c>.
            \n
            The endpoint will return 400 Bad Request if the video is not currently promoted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with VideoId and UnpromotedAt on success\n
            - Returns 400 Bad Request if the video is not currently promoted\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the video does not exist\n
        """
    );
}
