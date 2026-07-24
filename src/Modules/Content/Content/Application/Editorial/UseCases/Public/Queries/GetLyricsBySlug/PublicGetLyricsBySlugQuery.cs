using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsBySlug;

/// <summary>
/// Query for retrieving a lyrics page by its URL-safe slug.
/// </summary>
/// <param name="Slug">The URL-safe slug of the lyrics page.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the returned
/// lyrics page's <c>IsLiked</c> flag resolves to false.
/// </param>
public record PublicGetLyricsBySlugQuery(string Slug, Guid? CurrentUserId = null) : IQuery<PublicGetLyricsBySlugResult>;

/// <summary>
/// Response-only summary of another track on the same album as the requested lyrics page.
/// Not part of <see cref="LyricsDetailDto" /> — detail-page-specific, like <c>VideoSlug</c>/
/// <c>ArtistSlug</c>.
/// </summary>
/// <param name="Slug">The URL-safe slug of the sibling track's lyrics page.</param>
/// <param name="SongTitle">The sibling track's song title.</param>
public record AlbumTrackDto(string Slug, string SongTitle);

/// <summary>
/// Response-only resolved streaming platform deep link for the requested lyrics page's release
/// (either the parent album or the standalone single itself). Not part of
/// <see cref="LyricsDetailDto" /> — detail-page-specific, like <c>VideoSlug</c>/<c>ArtistSlug</c>.
/// </summary>
/// <param name="Platform">The streaming platform name (e.g. "Spotify", "AppleMusic").</param>
/// <param name="Url">The curated deep link URL, or a generated search-query fallback.</param>
public record StreamingLinkDto(string Platform, string Url);

/// <summary>
/// Result of the <see cref="PublicGetLyricsBySlugQuery" /> containing the matching lyrics page.
/// </summary>
/// <param name="Lyrics">The matched lyrics information.</param>
/// <param name="VideoSlug">
/// The slug of the linked video, or null if this lyrics page is standalone or the linked
/// video no longer exists.
/// </param>
/// <param name="ArtistSlug">
/// The slug of the linked artist profile, or null if this lyrics page has no linked
/// <see cref="_116.Content.Domain.Entities.ArtistEntity" /> or the linked profile no longer exists.
/// </param>
/// <param name="AlbumTracks">
/// Other published tracks from the same album, excluding this one. Empty when the lyrics page
/// has no linked album (a standalone single).
/// </param>
/// <param name="StreamingLinks">
/// The resolved streaming platform deep links for this release — always populated for both an
/// album track and a standalone single, either curated or generated.
/// </param>
public record PublicGetLyricsBySlugResult(
    LyricsDetailDto Lyrics,
    string? VideoSlug,
    string? ArtistSlug,
    IReadOnlyList<AlbumTrackDto> AlbumTracks,
    IReadOnlyList<StreamingLinkDto> StreamingLinks
);
