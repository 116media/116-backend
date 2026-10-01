using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.Contracts;

/// <summary>
/// The revision voted on and its net approvals including the vote just cast.
/// </summary>
/// <param name="Revision">The revision.</param>
/// <param name="NetApprovals">Approvals minus rejections, the new vote included.</param>
public record LyricsRevisionVoteData(LyricsRevisionEntity Revision, int NetApprovals);

/// <summary>
/// Casts a vote on a lyrics revision: loads it, refuses a second vote from the same user,
/// stages the vote and tallies the result.
/// </summary>
public interface IPublicLyricsRevisionVoteService
{
    /// <summary>
    /// Casts the vote, throwing the localized error when the user already voted. The caller
    /// applies any auto-accept and owns the commit.
    /// </summary>
    /// <param name="revisionId">The revision.</param>
    /// <param name="userId">The voter.</param>
    /// <param name="vote">The vote.</param>
    /// <param name="comment">The voter's optional comment.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<LyricsRevisionVoteData> CastVoteAsync(
        Guid revisionId,
        Guid userId,
        EnumVote vote,
        string? comment,
        CancellationToken cancellationToken
    );
}
