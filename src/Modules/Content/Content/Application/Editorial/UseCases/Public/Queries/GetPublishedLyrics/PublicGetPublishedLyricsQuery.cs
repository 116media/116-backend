using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedLyrics;

/// <summary>
/// Query for retrieving a paginated list of published lyrics pages for public consumption.
/// Supports optional filtering by search term, language, and category.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against song title, artist name, and lyrics text.</param>
/// <param name="Language">Optional filter by ISO 639-1 language code (e.g., "fr", "en").</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
/// <param name="Sort">
/// Optional sort order: <c>"newest"</c> (also the implicit default) sorts by recency;
/// <c>"views"</c>/<c>"likes"</c>/<c>"shares"</c> sort by the matching interaction counter,
/// descending, tie-broken by recency.
/// </param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// <c>IsLiked</c> flag on the returned summaries resolves to false.
/// </param>
public record PublicGetPublishedLyricsQuery(
    PaginatedRequest PaginatedRequest,
    string? Search,
    string? Language,
    Guid? CategoryId,
    string? Sort,
    Guid? CurrentUserId = null
) : IQuery<PublicGetPublishedLyricsResult>;

/// <summary>
/// Result of the <see cref="PublicGetPublishedLyricsQuery" /> containing a paginated list of lyrics summaries.
/// </summary>
/// <param name="Lyrics">The paginated result containing lyrics summary DTOs.</param>
public record PublicGetPublishedLyricsResult(PaginatedResult<LyricsSummaryDto> Lyrics);
