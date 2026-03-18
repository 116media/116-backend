using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Handles the <see cref="AdminAddOrderItemCommand" /> to add a commissioned content item to a draft order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="addOrderItemFactory">Factory for the order item creation flow.</param>
public class AdminAddOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    IAddOrderItemFactory addOrderItemFactory
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

        ContentOrderEntity order = await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        (ContentOrderItemEntity item, string categoryName, string? promotionLevelName) =
            await addOrderItemFactory.CreateItemAsync(
                order: order,
                contentKind: command.ContentKind,
                categoryId: categoryId,
                promotionLevelId: command.PromotionLevelId,
                socialBoost: command.SocialBoost,
                isBonus: command.IsBonus,
                ct: cancellationToken
            );

        var dto = new OrderItemDto(
            Id: item.Id,
            ContentKind: item.ContentKind.ToString(),
            CategoryName: categoryName,
            PromotionLevelName: promotionLevelName,
            PromoPriceUsd: item.PromoPriceSnapshotUsd,
            SocialBoost: item.SocialBoost,
            IsBonus: item.IsBonus,
            Tiers: []
        );

        return new AdminAddOrderItemResult(Item: dto);
    }
}
