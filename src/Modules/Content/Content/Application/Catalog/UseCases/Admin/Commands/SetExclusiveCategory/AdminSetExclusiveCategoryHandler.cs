using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Handles the <see cref="AdminSetExclusiveCategoryCommand" /> to toggle the exclusive flag on a category.
/// Enforces the mutex constraint: only one category can be exclusive at a time.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for file storage operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSetExclusiveCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminSetExclusiveCategoryCommand, AdminSetExclusiveCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminSetExclusiveCategoryResult> Handle(
        AdminSetExclusiveCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        if (!category.IsActive)
        {
            throw i18n.Category.CannotMakeInactiveExclusive();
        }

        if (category.ContentType.Name != nameof(EnumCoreContentType.Video))
        {
            throw i18n.Category.OnlyVideoCategoryCanBeExclusive();
        }

        CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
            cancellationToken: cancellationToken
        );

        if (currentExclusive is not null && currentExclusive.Id != id)
        {
            currentExclusive.ClearExclusive();
        }

        category.SetExclusive();

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await updated.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminSetExclusiveCategoryResult(Category: dto);
    }
}
