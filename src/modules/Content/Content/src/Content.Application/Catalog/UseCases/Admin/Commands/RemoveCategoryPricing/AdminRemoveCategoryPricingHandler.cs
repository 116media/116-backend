using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Handles the <see cref="AdminRemoveCategoryPricingCommand" /> to drop a category's pricing row.
/// </summary>
/// <param name="categoryRepository">Repository loading the category aggregate.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="pricingDtoService">Service assembling the remaining pricing rows.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRemoveCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryPricingDtoService pricingDtoService,
    ContentI18n i18n
) : ICommandHandler<AdminRemoveCategoryPricingCommand, AdminRemoveCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminRemoveCategoryPricingResult> Handle(
        AdminRemoveCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);
        Guid pricingTierId = Guid.Parse(command.PricingTierId);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );

        if (!category.RemovePricing(pricingTierId: pricingTierId))
        {
            throw i18n.Category.PricingNotFound(categoryId: categoryId, tierId: pricingTierId);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        IReadOnlyList<CategoryPricingDto> dtoList = await pricingDtoService.CreateManyAsync(
            category.Pricing,
            cancellationToken
        );
        return new AdminRemoveCategoryPricingResult(Pricing: dtoList, IsSuccess: true);
    }
}
