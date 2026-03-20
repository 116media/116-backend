using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.DeletePlaylist;

/// <summary>
/// Command to delete a user playlist.
/// </summary>
/// <param name="Id">The unique identifier of the playlist to delete.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicDeletePlaylistCommand(Guid Id, Guid UserId) : ICommand<PublicDeletePlaylistResult>;

/// <summary>
/// Result of the <see cref="PublicDeletePlaylistCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicDeletePlaylistResult(bool IsSuccess);
