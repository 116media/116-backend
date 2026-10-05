using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Handles the <see cref="AdminCreateCategoryCommand" /> to create a category.
/// </summary>
/// <param name="createCategoryService">Service resolving and staging the category.</param>
/// <param name="categoryRepository">Repository reloading the committed category.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryDtoService">Service assembling the category DTO.</param>
public class AdminCreateCategoryHandler(
    IAdminCreateCategoryService createCategoryService,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryDtoService categoryDtoService
) : ICommandHandler<AdminCreateCategoryCommand, AdminCreateCategoryResult>
{
    /// <inheritdoc />
    public async Task<AdminCreateCategoryResult> Handle(
        AdminCreateCategoryCommand command,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity category = await createCategoryService.CreateAsync(command, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryEntity created = await categoryRepository.GetByIdOrThrowAsync(
            id: category.Id,
            cancellationToken: cancellationToken
        );
        var dto = await categoryDtoService.CreateAsync(created, cancellationToken);
        return new AdminCreateCategoryResult(Category: dto);
    }
}
