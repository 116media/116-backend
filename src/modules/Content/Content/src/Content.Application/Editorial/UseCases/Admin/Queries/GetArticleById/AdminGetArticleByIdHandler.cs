using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Handles the <see cref="AdminGetArticleByIdQuery" /> to serve an article with its author profile.
/// </summary>
/// <param name="articleRepository">Repository loading the article.</param>
/// <param name="articleDtoService">Service assembling the article detail with its author.</param>
public class AdminGetArticleByIdHandler(IArticleRepository articleRepository, IArticleDtoService articleDtoService)
    : IQueryHandler<AdminGetArticleByIdQuery, AdminGetArticleByIdResult>
{
    /// <inheritdoc />
    public async Task<AdminGetArticleByIdResult> Handle(
        AdminGetArticleByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: query.Id,
            cancellationToken: cancellationToken
        );

        var dto = await articleDtoService.CreateDetailWithAuthorAsync(article, cancellationToken);
        return new AdminGetArticleByIdResult(Article: dto);
    }
}
