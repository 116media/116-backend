using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Handles the <see cref="PublicVoteOnTranslationRevisionCommand" /> to record a community vote
/// on a pending translation revision, auto-accepting it once the net approval threshold is met.
/// </summary>
/// <param name="revisionRepository">Repository for translation revision data access operations.</param>
/// <param name="voteRepository">Repository for translation revision vote data access operations.</param>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicVoteOnTranslationRevisionHandler(
    ITranslationRevisionRepository revisionRepository,
    ITranslationVoteRepository voteRepository,
    ITranslationRepository translationRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicVoteOnTranslationRevisionCommand, PublicVoteOnTranslationRevisionResult>
{
    /// <inheritdoc />
    public async Task<PublicVoteOnTranslationRevisionResult> Handle(
        PublicVoteOnTranslationRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsTranslationRevisionEntity revision = await revisionRepository.GetByIdOrThrowAsync(
            id: command.RevisionId,
            cancellationToken: cancellationToken
        );

        bool alreadyVoted = await voteRepository.HasVotedAsync(
            revisionId: command.RevisionId,
            userId: command.UserId,
            cancellationToken: cancellationToken
        );

        if (alreadyVoted)
        {
            throw i18n.Translation.AlreadyVoted();
        }

        // Tally existing votes BEFORE adding this one — GetNetApprovalsAsync queries the
        // database directly, which does not see an entity that's only been added to the
        // change tracker and not yet flushed. The just-cast vote's own contribution is added
        // in below explicitly rather than re-querying after the (still unsaved) insert.
        int netApprovalsBeforeThisVote = await voteRepository.GetNetApprovalsAsync(
            revisionId: command.RevisionId,
            cancellationToken: cancellationToken
        );

        // Unique (RevisionId, UserId) index is the real, DB-level backstop enforcement of the
        // pre-check above.
        var vote = LyricsTranslationVoteEntity.Create(
            id: Guid.NewGuid(),
            revisionId: command.RevisionId,
            userId: command.UserId,
            vote: command.Vote,
            comment: command.Comment
        );
        await voteRepository.AddAsync(vote: vote, cancellationToken: cancellationToken);

        int netApprovals = netApprovalsBeforeThisVote + (command.Vote == EnumVote.Approve ? 1 : -1);

        if (netApprovals >= TranslationConstants.AutoAcceptThreshold && revision.Status == EnumRevisionStatus.Pending)
        {
            revision.Accept(decidedByUserId: null);
            revisionRepository.Update(revision: revision);

            LyricsTranslationEntity translation = await translationRepository.GetByIdOrThrowAsync(
                id: revision.TranslationId,
                cancellationToken: cancellationToken
            );
            translation.ApplyAcceptedRevision(newText: revision.ProposedText);
            translationRepository.Update(translation: translation);
        }

        // Both the revision's acceptance and the translation's applied text commit together in
        // this single call — the two-step apply is atomic here, unlike spec 11's submission
        // approval sequence which spans two separate commits.
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicVoteOnTranslationRevisionResult(IsSuccess: true);
    }
}
