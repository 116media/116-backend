using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDecideLyricsRevisionHandler(
    ILyricsRevisionRepository revisionRepository,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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

        bool decided = command.Accept
            ? revision.Accept(decidedByUserId: command.DecidedByUserId)
            : revision.Reject(decidedByUserId: command.DecidedByUserId);

        if (!decided)
        {
            throw i18n.LyricsRevision.AlreadyDecided();
        }

        if (command.Accept)
        {
            LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
                id: revision.LyricsId,
                cancellationToken: cancellationToken
            );
            lyrics.ReplaceLyricsText(lyricsText: revision.ProposedText);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDecideLyricsRevisionResult(IsSuccess: true);
    }
}
