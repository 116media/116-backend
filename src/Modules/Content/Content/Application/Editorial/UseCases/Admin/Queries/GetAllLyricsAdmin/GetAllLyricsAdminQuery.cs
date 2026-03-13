using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllLyricsAdmin;

/// <summary>
/// Query for retrieving a paginated list of lyrics pages for admin management.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
public record GetAllLyricsAdminQuery(PaginatedRequest PaginatedRequest) : IQuery<GetAllLyricsAdminResult>;

/// <summary>
/// Result of the <see cref="GetAllLyricsAdminQuery" /> containing a paginated list of lyrics DTOs.
/// </summary>
/// <param name="Lyrics">The paginated result containing lyrics DTOs.</param>
public record GetAllLyricsAdminResult(PaginatedResult<LyricsDto> Lyrics);
