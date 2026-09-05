using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Handles the <see cref="AdminCreateCategoryCommand" /> to create a new content category.
/// </summary>
/// <param name="lookupRepository">Repository for verifying lookup entities (content type).</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="fileRepository">Repository for file storage operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminCreateCategoryHandler(
    ILookupRepository lookupRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminCreateCategoryCommand, AdminCreateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateCategoryResult> Handle(
        AdminCreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid contentTypeId = Guid.Parse(command.ContentTypeId);

        ContentTypeEntity contentType = await lookupRepository.GetContentTypeByIdOrThrowAsync(
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

            if (currentExclusive is not null)
            {
                // Clear the previous exclusive first
                currentExclusive.ClearExclusive();
                await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
            }
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
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity created = await categoryRepository.GetByIdOrThrowAsync(
            id: category.Id,
            cancellationToken: cancellationToken
        );

        var dto = await created.ToCategoryDtoAsync(mapper, fileRepository, cancellationToken);
        return new AdminCreateCategoryResult(Category: dto);
    }
}
