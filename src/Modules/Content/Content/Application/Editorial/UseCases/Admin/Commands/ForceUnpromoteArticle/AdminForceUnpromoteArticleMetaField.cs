using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Contains metadata information for the force-unpromote article route.
/// </summary>
public static class AdminForceUnpromoteArticleMetaField
{
    public static readonly RouteMetadata ForceUnpromoteArticle = new(
        "ForceUnpromoteArticle",
        "Force-unpromote a promoted article (SuperAdmin only)",
        """
            Immediately removes the active paid promotion from an article, regardless of the
            original <c>PromotedUntil</c> expiry date.
            \n
            The operation records three audit fields on the article:
            <c>UnpromotedAt</c> (UTC timestamp), <c>UnpromotedBy</c> (SuperAdmin UUID), and
            <c>UnpromotedReason</c> (free-text justification up to 500 chars).
            These fields are the inputs required to compute the pro-rata refund amount:
            <c>refund = PromoPriceSnapshotUsd × (PromotedUntil − UnpromotedAt) / DurationDays</c>.
            \n
            The endpoint will return 400 Bad Request if the article is not currently promoted.
            \n
            **Authentication Requirements:**\n
            - User must be authenticated with a valid access token\n
            - User must have SuperAdmin role\n
            \n
            **Response Codes:**\n
            - Returns 200 OK with ArticleId and UnpromotedAt on success\n
            - Returns 400 Bad Request if the article is not currently promoted\n
            - Returns 401 Unauthorized if access token is invalid or expired\n
            - Returns 403 Forbidden if user lacks SuperAdmin role\n
            - Returns 404 Not Found if the article does not exist\n
        """
    );
}
