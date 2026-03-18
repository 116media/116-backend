using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.CreatePlaylist;

/// <summary>
/// Command to create a new user playlist.
/// </summary>
/// <param name="UserId">The identity user UUID of the playlist owner.</param>
/// <param name="Name">The display name for the playlist.</param>
public record PublicCreatePlaylistCommand(Guid UserId, string Name) : ICommand<PublicCreatePlaylistResult>;

/// <summary>
/// Result of the <see cref="PublicCreatePlaylistCommand" />.
/// </summary>
/// <param name="Playlist">The newly created playlist DTO.</param>
public record PublicCreatePlaylistResult(PlaylistDto Playlist);
