using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeLyricsRevision;

/// <summary>
/// Handles the <see cref="PublicProposeLyricsRevisionCommand" /> to propose a community
/// correction to a published lyrics page's canonical text.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="revisionRepository">Repository for lyrics-text correction revision data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicProposeLyricsRevisionHandler(
    ILyricsRepository lyricsRepository,
    ILyricsRevisionRepository revisionRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicProposeLyricsRevisionCommand, PublicProposeLyricsRevisionResult>
{
    /// <inheritdoc />
    public async Task<PublicProposeLyricsRevisionResult> Handle(
        PublicProposeLyricsRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        // Only confirms the lyrics record exists — no check on its origin (admin-entered,
        // community-submitted, or verified-artist self-uploaded), and no restriction to only
        // Published records. Every lyrics page goes through the identical correction flow.
        await lyricsRepository.GetByIdOrThrowAsync(id: command.LyricsId, cancellationToken: cancellationToken);

        LyricsRevisionEntity revision = LyricsRevisionEntity.Propose(
            id: Guid.NewGuid(),
            lyricsId: command.LyricsId,
            proposedText: command.ProposedText,
            editSummary: command.EditSummary,
            userId: command.UserId
        );

        await revisionRepository.AddAsync(revision: revision, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicProposeLyricsRevisionResult(RevisionId: revision.Id);
    }
}
