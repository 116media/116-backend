using System.Text.RegularExpressions;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArticle;

/// <summary>
/// Handles the <see cref="AdminUpdateArticleCommand" /> to update all editable article fields.
/// Allowed when the article status is <c>Draft</c>, <c>PendingPayment</c>, <c>PendingReview</c>,
/// or <c>Rejected</c>. Computes an image diff against the previous body and cleans up removed
/// Cloudinary assets after commit.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for deleting Cloudinary image assets.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public partial class AdminUpdateArticleHandler(
    ICategoryRepository categoryRepository,
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService,
    IMapper mapper
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

        if (article.Status is EnumContentStatus.Approved or EnumContentStatus.Published or EnumContentStatus.Archived)
        {
            throw ArticleErrors.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: "Draft/PendingPayment/PendingReview/Rejected (editable)"
            );
        }

        await categoryRepository.GetByIdOrThrowAsync(id: command.CategoryId, cancellationToken: cancellationToken);

        if (command.Slug != article.Slug)
        {
            ArticleEntity? slugConflict = await articleRepository.GetBySlugAsync(
                slug: command.Slug,
                cancellationToken: cancellationToken
            );

            if (slugConflict is not null && slugConflict.Id != article.Id)
            {
                throw ArticleErrors.SlugAlreadyExists(slug: command.Slug);
            }
        }

        IReadOnlyList<ArticleImageEntity> existingImages = await articleRepository.GetImagesByArticleIdAsync(
            articleId: article.Id,
            cancellationToken: cancellationToken
        );

        HashSet<string> newBodyUrls = CloudinaryUrlRegex
            .Matches(command.Body)
            .Select(m => m.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<ArticleImageEntity> imagesToRemove = existingImages
            .Where(img => img.ImageType == EnumArticleImageType.Body && !newBodyUrls.Contains(img.Url))
            .ToList();

        if (article.CoverImageUrl is not null && article.CoverImageUrl != command.CoverImageUrl)
        {
            ArticleImageEntity? oldCover = existingImages.FirstOrDefault(img =>
                img.ImageType == EnumArticleImageType.Cover && img.Url == article.CoverImageUrl
            );

            if (oldCover is not null && !imagesToRemove.Contains(oldCover))
            {
                imagesToRemove.Add(oldCover);
            }
        }

        article.Update(
            categoryId: command.CategoryId,
            title: command.Title,
            slug: command.Slug,
            headline: command.Headline,
            body: command.Body,
            coverImageUrl: command.CoverImageUrl,
            customerId: command.CustomerId,
            orderItemId: command.OrderItemId,
            socialBoost: command.SocialBoost,
            isFeatured: command.IsFeatured,
            featuredUntil: command.FeaturedUntil,
            metaTitle: command.MetaTitle,
            metaDescription: command.MetaDescription
        );

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (imagesToRemove.Count > 0)
        {
            IEnumerable<string> storageKeys = imagesToRemove.Select(img => img.StorageKey);
            await cloudinaryService.DeleteImagesAsync(publicIds: storageKeys, cancellationToken: cancellationToken);

            articleRepository.RemoveImages(images: imagesToRemove);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToArticleDetailDto(mapper);
        return new AdminUpdateArticleResult(Article: dto);
    }

    [GeneratedRegex(
        @"https?://res\.cloudinary\.com/[^\s""'<>]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        "en-RW"
    )]
    private static partial Regex MyRegex();
}
