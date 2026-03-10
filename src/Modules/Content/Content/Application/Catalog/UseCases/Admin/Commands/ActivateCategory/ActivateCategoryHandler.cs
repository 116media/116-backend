using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Handles the <see cref="ActivateCategoryCommand" /> to activate a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class ActivateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<ActivateCategoryCommand, ActivateCategoryResult>
{
    /// <inheritdoc />
    public async Task<ActivateCategoryResult> Handle(
        ActivateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool activated = category.Activate();

        if (!activated)
        {
            throw CategoryErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        var dto = updated.ToCategoryDto(mapper);
        return new ActivateCategoryResult(Category: dto);
    }
}
