using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteLyrics;

/// <summary>
/// Handles the <see cref="AdminDeleteLyricsCommand" /> to permanently delete a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeleteLyricsHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminDeleteLyricsCommand, AdminDeleteLyricsResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteLyricsResult> Handle(
        AdminDeleteLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        lyricsRepository.Remove(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteLyricsResult(IsSuccess: true);
    }
}
