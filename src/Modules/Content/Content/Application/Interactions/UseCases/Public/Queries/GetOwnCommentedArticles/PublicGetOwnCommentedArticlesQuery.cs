using _116.Content.Application.Shared.DTOs;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetOwnCommentedArticles;

/// <summary>
/// Retrieves published articles with remaining comments by the authenticated user.
/// </summary>
public record PublicGetOwnCommentedArticlesQuery(Guid UserId, PaginatedRequest PaginatedRequest)
    : IQuery<PublicGetOwnCommentedArticlesResult>;

/// <summary>
/// Contains the authenticated user's commented article page.
/// </summary>
public record PublicGetOwnCommentedArticlesResult(PaginatedResult<UserCommentedArticleDto> Articles);
