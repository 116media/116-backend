using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RenamePlaylist;

/// <summary>
/// Command to rename a user playlist.
/// </summary>
/// <param name="Id">The unique identifier of the playlist to rename.</param>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
/// <param name="Name">The new display name for the playlist.</param>
public record PublicRenamePlaylistCommand(Guid Id, Guid UserId, string Name) : ICommand<PublicRenamePlaylistResult>;

/// <summary>
/// Result of the <see cref="PublicRenamePlaylistCommand" />.
/// </summary>
/// <param name="IsSuccess">Indicates if the operation was successful.</param>
public record PublicRenamePlaylistResult(bool IsSuccess);
