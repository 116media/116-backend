using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Resolves and applies the edit for the admin edit-order-item use case.
/// </summary>
/// <param name="contentOrderRepository">Repository loading the order with its items.</param>
/// <param name="categoryRepository">Repository validating the new category.</param>
/// <param name="promotionLevelRepository">Repository validating the new promotion level.</param>
public class AdminEditOrderItemService(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    IPromotionLevelRepository promotionLevelRepository
) : IAdminEditOrderItemService
{
    /// <inheritdoc />
    public async Task<ContentOrderItemEntity> EditAsync(
        AdminEditOrderItemCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);
        Guid itemId = Guid.Parse(command.ItemId);

        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: orderId);
        }

        order.EnsureDraft();
        ContentOrderItemEntity? item = order.FindItem(itemId: itemId);

        if (item is null)
        {
            throw ContentOrderErrors.ItemNotFound(itemId: itemId);
        }

        Guid? newCategoryId = command.CategoryId is not null ? Guid.Parse(command.CategoryId) : null;

        if (newCategoryId is { } categoryId)
        {
            CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
                id: categoryId,
                cancellationToken: cancellationToken
            );
            category.EnsureCommissionable();
        }

        decimal? promoPriceSnapshot = null;

        if (command.PromotionLevelId is { } promotionLevelId)
        {
            PromotionLevelEntity promoLevel = await promotionLevelRepository.GetByIdOrThrowAsync(
                id: promotionLevelId,
                cancellationToken: cancellationToken
            );
            promoLevel.EnsureActive();
            promoPriceSnapshot = promoLevel.PriceUsd;
        }

        item.Update(
            categoryId: newCategoryId,
            contentKind: command.ContentKind,
            promotionLevelId: command.PromotionLevelId,
            promoPriceSnapshotUsd: promoPriceSnapshot,
            socialBoost: command.SocialBoost,
            isBonus: command.IsBonus
        );
        order.RecalculateTotalFromItems();

        return item;
    }
}
