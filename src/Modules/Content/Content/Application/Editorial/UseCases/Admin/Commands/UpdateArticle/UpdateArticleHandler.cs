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
/// Handles the <see cref="UpdateArticleCommand" /> to save article content (step 2).
/// Computes an image diff against the previous body and cleans up removed Cloudinary assets.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cloudinaryService">Service for deleting Cloudinary image assets.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    ICloudinaryService cloudinaryService,
    IMapper mapper
) : ICommandHandler<UpdateArticleCommand, UpdateArticleResult>
{
    private static readonly Regex CloudinaryUrlRegex = new(
        @"https?://res\.cloudinary\.com/[^\s""'<>]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <inheritdoc />
    public async Task<UpdateArticleResult> Handle(UpdateArticleCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (article.Status != EnumContentStatus.Draft && article.Status != EnumContentStatus.Rejected)
        {
            throw ArticleErrors.InvalidStatusTransition(
                from: article.Status.ToString(),
                to: "Draft/Rejected (editable)"
            );
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

        article.UpdateContent(headline: command.Headline, body: command.Body, coverImageUrl: command.CoverImageUrl);

        articleRepository.Update(article: article);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        if (imagesToRemove.Count > 0)
        {
            IEnumerable<string> storageKeys = imagesToRemove.Select(img => img.StorageKey);
            await cloudinaryService.DeleteImagesAsync(storageKeys: storageKeys, cancellationToken: cancellationToken);

            articleRepository.RemoveImages(images: imagesToRemove);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        ArticleEntity updated = await articleRepository.GetByIdOrThrowAsync(
            id: article.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToArticleDetailDto(mapper);
        return new UpdateArticleResult(Article: dto);
    }
}
