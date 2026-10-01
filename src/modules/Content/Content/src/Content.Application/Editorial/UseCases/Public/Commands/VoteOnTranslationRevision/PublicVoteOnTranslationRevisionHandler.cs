using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Handles the <see cref="PublicVoteOnTranslationRevisionCommand" /> to vote on a translation
/// revision, applying it to the translation when the vote crosses the auto-accept threshold.
/// </summary>
/// <param name="voteService">Service casting and tallying the vote.</param>
/// <param name="translationRepository">Repository loading the translation an accepted revision applies to.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicVoteOnTranslationRevisionHandler(
    IPublicTranslationRevisionVoteService voteService,
    ITranslationRepository translationRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicVoteOnTranslationRevisionCommand, PublicVoteOnTranslationRevisionResult>
{
    /// <inheritdoc />
    public async Task<PublicVoteOnTranslationRevisionResult> Handle(
        PublicVoteOnTranslationRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        TranslationRevisionVoteData tally = await voteService.CastVoteAsync(
            revisionId: command.RevisionId,
            userId: command.UserId,
            vote: command.Vote,
            comment: command.Comment,
            cancellationToken: cancellationToken
        );

        if (
            tally.NetApprovals >= TranslationConstants.AutoAcceptThreshold
            && new PendingTranslationRevisionSpecification().IsSatisfiedBy(tally.Revision)
            && tally.Revision.Accept(decidedByUserId: null)
        )
        {
            LyricsTranslationEntity translation = await translationRepository.GetByIdOrThrowAsync(
                id: tally.Revision.TranslationId,
                cancellationToken: cancellationToken
            );
            translation.ApplyAcceptedRevision(newText: tally.Revision.ProposedText);
        }

        // The acceptance and the applied text commit together.
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new PublicVoteOnTranslationRevisionResult(IsSuccess: true);
    }
}
