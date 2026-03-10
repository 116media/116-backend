using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Handles the <see cref="UpdateCategoryCommand" /> to update an existing category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class UpdateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<UpdateCategoryCommand, UpdateCategoryResult>
{
    /// <inheritdoc />
    public async Task<UpdateCategoryResult> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        CategoryEntity? slugConflict = await categoryRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (slugConflict is not null && slugConflict.Id != command.Id)
        {
            throw CategoryErrors.AlreadyExists(slug: command.Slug);
        }

        category.Update(name: command.Name, slug: command.Slug, description: command.Description);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToCategoryDto(mapper);
        return new UpdateCategoryResult(Category: dto);
    }
}
