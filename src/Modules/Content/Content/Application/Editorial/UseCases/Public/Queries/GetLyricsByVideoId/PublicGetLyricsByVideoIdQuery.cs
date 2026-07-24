using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsByVideoId;

/// <summary>
/// Query for retrieving the lyrics page linked to a video.
/// </summary>
/// <param name="VideoId">The unique identifier of the video.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the returned
/// lyrics page's <c>IsLiked</c> flag resolves to false.
/// </param>
public record PublicGetLyricsByVideoIdQuery(string VideoId, Guid? CurrentUserId = null)
    : IQuery<PublicGetLyricsByVideoIdResult>;

/// <summary>
/// Result of the <see cref="PublicGetLyricsByVideoIdQuery" /> containing the matching lyrics page.
/// </summary>
/// <param name="Lyrics">The lyrics information linked to the video.</param>
public record PublicGetLyricsByVideoIdResult(LyricsDetailDto Lyrics);
