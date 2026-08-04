using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Handles the <see cref="AdminUploadCategoryPosterCommand" /> to upload or replace a category poster image.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="fileRepository">Repository for file storage operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUploadCategoryPosterHandler(
    ICategoryRepository categoryRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUploadCategoryPosterCommand, AdminUploadCategoryPosterResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadCategoryPosterResult> Handle(
        AdminUploadCategoryPosterCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        IFormFile file = command.File!;

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: category.PosterFileId,
            file: file,
            publicId: id.ToString(),
            folder: "content/category-posters",
            originalFileName: file.FileName,
            mimeType: file.ContentType,
            cancellationToken: cancellationToken
        );

        category.SetPosterFileId(posterFileId: fileEntity.Id);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUploadCategoryPosterResult(Category: dto);
    }
}
