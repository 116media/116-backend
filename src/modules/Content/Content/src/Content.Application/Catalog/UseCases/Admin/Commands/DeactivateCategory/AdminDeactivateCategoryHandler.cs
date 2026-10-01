using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Handles the <see cref="AdminDeactivateCategoryCommand" /> to deactivate a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoService">Builds category projections with their posters resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDeactivateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoService categoryDtoService,
    ContentI18n i18n
) : ICommandHandler<AdminDeactivateCategoryCommand, AdminDeactivateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminDeactivateCategoryResult> Handle(
        AdminDeactivateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool deactivated = category.Deactivate();

        if (!deactivated)
        {
            throw i18n.Category.AlreadyInactive();
        }
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await categoryDtoService.CreateAsync(updated, cancellationToken);
        return new AdminDeactivateCategoryResult(Category: dto);
    }
}
