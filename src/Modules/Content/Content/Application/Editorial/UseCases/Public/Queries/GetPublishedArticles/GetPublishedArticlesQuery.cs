using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPublishedArticles;

/// <summary>
/// Query for retrieving a paginated list of published articles for public consumption.
/// Supports optional filtering by category and tag slug.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="CategoryId">Optional filter by category identifier.</param>
/// <param name="TagSlug">Optional filter by tag slug (aspirational — reserved for future use).</param>
public record GetPublishedArticlesQuery(PaginatedRequest PaginatedRequest, Guid? CategoryId, string? TagSlug)
    : IQuery<GetPublishedArticlesResult>;

/// <summary>
/// Result of the <see cref="GetPublishedArticlesQuery" /> containing a paginated list of article summaries.
/// </summary>
/// <param name="Articles">The paginated result containing article summary DTOs.</param>
public record GetPublishedArticlesResult(PaginatedResult<ArticleSummaryDto> Articles);
