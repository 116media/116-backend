using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ArchiveLyrics;

/// <summary>
/// Handles the <see cref="AdminArchiveLyricsCommand" /> to archive a lyrics page.
/// Note: Archiving is reversible.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminArchiveLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminArchiveLyricsCommand, AdminArchiveLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminArchiveLyricsResult> Handle(
        AdminArchiveLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        bool archived = lyrics.Archive();

        if (!archived)
        {
            throw i18n.Lyrics.AlreadyArchived();
        }

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminArchiveLyricsResult(IsSuccess: true);
    }
}
