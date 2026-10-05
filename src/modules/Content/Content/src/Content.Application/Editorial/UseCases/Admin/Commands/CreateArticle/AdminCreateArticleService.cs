using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArticle;

/// <summary>
/// Resolves and applies an article creation for the admin create-article use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="articleRepository">Repository checking the slug and staging the article.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateArticleService(
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    ContentI18n i18n
) : IAdminCreateArticleService
{
    /// <inheritdoc />
    public async Task<ArticleEntity> CreateAsync(AdminCreateArticleCommand command, CancellationToken cancellationToken)
    {
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        ArticleEntity? existing = await articleRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Article.SlugAlreadyExists(slug: command.Slug);
        }

        ArticleEntity article = command.CustomerId.HasValue
            ? ArticleEntity.CreatePaid(
                id: Guid.NewGuid(),
                customerId: command.CustomerId.Value,
                orderItemId: command.OrderItemId!.Value,
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId
            )
            : ArticleEntity.CreateFree(
                id: Guid.NewGuid(),
                categoryId: command.CategoryId,
                title: command.Title,
                slug: command.Slug,
                authorId: command.AuthorId
            );

        await articleRepository.AddAsync(article: article, cancellationToken: cancellationToken);

        return article;
    }
}
