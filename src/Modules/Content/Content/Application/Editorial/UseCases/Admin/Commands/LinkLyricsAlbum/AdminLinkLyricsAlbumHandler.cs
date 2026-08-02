using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.LinkLyricsAlbum;

/// <summary>
/// Handles the <see cref="AdminLinkLyricsAlbumCommand" /> to link or unlink a lyrics page's
/// real, addressable album.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="albumRepository">Repository for album data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminLinkLyricsAlbumHandler(
    ILyricsRepository lyricsRepository,
    IAlbumRepository albumRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminLinkLyricsAlbumCommand, AdminLinkLyricsAlbumResult>
{
    /// <inheritdoc />
    public async Task<AdminLinkLyricsAlbumResult> Handle(
        AdminLinkLyricsAlbumCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        if (command.AlbumId.HasValue)
        {
            await albumRepository.GetByIdOrThrowAsync(id: command.AlbumId.Value, cancellationToken: cancellationToken);
            lyrics.LinkAlbum(albumId: command.AlbumId.Value);
        }
        else
        {
            lyrics.UnlinkAlbum();
        }

        lyricsRepository.Update(lyrics: lyrics);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminLinkLyricsAlbumResult(IsSuccess: true);
    }
}
