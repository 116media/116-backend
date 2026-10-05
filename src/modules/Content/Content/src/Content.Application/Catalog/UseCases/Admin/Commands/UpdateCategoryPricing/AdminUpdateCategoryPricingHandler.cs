using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Catalog.Services;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Handles the <see cref="AdminUpdateCategoryPricingCommand" /> to change a category's price for a tier.
/// </summary>
/// <param name="categoryRepository">Repository loading the category aggregate.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="pricingDtoService">Service assembling the updated pricing row.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateCategoryPricingHandler(
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ICategoryPricingDtoService pricingDtoService,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateCategoryPricingCommand, AdminUpdateCategoryPricingResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateCategoryPricingResult> Handle(
        AdminUpdateCategoryPricingCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid categoryId = Guid.Parse(command.CategoryId);
        Guid pricingTierId = Guid.Parse(command.PricingTierId);

        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );

        CategoryPricingEntity? pricing = category.FindPricing(pricingTierId: pricingTierId);

        if (pricing is null)
        {
            throw i18n.Category.PricingNotFound(categoryId: categoryId, tierId: pricingTierId);
        }

        category.SetPricing(pricingTierId: pricingTierId, priceUsd: command.PriceUsd);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        CategoryPricingDto dto = await pricingDtoService.CreateAsync(pricing, cancellationToken);
        return new AdminUpdateCategoryPricingResult(Pricing: dto);
    }
}
