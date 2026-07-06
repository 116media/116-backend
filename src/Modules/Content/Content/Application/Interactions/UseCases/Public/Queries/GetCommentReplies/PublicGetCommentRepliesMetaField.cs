using _116.Shared.Application.Metadata;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies;

/// <summary>
/// Contains metadata information for the get comment replies route.
/// </summary>
public static class PublicGetCommentRepliesMetaField
{
    public static readonly RouteMetadata PublicGetCommentReplies = new(
        "PublicGetCommentReplies",
        "List replies to a comment",
        """
            Returns a paginated list of non-deleted replies to a top-level comment, each
            enriched with the replier's author profile.
            \n
            **Authentication Requirements:**\n
            - No authentication required — anonymous access is permitted\n
            \n
            **Response Codes:**\n
            - Returns 200 OK on success\n
            - Returns 429 Too Many Requests if rate limit is exceeded\n
        """
    );
}
