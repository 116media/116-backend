using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Handles the <see cref="AdminAddItemTierCommand" /> to attach a pricing tier snapshot to a specific order item.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="categoryRepository">Repository for category pricing data access operations.</param>
/// <param name="lookupRepository">Repository for pricing tier data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminAddItemTierHandler(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminAddItemTierCommand, AdminAddItemTierResult>
{
    /// <inheritdoc />
    public async Task<AdminAddItemTierResult> Handle(
        AdminAddItemTierCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid orderItemId = Guid.Parse(command.OrderItemId);
        Guid pricingTierId = Guid.Parse(command.PricingTierId);

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

        var dto = new ItemTierDto(TierName: pricingTier.Name, PriceSnapshotUsd: tier.PriceSnapshotUsd);

        return new AdminAddItemTierResult(Tier: dto);
    }
}
