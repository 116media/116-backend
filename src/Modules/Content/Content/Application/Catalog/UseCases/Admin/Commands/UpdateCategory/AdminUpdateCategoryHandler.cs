using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Handles the <see cref="AdminUpdateCategoryCommand" /> to update an existing category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for file storage operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateCategoryCommand, AdminUpdateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateCategoryResult> Handle(
        AdminUpdateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        CategoryEntity? slugConflict = await categoryRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (slugConflict is not null && slugConflict.Id != id)
        {
            throw i18n.Category.AlreadyExists(slug: command.Slug);
        }

        if (command.IsExclusive && !category.IsActive)
        {
            throw i18n.Category.CannotMakeInactiveExclusive();
        }

        if (command.IsExclusive)
        {
            CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                cancellationToken: cancellationToken
            );

            if (currentExclusive is not null && currentExclusive.Id != category.Id)
            {
                currentExclusive.ClearExclusive();
            }
        }

        if (command.Poster is not null)
        {
            FileEntity posterFile = await fileRepository.ReplaceImageFileAsync(
                currentFileId: category.PosterFileId,
                file: command.Poster,
                publicId: category.Id.ToString(),
                folder: "content/category-posters",
                originalFileName: command.Poster.FileName,
                mimeType: command.Poster.ContentType,
                cancellationToken: cancellationToken
            );

            category.SetPosterFileId(posterFileId: posterFile.Id);
        }

        category.Update(
            name: command.Name,
            slug: command.Slug,
            description: command.Description,
            isGossip: command.IsGossip,
            isExclusive: command.IsExclusive,
            errors: i18n.Category
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminUpdateCategoryResult(Category: dto);
    }
}
