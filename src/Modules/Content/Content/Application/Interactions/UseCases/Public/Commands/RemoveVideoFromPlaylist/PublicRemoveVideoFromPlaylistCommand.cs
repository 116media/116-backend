using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RemoveVideoFromPlaylist;

/// <summary>
/// Command to remove a video from a user's playlist.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist.</param>
/// <param name="VideoId">The unique identifier of the video to remove.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicRemoveVideoFromPlaylistCommand(Guid PlaylistId, Guid VideoId, Guid UserId)
    : ICommand<PublicRemoveVideoFromPlaylistResult>;

/// <summary>
/// Result of the <see cref="PublicRemoveVideoFromPlaylistCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRemoveVideoFromPlaylistResult(bool IsSuccess);
