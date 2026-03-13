using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArticleImage;

/// <summary>
/// Handles the <see cref="UploadArticleImageCommand" /> to upload an image for an article.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="cloudinaryService">Service for uploading Cloudinary image assets.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UploadArticleImageHandler(
    IArticleRepository articleRepository,
    ICloudinaryService cloudinaryService,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<UploadArticleImageCommand, UploadArticleImageResult>
{
    /// <inheritdoc />
    public async Task<UploadArticleImageResult> Handle(
        UploadArticleImageCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid articleId = Guid.Parse(command.ArticleId);

        await articleRepository.GetByIdOrThrowAsync(id: articleId, cancellationToken: cancellationToken);

        string storageKey = $"content/article-images/{Guid.NewGuid()}";

        CloudinaryUploadResult uploadResult = await cloudinaryService.UploadImageAsync(
            file: command.File,
            publicId: storageKey,
            folder: "content/article-images",
            cancellationToken: cancellationToken
        );

        ArticleImageEntity image = ArticleImageEntity.Create(
            id: Guid.NewGuid(),
            articleId: articleId,
            storageKey: uploadResult.PublicId,
            url: uploadResult.SecureUrl,
            imageType: command.ImageType
        );

        await articleRepository.AddImageAsync(image: image, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = mapper.Map<ArticleImageDto>(image);
        return new UploadArticleImageResult(Image: dto);
    }
}
