using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Handles the <see cref="AdminUploadArticleImageCommand" /> to upload an image for an article.
/// For <c>Cover</c> images, the file is tracked via <see cref="FileReferenceDto" /> and the article's
/// <c>CoverImageFileId</c> is updated.
/// For <c>Body</c> images the public ID is <c>{articleId}-{imageId}</c> and the URL is returned
/// for embedding in the article body HTML.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUploadArticleImageHandler(
    IArticleRepository articleRepository,
    IFileStorageService fileStorage,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUploadArticleImageCommand, AdminUploadArticleImageResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadArticleImageResult> Handle(
        AdminUploadArticleImageCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid articleId = Guid.Parse(command.ArticleId);

        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: articleId,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.File!;

        bool isCover = command.ImageType == EnumArticleImageType.Cover;

        if (isCover)
        {
            return await HandleCoverImage(article, articleId, file, cancellationToken);
        }

        return await HandleBodyImage(article, file, command.ImageType, cancellationToken);
    }

    /// <summary>
    /// Handles cover image upload via centralized FileReferenceDto tracking.
    /// </summary>
    private async Task<AdminUploadArticleImageResult> HandleCoverImage(
        ArticleEntity article,
        Guid articleId,
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        article.RemoveCoverImage();

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: articleId.ToString(),
            folder: "content/article-images",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        ArticleImageEntity image = article.AddImage(
            id: Guid.NewGuid(),
            storageKey: uploaded.Reference.StorageKey ?? string.Empty,
            url: uploaded.Reference.StorageUrl,
            imageType: EnumArticleImageType.Cover
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: article.CoverImageFileId,
                    cancellationToken: ct
                );

                article.UpdateCoverImage(coverImageFileId: uploaded.Reference.Id);
            },
            cancellationToken: cancellationToken
        );

        var dto = mapper.Map<ArticleImageDto>(image);
        return new AdminUploadArticleImageResult(Image: dto);
    }

    /// <summary>
    /// Handles body image upload via direct Cloudinary upload (not tracked by FileReferenceDto).
    /// </summary>
    private async Task<AdminUploadArticleImageResult> HandleBodyImage(
        ArticleEntity article,
        IFormFile file,
        EnumArticleImageType imageType,
        CancellationToken cancellationToken
    )
    {
        var imageId = Guid.NewGuid();
        string publicId = $"{article.Id}-{imageId}";

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: publicId,
            folder: "content/article-images",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        ArticleImageEntity image = article.AddImage(
            id: imageId,
            storageKey: uploaded.Reference.StorageKey ?? string.Empty,
            url: uploaded.Reference.StorageUrl,
            imageType: imageType
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = mapper.Map<ArticleImageDto>(image);
        return new AdminUploadArticleImageResult(Image: dto);
    }
}
