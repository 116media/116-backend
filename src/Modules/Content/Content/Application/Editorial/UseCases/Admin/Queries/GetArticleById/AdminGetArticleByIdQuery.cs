using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Query for retrieving the full details of an article by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the article to retrieve.</param>
public record AdminGetArticleByIdQuery(Guid Id) : IQuery<AdminGetArticleByIdResult>;

/// <summary>
/// Result of the <see cref="AdminGetArticleByIdQuery" /> containing the full article details.
/// </summary>
/// <param name="Article">The detailed article information.</param>
public record AdminGetArticleByIdResult(ArticleDetailDto Article);
