using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Resolves and applies a category creation for the admin create-category use case.
/// </summary>
/// <param name="contentTypeRepository">Repository resolving the category's content type.</param>
/// <param name="categoryRepository">Repository checking the slug and staging the category.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateCategoryService(
    IContentTypeRepository contentTypeRepository,
    ICategoryRepository categoryRepository,
    ContentI18n i18n
) : IAdminCreateCategoryService
{
    /// <inheritdoc />
    public async Task<CategoryEntity> CreateAsync(
        AdminCreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid contentTypeId = Guid.Parse(command.ContentTypeId);
        ContentTypeEntity contentType = await contentTypeRepository.GetByIdOrThrowAsync(
            id: contentTypeId,
            cancellationToken: cancellationToken
        );

        CategoryEntity? existing = await categoryRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Category.AlreadyExists(slug: command.Slug);
        }

        if (command.IsExclusive)
        {
            if (contentType.Name != nameof(EnumCoreContentType.Video))
            {
                throw i18n.Category.OnlyVideoCategoryCanBeExclusive();
            }

            CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                cancellationToken: cancellationToken
            );
            currentExclusive?.ClearExclusive();
        }

        var category = CategoryEntity.Create(
            id: Guid.NewGuid(),
            contentTypeId: contentTypeId,
            name: command.Name,
            slug: command.Slug,
            description: command.Description,
            isFree: command.IsFree,
            isGossip: command.IsGossip,
            isExclusive: command.IsExclusive
        );

        await categoryRepository.AddAsync(category: category, cancellationToken: cancellationToken);

        return category;
    }
}
