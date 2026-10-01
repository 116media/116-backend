using _116.BuildingBlocks.Application.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteArticle;

/// <summary>
/// Resolves and applies a forced unpromotion for the admin force-unpromote use case.
/// </summary>
/// <param name="articleRepository">Repository resolving the article.</param>
/// <param name="currentActor">The acting admin.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
/// <param name="timeProvider">Clock stamping the unpromotion.</param>
public class AdminForceUnpromoteArticleService(
    IArticleRepository articleRepository,
    ICurrentActor currentActor,
    ContentI18n i18n,
    TimeProvider timeProvider
) : IAdminForceUnpromoteArticleService
{
    /// <inheritdoc />
    public async Task<ArticleEntity> UnpromoteAsync(string slug, string reason, CancellationToken cancellationToken)
    {
        ArticleEntity? article = await articleRepository.GetBySlugAsync(
            slug: slug,
            cancellationToken: cancellationToken
        );

        if (article is null)
        {
            throw i18n.Article.NotFound(Guid.Empty);
        }

        article.ForceUnpromote(unpromotedBy: currentActor.UserId!, reason: reason, now: timeProvider.GetUtcNow());

        return article;
    }
}
