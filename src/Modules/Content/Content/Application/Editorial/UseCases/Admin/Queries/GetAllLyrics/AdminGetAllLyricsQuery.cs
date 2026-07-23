using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyrics;

/// <summary>
/// Query for retrieving a paginated list of lyrics pages for admin management.
/// Supports optional filtering by search term, status, and category.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against song title, artist name, and lyrics text.</param>
/// <param name="Status">Optional filter by content status.</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
public record AdminGetAllLyricsQuery(
    PaginatedRequest PaginatedRequest,
    string? Search,
    EnumContentStatus? Status,
    Guid? CategoryId
) : IQuery<AdminGetAllLyricsResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllLyricsQuery" /> containing a paginated list of lyrics summaries.
/// </summary>
/// <param name="Lyrics">The paginated result containing lyrics summary DTOs.</param>
public record AdminGetAllLyricsResult(PaginatedResult<LyricsSummaryDto> Lyrics);
