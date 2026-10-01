using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Handles the <see cref="PublicVoteOnLyricsRevisionCommand" /> to vote on a lyrics revision,
/// applying it to the page when the vote crosses the auto-accept threshold.
/// </summary>
/// <param name="voteService">Service casting and tallying the vote.</param>
/// <param name="lyricsRepository">Repository loading the page an accepted revision applies to.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicVoteOnLyricsRevisionHandler(
    IPublicLyricsRevisionVoteService voteService,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicVoteOnLyricsRevisionCommand, PublicVoteOnLyricsRevisionResult>
{
    /// <inheritdoc />
    public async Task<PublicVoteOnLyricsRevisionResult> Handle(
        PublicVoteOnLyricsRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsRevisionVoteData tally = await voteService.CastVoteAsync(
            revisionId: command.RevisionId,
            userId: command.UserId,
            vote: command.Vote,
            comment: command.Comment,
            cancellationToken: cancellationToken
        );

        if (
            tally.NetApprovals >= LyricsRevisionConstants.AutoAcceptThreshold
            && new PendingLyricsRevisionSpecification().IsSatisfiedBy(tally.Revision)
            && tally.Revision.Accept(decidedByUserId: null)
        )
        {
            LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
                id: tally.Revision.LyricsId,
                cancellationToken: cancellationToken
            );
            lyrics.ReplaceLyricsText(lyricsText: tally.Revision.ProposedText);
        }

        // The acceptance and the replaced text commit together.
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        return new PublicVoteOnLyricsRevisionResult(IsSuccess: true);
    }
}
