using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetArtistBySlug;

/// <summary>
/// Query for retrieving an artist's public profile page by its URL-safe slug, including
/// paginated published lyrics and videos linked to the artist.
/// </summary>
/// <param name="Slug">The URL-safe slug of the artist profile.</param>
/// <param name="LyricsPage">Pagination parameters for the artist's published lyrics.</param>
/// <param name="VideosPage">Pagination parameters for the artist's published videos.</param>
public record PublicGetArtistBySlugQuery(string Slug, PaginatedRequest LyricsPage, PaginatedRequest VideosPage)
    : IQuery<PublicGetArtistBySlugResult>;

/// <summary>
/// Result of the <see cref="PublicGetArtistBySlugQuery" /> containing the artist profile,
/// every surface total, and its published catalog.
/// </summary>
/// <param name="Artist">The matched artist profile information.</param>
/// <param name="Totals">
/// Every per-surface item count, shipped before any tab is opened so the client can render
/// the stat row, hide empty tabs, and resolve the default tab server-side with no flash of
/// the wrong panel.
/// </param>
/// <param name="Lyrics">The artist's paginated published lyrics pages.</param>
/// <param name="Videos">The artist's paginated published videos.</param>
public record PublicGetArtistBySlugResult(
    ArtistDto Artist,
    ArtistTotalsDto Totals,
    PaginatedResult<LyricsSummaryDto> Lyrics,
    PaginatedResult<VideoSummaryDto> Videos
);

/// <summary>
/// An artist's per-surface item counts. Albums and mixtapes are separate — they are two
/// sections that hide independently. The sum of all five is the profile's 404 predicate.
/// </summary>
/// <param name="Songs">Published lyrics pages where this artist is the primary credit.</param>
/// <param name="Videos">Published videos where this artist is the primary credit.</param>
/// <param name="Albums">Releases typed Album linked to this artist.</param>
/// <param name="Mixtapes">Releases typed Mixtape linked to this artist.</param>
/// <param name="News">Published articles tagged to this artist.</param>
public record ArtistTotalsDto(int Songs, int Videos, int Albums, int Mixtapes, int News);
