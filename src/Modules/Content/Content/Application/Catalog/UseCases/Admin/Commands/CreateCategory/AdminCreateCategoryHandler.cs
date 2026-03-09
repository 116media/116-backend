using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Handles the <see cref="AdminCreateCategoryCommand" /> to create a new content category.
/// </summary>
/// <param name="lookupRepository">Repository for verifying lookup entities (content type).</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminCreateCategoryHandler(
    ILookupRepository lookupRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminCreateCategoryCommand, AdminCreateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateCategoryResult> Handle(
        AdminCreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid contentTypeId = Guid.Parse(command.ContentTypeId);

        await lookupRepository.GetContentTypeByIdOrThrowAsync(id: contentTypeId, cancellationToken: cancellationToken);

        CategoryEntity? existing = await categoryRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw CategoryErrors.AlreadyExists(slug: command.Slug);
        }

        var category = CategoryEntity.Create(
            id: Guid.NewGuid(),
            contentTypeId: contentTypeId,
            name: command.Name,
            slug: command.Slug,
            description: command.Description,
            isFree: command.IsFree
        );

        await categoryRepository.AddAsync(category: category, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity created = await categoryRepository.GetByIdOrThrowAsync(
            id: category.Id,
            cancellationToken: cancellationToken
        );

        var dto = created.ToCategoryDto(mapper);
        return new AdminCreateCategoryResult(Category: dto);
    }
}
