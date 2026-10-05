using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Casts and tallies a vote for the public vote-on-translation-revision use case.
/// </summary>
/// <param name="revisionRepository">Repository loading the revision.</param>
/// <param name="voteRepository">Repository checking, staging and tallying votes.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicTranslationRevisionVoteService(
    ITranslationRevisionRepository revisionRepository,
    ITranslationVoteRepository voteRepository,
    ContentI18n i18n
) : IPublicTranslationRevisionVoteService
{
    /// <inheritdoc />
    public async Task<TranslationRevisionVoteData> CastVoteAsync(
        Guid revisionId,
        Guid userId,
        EnumVote vote,
        string? comment,
        CancellationToken cancellationToken
    )
    {
        LyricsTranslationRevisionEntity revision = await revisionRepository.GetByIdOrThrowAsync(
            id: revisionId,
            cancellationToken: cancellationToken
        );

        bool alreadyVoted = await voteRepository.HasVotedAsync(
            revisionId: revisionId,
            userId: userId,
            cancellationToken: cancellationToken
        );

        if (alreadyVoted)
        {
            throw i18n.Translation.AlreadyVoted();
        }

        // The tally is read before the insert, which the database cannot see yet; the new vote is added in below.
        int netApprovalsBeforeThisVote = await voteRepository.GetNetApprovalsAsync(
            revisionId: revisionId,
            cancellationToken: cancellationToken
        );

        // The unique (RevisionId, UserId) index is the database-level backstop of the pre-check above.
        var newVote = LyricsTranslationVoteEntity.Create(
            id: Guid.NewGuid(),
            revisionId: revisionId,
            userId: userId,
            vote: vote,
            comment: comment
        );
        await voteRepository.AddAsync(vote: newVote, cancellationToken: cancellationToken);

        int netApprovals = netApprovalsBeforeThisVote + (vote == EnumVote.Approve ? 1 : -1);

        return new TranslationRevisionVoteData(Revision: revision, NetApprovals: netApprovals);
    }
}
