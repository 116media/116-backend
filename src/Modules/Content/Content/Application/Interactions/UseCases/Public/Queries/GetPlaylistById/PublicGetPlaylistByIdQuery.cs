using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetPlaylistById;

/// <summary>
/// Query for retrieving a playlist by its identifier, including all videos.
/// </summary>
/// <param name="Id">The unique identifier of the playlist.</param>
/// <param name="UserId">The identity user UUID of the requesting user (for ownership check).</param>
public record PublicGetPlaylistByIdQuery(Guid Id, Guid UserId) : IQuery<PublicGetPlaylistByIdResult>;

/// <summary>
/// Result of the <see cref="PublicGetPlaylistByIdQuery" />.
/// </summary>
/// <param name="Playlist">The playlist detail DTO.</param>
public record PublicGetPlaylistByIdResult(PlaylistDetailDto Playlist);
