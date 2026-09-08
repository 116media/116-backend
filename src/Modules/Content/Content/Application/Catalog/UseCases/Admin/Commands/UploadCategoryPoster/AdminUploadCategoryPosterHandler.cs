using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UploadCategoryPoster;

/// <summary>
/// Handles the <see cref="AdminUploadCategoryPosterCommand" /> to upload or replace a category poster image.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUploadCategoryPosterHandler(
    ICategoryRepository categoryRepository,
    IFileStorageService fileStorage,
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

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: id.ToString(),
            folder: "content/category-posters",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: category.PosterFileId,
                    cancellationToken: ct
                );

                category.SetPosterFileId(posterFileId: uploaded.Reference.Id);
            },
            cancellationToken: cancellationToken
        );

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileStorage, cancellationToken);
        return new AdminUploadCategoryPosterResult(Category: dto);
    }
}
