using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPromotedArticles;

/// <summary>
/// Query for retrieving the list of currently promoted published articles.
/// </summary>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null, the per-user
/// interaction flags on the returned summaries resolve to false.
/// </param>
public record PublicGetPromotedArticlesQuery(Guid? CurrentUserId = null) : IQuery<PublicGetPromotedArticlesResult>;

/// <summary>
/// Result of the <see cref="PublicGetPromotedArticlesQuery" /> containing promoted article summaries.
/// </summary>
/// <param name="Articles">The list of promoted article summary DTOs.</param>
public record PublicGetPromotedArticlesResult(IReadOnlyList<ArticleSummaryDto> Articles);
