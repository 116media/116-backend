using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist;

/// <summary>
/// Handles the <see cref="PublicRemoveVideoFromPlaylistCommand" /> to remove a video from a playlist.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicRemoveVideoFromPlaylistHandler(
    IPlaylistRepository playlistRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicRemoveVideoFromPlaylistCommand, PublicRemoveVideoFromPlaylistResult>
{
    /// <inheritdoc />
    public async Task<PublicRemoveVideoFromPlaylistResult> Handle(
        PublicRemoveVideoFromPlaylistCommand command,
        CancellationToken cancellationToken
    )
    {
        PlaylistEntity? playlist = await playlistRepository.GetByIdAsync(
            id: command.PlaylistId,
            cancellationToken: cancellationToken
        );

        if (playlist is not null)
        {
            if (playlist.UserId != command.UserId)
            {
                throw i18n.Playlist.NotOwner();
            }

            await playlistRepository.RemoveVideoAsync(
                playlistId: command.PlaylistId,
                videoId: command.VideoId,
                cancellationToken: cancellationToken
            );

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new PublicRemoveVideoFromPlaylistResult(IsSuccess: true);
        }

        throw i18n.Playlist.NotFound(id: command.PlaylistId);
    }
}
