using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Resolves and gates the category for the admin update-category use case.
/// </summary>
/// <param name="categoryRepository">Repository loading the category and checking the slug.</param>
/// <param name="contentTypeRepository">Repository resolving the category's content type.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateCategoryService(
    ICategoryRepository categoryRepository,
    IContentTypeRepository contentTypeRepository,
    ContentI18n i18n
) : IAdminUpdateCategoryService
{
    /// <inheritdoc />
    public async Task<CategoryEntity> EnsureUpdatableAsync(
        AdminUpdateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );
        ContentTypeEntity contentType = await contentTypeRepository.GetByIdOrThrowAsync(
            id: category.ContentTypeId,
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

        if (command.IsExclusive)
        {
            if (!category.IsActive)
            {
                throw i18n.Category.CannotMakeInactiveExclusive();
            }

            if (contentType.Name != nameof(EnumCoreContentType.Video))
            {
                throw i18n.Category.OnlyVideoCategoryCanBeExclusive();
            }
        }

        if (command.IsDefaultForLyrics)
        {
            if (!category.IsActive)
            {
                throw i18n.Category.CannotMakeInactiveDefaultForLyrics();
            }

            if (contentType.Name != nameof(EnumCoreContentType.Lyrics))
            {
                throw i18n.Category.OnlyLyricsCategoryCanBeDefault();
            }
        }

        return category;
    }
}
