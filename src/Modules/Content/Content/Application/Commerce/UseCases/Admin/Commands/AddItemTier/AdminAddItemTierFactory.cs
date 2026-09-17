using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Factory implementation for attaching pricing tiers to order items.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="pricingTierRepository">Repository for pricing tier data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
/// <param name="categoryErrors">Category domain error factory.</param>
public class AdminAddItemTierFactory(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    IPricingTierRepository pricingTierRepository,
    IContentUnitOfWork unitOfWork,
    ContentOrderErrors contentOrderErrors,
    CategoryErrors categoryErrors
) : IAddItemTierFactory
{
    /// <inheritdoc />
    public async Task<(ContentItemTierEntity Tier, string TierName)> AttachTierAsync(
        Guid orderId,
        Guid orderItemId,
        Guid pricingTierId,
        CancellationToken cancellationToken
    )
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is not null)
        {
            if (order.Status != EnumOrderStatus.Draft)
            {
                throw contentOrderErrors.CannotAddItemToNonDraftOrder();
            }

            ContentOrderItemEntity? item = order.FindItem(itemId: orderItemId);

            if (item is null)
            {
                throw contentOrderErrors.ItemNotFound(itemId: orderItemId);
            }

            bool alreadyAttached = item.Tiers.Any(t => t.PricingTierId == pricingTierId);
            if (alreadyAttached)
            {
                throw contentOrderErrors.TierAlreadyAttached();
            }

            PricingTierEntity pricingTier = await pricingTierRepository.GetByIdOrThrowAsync(
                id: pricingTierId,
                cancellationToken: cancellationToken
            );

            CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
                id: item.CategoryId,
                cancellationToken: cancellationToken
            );

            CategoryPricingEntity? categoryPricing = category.FindPricing(pricingTierId: pricingTierId);

            if (categoryPricing is null)
            {
                throw categoryErrors.PricingNotFound(categoryId: item.CategoryId, tierId: pricingTierId);
            }

            ContentItemTierEntity tier = order.AddTier(
                item: item,
                pricingTierId: pricingTierId,
                priceSnapshotUsd: categoryPricing.PriceUsd
            );

            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return (tier, pricingTier.Name);
        }

        throw contentOrderErrors.NotFound(id: orderId);
    }
}
