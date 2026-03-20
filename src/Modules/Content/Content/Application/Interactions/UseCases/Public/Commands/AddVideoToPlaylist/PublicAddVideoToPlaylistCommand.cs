using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddVideoToPlaylist;

/// <summary>
/// Command to add a video to a user's playlist.
/// </summary>
/// <param name="PlaylistId">The unique identifier of the playlist.</param>
/// <param name="VideoId">The unique identifier of the video to add.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
/// <param name="SortOrder">The display order for this video within the playlist.</param>
public record PublicAddVideoToPlaylistCommand(Guid PlaylistId, Guid VideoId, Guid UserId, int SortOrder)
    : ICommand<PublicAddVideoToPlaylistResult>;

/// <summary>
/// Result of the <see cref="PublicAddVideoToPlaylistCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicAddVideoToPlaylistResult(bool IsSuccess);
