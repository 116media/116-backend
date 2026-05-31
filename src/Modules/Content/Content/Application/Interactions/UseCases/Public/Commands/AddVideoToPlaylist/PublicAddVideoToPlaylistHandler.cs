using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;

/// <summary>
/// Handles the <see cref="PublicAddVideoToPlaylistCommand" /> to add a video to a playlist.
/// </summary>
/// <param name="playlistRepository">Repository for playlist data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicAddVideoToPlaylistHandler(
    IPlaylistRepository playlistRepository,
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicAddVideoToPlaylistCommand, PublicAddVideoToPlaylistResult>
{
    /// <inheritdoc />
    public async Task<PublicAddVideoToPlaylistResult> Handle(
        PublicAddVideoToPlaylistCommand command,
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

            VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
                id: command.VideoId,
                cancellationToken: cancellationToken
            );

            if (video.Status != EnumContentStatus.Published)
            {
                throw i18n.Video.NotFound(id: command.VideoId);
            }

            bool alreadyExists = await playlistRepository.VideoExistsInPlaylistAsync(
                playlistId: command.PlaylistId,
                videoId: command.VideoId,
                cancellationToken: cancellationToken
            );

            if (alreadyExists)
            {
                throw i18n.Playlist.VideoAlreadyInPlaylist();
            }

            var playlistVideo = PlaylistVideoEntity.Create(
                id: Guid.NewGuid(),
                playlistId: command.PlaylistId,
                videoId: command.VideoId,
                sortOrder: command.SortOrder
            );

            await playlistRepository.AddVideoAsync(playlistVideo: playlistVideo, cancellationToken: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return new PublicAddVideoToPlaylistResult(IsSuccess: true);
        }

        throw i18n.Playlist.NotFound(id: command.PlaylistId);
    }
}
