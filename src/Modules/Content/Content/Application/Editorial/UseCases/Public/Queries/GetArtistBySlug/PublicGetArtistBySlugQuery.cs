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
/// Result of the <see cref="PublicGetArtistBySlugQuery" /> containing the artist profile and
/// its published catalog.
/// </summary>
/// <param name="Artist">The matched artist profile information.</param>
/// <param name="Lyrics">The artist's paginated published lyrics pages.</param>
/// <param name="Videos">The artist's paginated published videos.</param>
public record PublicGetArtistBySlugResult(
    ArtistDto Artist,
    PaginatedResult<LyricsSummaryDto> Lyrics,
    PaginatedResult<VideoSummaryDto> Videos
);
