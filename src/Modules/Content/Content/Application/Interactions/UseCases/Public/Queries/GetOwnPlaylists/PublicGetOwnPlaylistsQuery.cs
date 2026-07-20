using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnPlaylists;

/// <summary>
/// Query for retrieving all playlists owned by the authenticated user.
/// </summary>
/// <param name="UserId">The identity user UUID of the requesting user.</param>
public record PublicGetOwnPlaylistsQuery(Guid UserId) : IQuery<PublicGetOwnPlaylistsResult>;

/// <summary>
/// Result of the <see cref="PublicGetOwnPlaylistsQuery" />.
/// </summary>
/// <param name="Playlists">The list of the user's playlist DTOs.</param>
public record PublicGetOwnPlaylistsResult(IReadOnlyList<PlaylistDto> Playlists);
