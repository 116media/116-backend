using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Handles the <see cref="AdminAddOrderItemCommand" /> to add a commissioned content item to a draft order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="addOrderItemService">Service for the order item creation flow.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAddOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    IAddOrderItemService addOrderItemService,
    ContentI18n i18n
) : ICommandHandler<AdminAddOrderItemCommand, AdminAddOrderItemResult>
{
    /// <inheritdoc />
    public async Task<AdminAddOrderItemResult> Handle(
        AdminAddOrderItemCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid categoryId = Guid.Parse(command.CategoryId);

        // Items and tiers are loaded so the service's total recalculation sees the whole order.
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw i18n.ContentOrder.NotFound(id: orderId);
        }

        (ContentOrderItemEntity item, string categoryName, string? promotionLevelName) =
            await addOrderItemService.CreateItemAsync(
                order: order,
                categoryId: categoryId,
                contentKind: command.ContentKind,
                promotionLevelId: command.PromotionLevelId,
                socialBoost: command.SocialBoost,
                isBonus: command.IsBonus,
                ct: cancellationToken
            );

        var dto = new OrderItemDto(
            Id: item.Id,
            CategoryName: categoryName,
            CategoryId: item.CategoryId,
            ContentKind: item.ContentKind,
            PromotionLevelId: item.PromotionLevelId,
            PromotionLevelName: promotionLevelName,
            PromoPriceUsd: item.PromoPriceSnapshotUsd?.Amount,
            SocialBoost: item.SocialBoost,
            IsBonus: item.IsBonus,
            Tiers: []
        );

        return new AdminAddOrderItemResult(Item: dto);
    }
}
