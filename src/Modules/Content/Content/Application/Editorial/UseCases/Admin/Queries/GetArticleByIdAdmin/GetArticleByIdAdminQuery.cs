using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleByIdAdmin;

/// <summary>
/// Query for retrieving the full details of an article by its unique identifier.
/// </summary>
/// <param name="Id">The unique identifier of the article to retrieve.</param>
public record GetArticleByIdAdminQuery(string Id) : IQuery<GetArticleByIdAdminResult>;

/// <summary>
/// Result of the <see cref="GetArticleByIdAdminQuery" /> containing the full article details.
/// </summary>
/// <param name="Article">The detailed article information.</param>
public record GetArticleByIdAdminResult(ArticleDetailDto Article);
