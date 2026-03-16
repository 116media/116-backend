using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetAllArticles;

/// <summary>
/// Query for retrieving a paginated list of articles for admin management.
/// Supports optional filtering by status and category.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Search">Optional search term matched against title, headline, body, meta title, and meta description.</param>
/// <param name="Status">Optional filter by content status.</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
public record AdminGetAllArticlesQuery(
    PaginatedRequest PaginatedRequest,
    string? Search,
    EnumContentStatus? Status,
    Guid? CategoryId
) : IQuery<AdminGetAllArticlesResult>;

/// <summary>
/// Result of the <see cref="AdminGetAllArticlesQuery" /> containing a paginated list of article summaries.
/// </summary>
/// <param name="Articles">The paginated result containing article summary DTOs.</param>
public record AdminGetAllArticlesResult(PaginatedResult<ArticleSummaryDto> Articles);
