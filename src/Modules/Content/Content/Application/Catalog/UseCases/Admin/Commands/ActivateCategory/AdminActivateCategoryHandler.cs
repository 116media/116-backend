using _116.Content.Application.Catalog.Factories;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Handles the <see cref="AdminActivateCategoryCommand" /> to activate a category.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoFactory">Builds category projections with their posters resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminActivateCategoryHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoFactory categoryDtoFactory,
    ContentI18n i18n
) : ICommandHandler<AdminActivateCategoryCommand, AdminActivateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminActivateCategoryResult> Handle(
        AdminActivateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool activated = category.Activate();

        if (!activated)
        {
            throw i18n.Category.AlreadyActive();
        }

        categoryRepository.Update(category: category);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        var dto = await categoryDtoFactory.CreateAsync(updated, cancellationToken);
        return new AdminActivateCategoryResult(Category: dto);
    }
}
