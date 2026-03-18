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
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminAddItemTierFactory(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
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

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: orderId);
        }

        if (order.Status != EnumOrderStatus.Draft)
        {
            throw ContentOrderErrors.CannotAddItemToNonDraftOrder();
        }

        ContentOrderItemEntity? item = await contentOrderRepository.GetItemByIdAsync(
            orderId: orderId,
            itemId: orderItemId,
            ct: cancellationToken
        );
        if (item is null)
        {
            throw ContentOrderErrors.ItemNotFound(itemId: orderItemId);
        }

        PricingTierEntity pricingTier = await lookupRepository.GetPricingTierByIdOrThrowAsync(
            id: pricingTierId,
            cancellationToken: cancellationToken
        );

        CategoryPricingEntity? categoryPricing = await categoryRepository.GetPricingAsync(
            categoryId: item.CategoryId,
            pricingTierId: pricingTierId,
            cancellationToken: cancellationToken
        );

        if (categoryPricing is null)
        {
            throw PricingTierErrors.NotFound(id: pricingTierId);
        }

        var tier = ContentItemTierEntity.Create(
            id: Guid.NewGuid(),
            orderItemId: orderItemId,
            pricingTierId: pricingTierId,
            priceSnapshotUsd: categoryPricing.PriceUsd
        );

        await contentOrderRepository.AddItemTierAsync(tier: tier, ct: cancellationToken);
        order.RecalculateTotal(newTierPrice: categoryPricing.PriceUsd);

        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return (tier, pricingTier.Name);
    }
}
