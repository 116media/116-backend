using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Handles the <see cref="AdminUploadArticleImageCommand" /> to upload an image for an article.
/// For <c>Cover</c> images, the file is tracked via <see cref="FileEntity" /> and the article's
/// <c>CoverImageFileId</c> is updated.
/// For <c>Body</c> images the public ID is <c>{articleId}-{imageId}</c> and the URL is returned
/// for embedding in the article body HTML.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="cloudinaryService">Service for uploading Cloudinary image assets.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUploadArticleImageHandler(
    IArticleRepository articleRepository,
    ICloudinaryService cloudinaryService,
    IFileRepository fileRepository,
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

        return await HandleBodyImage(articleId, file, command.ImageType, cancellationToken);
    }

    /// <summary>
    /// Handles cover image upload via centralized FileEntity tracking.
    /// </summary>
    private async Task<AdminUploadArticleImageResult> HandleCoverImage(
        ArticleEntity article,
        Guid articleId,
        IFormFile file,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<ArticleImageEntity> existingImages = await articleRepository.GetImagesByArticleIdAsync(
            articleId: articleId,
            cancellationToken: cancellationToken
        );

        ArticleImageEntity? oldCover = existingImages.FirstOrDefault(img =>
            img.ImageType == EnumArticleImageType.Cover
        );

        if (oldCover is not null)
        {
            articleRepository.RemoveImages(images: [oldCover]);
        }

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: article.CoverImageFileId,
            file: file,
            publicId: articleId.ToString(),
            folder: "content/article-images",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        article.UpdateCoverImage(coverImageFileId: fileEntity.Id);
        articleRepository.Update(article: article);

        var image = ArticleImageEntity.Create(
            id: Guid.NewGuid(),
            articleId: articleId,
            storageKey: fileEntity.StorageKey ?? string.Empty,
            url: fileEntity.StorageUrl,
            imageType: EnumArticleImageType.Cover
        );

        await articleRepository.AddImageAsync(image: image, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = mapper.Map<ArticleImageDto>(image);
        return new AdminUploadArticleImageResult(Image: dto);
    }

    /// <summary>
    /// Handles body image upload via direct Cloudinary upload (not tracked by FileEntity).
    /// </summary>
    private async Task<AdminUploadArticleImageResult> HandleBodyImage(
        Guid articleId,
        IFormFile file,
        EnumArticleImageType imageType,
        CancellationToken cancellationToken
    )
    {
        var imageId = Guid.NewGuid();
        string publicId = $"{articleId}-{imageId}";

        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file: file,
            publicId: publicId,
            folder: "content/article-images",
            cancellationToken: cancellationToken
        );

        var image = ArticleImageEntity.Create(
            id: imageId,
            articleId: articleId,
            storageKey: uploadResult.PublicId,
            url: uploadResult.SecureUrl,
            imageType: imageType
        );

        await articleRepository.AddImageAsync(image: image, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = mapper.Map<ArticleImageDto>(image);
        return new AdminUploadArticleImageResult(Image: dto);
    }
}
