using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Handles the <see cref="PublicRenamePlaylistCommand" /> to rename a user playlist.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicRenamePlaylistHandler(
    IPlaylistRepository playlistRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicRenamePlaylistCommand, PublicRenamePlaylistResult>
{
    /// <inheritdoc />
    public async Task<PublicRenamePlaylistResult> Handle(
        PublicRenamePlaylistCommand command,
        CancellationToken cancellationToken
    )
    {
        PlaylistEntity? playlist = await playlistRepository.GetByIdAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        if (playlist is not null)
        {
            if (playlist.UserId != command.UserId)
            {
                throw i18n.Playlist.NotOwner();
            }

            playlist.Rename(name: command.Name);
            playlistRepository.Update(playlist: playlist);

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new PublicRenamePlaylistResult(IsSuccess: true);
        }

        throw i18n.Playlist.NotFound(id: command.Id);
    }
}
