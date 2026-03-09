using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Handles the <see cref="AdminUpdateCategoryCommand" /> to update an existing category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
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
            throw CategoryErrors.AlreadyExists(slug: command.Slug);
        }

        category.Update(name: command.Name, slug: command.Slug, description: command.Description);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToCategoryDto(mapper);
        return new AdminUpdateCategoryResult(Category: dto);
    }
}
