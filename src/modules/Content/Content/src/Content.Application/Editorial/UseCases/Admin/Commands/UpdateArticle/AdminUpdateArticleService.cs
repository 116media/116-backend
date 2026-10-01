using System.Text.RegularExpressions;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Resolves and applies an article update for the admin update-article use case.
/// </summary>
/// <param name="categoryRepository">Repository validating the category.</param>
/// <param name="articleRepository">Repository loading the article and checking the slug.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public partial class AdminUpdateArticleService(
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    ContentI18n i18n
) : IAdminUpdateArticleService
{
    private static readonly Regex CloudinaryUrlRegex = MyRegex();

    /// <inheritdoc />
    public async Task<ArticleEntity> UpdateAsync(AdminUpdateArticleCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );
        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        if (command.Slug != article.Slug)
        {
            ArticleEntity? slugConflict = await articleRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (slugConflict is not null && slugConflict.Id != article.Id)
            {
                throw i18n.Article.SlugAlreadyExists(slug: command.Slug);
            }
        }

        HashSet<string> newBodyUrls = CloudinaryUrlRegex
            .Matches(command.Body)
            .Select(m => m.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<string> orphanedStorageKeys = article
            .Images.Where(img => img.ImageType == EnumArticleImageType.Body && !newBodyUrls.Contains(img.Url))
            .Select(img => img.StorageKey)
            .ToList();

        article.Recategorize(categoryId: command.CategoryId);
        article.Retitle(title: command.Title, slug: command.Slug);
        article.ReviseBody(
            headline: command.Headline,
            body: command.Body,
            orphanedBodyImageStorageKeys: orphanedStorageKeys
        );
        article.AssignCommission(
            customerId: command.CustomerId,
            orderItemId: command.OrderItemId,
            socialBoost: command.SocialBoost
        );
        article.ReviseSeo(metaTitle: command.MetaTitle, metaDescription: command.MetaDescription);

        return article;
    }

    [GeneratedRegex(
        @"https?://res\.cloudinary\.com/[^\s""'<>]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-RW"
    )]
    private static partial Regex MyRegex();
}
