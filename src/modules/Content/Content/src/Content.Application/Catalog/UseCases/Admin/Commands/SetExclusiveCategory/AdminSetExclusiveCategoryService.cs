using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Resolves and gates the category for the admin set-exclusive use case.
/// </summary>
/// <param name="categoryRepository">Repository loading the category aggregate.</param>
/// <param name="contentTypeRepository">Repository resolving the category's content type.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSetExclusiveCategoryService(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    ContentI18n i18n
) : IAdminSetExclusiveCategoryService
{
    /// <inheritdoc />
    public async Task<CategoryEntity> EnsureExclusivableAsync(Guid categoryId, CancellationToken cancellationToken)
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );
        ContentTypeEntity contentType = await contentTypeRepository.GetByIdOrThrowAsync(
            id: category.ContentTypeId,
            cancellationToken: cancellationToken
        );

        if (!category.IsActive)
        {
            throw i18n.Category.CannotMakeInactiveExclusive();
        }

        if (contentType.Name != nameof(EnumCoreContentType.Video))
        {
            throw i18n.Category.OnlyVideoCategoryCanBeExclusive();
        }

        return category;
    }
}
