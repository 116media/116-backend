using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Handles the <see cref="AdminUpdateCategoryCommand" /> to update a category, handing the
/// exclusive and default-for-lyrics flags over from their current holders in one transaction.
/// </summary>
/// <param name="updateCategoryService">Service loading and gating the category.</param>
/// <param name="categoryRepository">Repository resolving the current holders and reloading the result.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoService">Service assembling the category DTO.</param>
public class AdminUpdateCategoryHandler(
    IAdminUpdateCategoryService updateCategoryService,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoService categoryDtoService
) : ICommandHandler<AdminUpdateCategoryCommand, AdminUpdateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateCategoryResult> Handle(
        AdminUpdateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity category = await updateCategoryService.EnsureUpdatableAsync(command, cancellationToken);

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                bool hasClearedPredecessor = false;

                if (command.IsExclusive)
                {
                    CategoryEntity? currentExclusive = await categoryRepository.GetExclusiveCategoryAsync(
                        cancellationToken: ct
                    );

                    if (currentExclusive is not null && currentExclusive.Id != category.Id)
                    {
                        currentExclusive.ClearExclusive();
                        hasClearedPredecessor = true;
                    }
                }

                if (command.IsDefaultForLyrics)
                {
                    CategoryEntity? currentDefault = await categoryRepository.GetDefaultLyricsCategoryAsync(
                        cancellationToken: ct
                    );

                    if (currentDefault is not null && currentDefault.Id != category.Id)
                    {
                        currentDefault.ClearDefaultForLyrics();
                        hasClearedPredecessor = true;
                    }
                }

                // The partial unique indexes are checked per statement, so the clear must reach the database before the set.
                if (hasClearedPredecessor)
                {
                    await unitOfWork.CommitAsync(cancellationToken: ct);
                }

                category.Rename(name: command.Name, slug: command.Slug);
                category.Redescribe(description: command.Description);
                category.Reclassify(
                    isGossip: command.IsGossip,
                    isExclusive: command.IsExclusive,
                    isDefaultForLyrics: command.IsDefaultForLyrics
                );
            },
            cancellationToken: cancellationToken
        );

        CategoryEntity updated = await categoryRepository.GetByIdOrThrowAsync(
            id: category.Id,
            cancellationToken: cancellationToken
        );
        var dto = await categoryDtoService.CreateAsync(updated, cancellationToken);
        return new AdminUpdateCategoryResult(Category: dto);
    }
}
