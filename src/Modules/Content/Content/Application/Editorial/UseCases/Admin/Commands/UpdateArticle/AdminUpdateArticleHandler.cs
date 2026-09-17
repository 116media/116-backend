using System.Text.RegularExpressions;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleCommand" /> to update all editable article fields.
/// Allowed when the article status is <c>Draft</c>, <c>PendingPayment</c>, <c>PendingReview</c>,
/// or <c>Rejected</c>. Computes an image diff against the previous body and hands the orphaned
/// storage keys to the aggregate, so row removal and remote-asset cleanup run post-commit in
/// one place with one retry story.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public partial class AdminUpdateArticleHandler(
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IFileStorageService fileStorage,
    IMapper mapper,
    ContentI18n i18n,
    IContentLookupFactory contentLookupFactory
) : ICommandHandler<AdminUpdateArticleCommand, AdminUpdateArticleResult>
{
    private static readonly Regex CloudinaryUrlRegex = MyRegex();

    /// <inheritdoc />
    public async Task<AdminUpdateArticleResult> Handle(
        AdminUpdateArticleCommand command,
        CancellationToken cancellationToken
    )
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
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToArticleDetailDtoAsync(
            mapper,
            await contentLookupFactory.ResolveForArticlesAsync([updated], cancellationToken),
            fileStorage,
            cancellationToken
        );
        return new AdminUpdateArticleResult(Article: dto);
    }

    [GeneratedRegex(
        @"https?://res\.cloudinary\.com/[^\s""'<>]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-RW"
    )]
    private static partial Regex MyRegex();
}
