using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Resolves and applies a category pricing row for the admin add-pricing use case.
/// </summary>
/// <param name="categoryRepository">Repository loading the category aggregate.</param>
/// <param name="pricingTierRepository">Repository resolving the priced tier.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAddCategoryPricingService(
    ICategoryRepository categoryRepository,
    IPricingTierRepository pricingTierRepository,
    ContentI18n i18n
) : IAdminAddCategoryPricingService
{
    /// <inheritdoc />
    public async Task<CategoryPricingData> AddAsync(
        Guid categoryId,
        Guid pricingTierId,
        decimal priceUsd,
        CancellationToken cancellationToken
    )
    {
        CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );
        PricingTierEntity pricingTier = await pricingTierRepository.GetByIdOrThrowAsync(
            id: pricingTierId,
            cancellationToken: cancellationToken
        );

        if (!pricingTier.IsActive)
        {
            throw i18n.PricingTier.IsInactive();
        }

        if (category.FindPricing(pricingTierId: pricingTierId) is not null)
        {
            throw i18n.Category.PricingAlreadyExists();
        }

        category.SetPricing(pricingTierId: pricingTierId, priceUsd: priceUsd);

        return new CategoryPricingData(Pricing: category.FindPricing(pricingTierId: pricingTierId)!, Tier: pricingTier);
    }
}
