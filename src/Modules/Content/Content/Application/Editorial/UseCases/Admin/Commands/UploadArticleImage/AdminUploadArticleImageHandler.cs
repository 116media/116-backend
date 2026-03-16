using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Handles the <see cref="AdminUploadArticleImageCommand" /> to upload an image for an article.
/// For <c>Cover</c> images the article's <c>CoverImageUrl</c> is updated automatically and the
/// public ID is the article ID (Cloudinary overwrites the previous cover on re-upload).
/// For <c>Body</c> images the public ID is <c>{articleId}-{imageId}</c> and the URL is returned
/// for embedding in the article body HTML.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="cloudinaryService">Service for uploading Cloudinary image assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUploadArticleImageHandler(
    IArticleRepository articleRepository,
    ICloudinaryService cloudinaryService,
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

        bool isCover = command.ImageType == EnumArticleImageType.Cover;
        var imageId = Guid.NewGuid();

        string publicId = isCover ? articleId.ToString() : $"{articleId}-{imageId}";

        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file: command.File,
            publicId: publicId,
            folder: "content/article-images",
            cancellationToken: cancellationToken
        );

        if (isCover)
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

            article.UpdateCoverImage(coverImageUrl: uploadResult.SecureUrl);
            articleRepository.Update(article: article);
        }

        var image = ArticleImageEntity.Create(
            id: imageId,
            articleId: articleId,
            storageKey: uploadResult.PublicId,
            url: uploadResult.SecureUrl,
            imageType: command.ImageType
        );

        await articleRepository.AddImageAsync(image: image, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = mapper.Map<ArticleImageDto>(image);
        return new AdminUploadArticleImageResult(Image: dto);
    }
}
