using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Handles the <see cref="AdminDecideLyricsRevisionCommand" /> to let a moderator accept or
/// reject a pending lyrics-text correction revision directly, bypassing the community vote
/// tally.
/// </summary>
/// <param name="revisionRepository">Repository for lyrics-text correction revision data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDecideLyricsRevisionHandler(
    ILyricsRevisionRepository revisionRepository,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminDecideLyricsRevisionCommand, AdminDecideLyricsRevisionResult>
{
    /// <inheritdoc />
    public async Task<AdminDecideLyricsRevisionResult> Handle(
        AdminDecideLyricsRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsRevisionEntity revision = await revisionRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        if (command.Accept)
        {
            revision.Accept(decidedByUserId: command.DecidedByUserId);
            revisionRepository.Update(revision: revision);

            LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
                id: revision.LyricsId,
                cancellationToken: cancellationToken
            );
            lyrics.ReplaceLyricsText(lyricsText: revision.ProposedText);
            lyricsRepository.Update(lyrics: lyrics);
        }
        else
        {
            revision.Reject(decidedByUserId: command.DecidedByUserId);
            revisionRepository.Update(revision: revision);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDecideLyricsRevisionResult(IsSuccess: true);
    }
}
