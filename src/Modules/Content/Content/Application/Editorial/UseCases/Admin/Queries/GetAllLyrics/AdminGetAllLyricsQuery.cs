using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;

/// <summary>
/// Query for retrieving a paginated list of lyrics pages for admin management.
/// Supports optional filtering by search term.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against song title, artist name, and body.</param>
public record AdminGetAllLyricsQuery(PaginatedRequest PaginatedRequest, string? Search)
    : IQuery<AdminGetAllLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllLyricsQuery" /> containing a paginated list of lyrics DTOs.
/// </summary>
/// <param name="Lyrics">The paginated result containing lyrics DTOs.</param>
public record AdminGetAllLyricsResult(PaginatedResult<LyricsDto> Lyrics);
