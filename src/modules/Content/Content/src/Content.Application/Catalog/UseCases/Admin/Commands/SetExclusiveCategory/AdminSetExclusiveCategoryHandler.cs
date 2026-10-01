using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.SetExclusiveCategory;

/// <summary>
/// Handles the <see cref="AdminSetExclusiveCategoryCommand" /> to move the exclusive flag onto a
/// category, clearing its current holder in the same transaction.
/// </summary>
/// <param name="setExclusiveService">Service loading and gating the category.</param>
/// <param name="categoryRepository">Repository resolving the current holder and reloading the result.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoService">Service assembling the category DTO.</param>
public class AdminSetExclusiveCategoryHandler(
    IAdminSetExclusiveCategoryService setExclusiveService,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoService categoryDtoService
) : ICommandHandler<AdminSetExclusiveCategoryCommand, AdminSetExclusiveCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminSetExclusiveCategoryResult> Handle(
        AdminSetExclusiveCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        CategoryEntity category = await setExclusiveService.EnsureExclusivableAsync(
            categoryId: id,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                    cancellationToken: ct
                );

                if (currentExclusive is not null && currentExclusive.Id != id)
                {
                    currentExclusive.ClearExclusive();
                }

                category.SetExclusive();
            },
            cancellationToken: cancellationToken
        );

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );
        var dto = await categoryDtoService.CreateAsync(updated, cancellationToken);
        return new AdminSetExclusiveCategoryResult(Category: dto);
    }
}
