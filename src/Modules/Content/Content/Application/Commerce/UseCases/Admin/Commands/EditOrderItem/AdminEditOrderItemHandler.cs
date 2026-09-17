using _116.Content.Application.Commerce.Factories;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrderItem;

/// <summary>
/// Handles the <see cref="AdminEditOrderItemCommand" /> to edit a content item in a draft order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="promotionLevelRepository">Repository for promotion level data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="orderDtoFactory">Builds order projections with their lookups resolved.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminEditOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    IPromotionLevelRepository promotionLevelRepository,
    IContentUnitOfWork unitOfWork,
    IContentOrderDtoFactory orderDtoFactory,
    ContentI18n i18n
) : ICommandHandler<AdminEditOrderItemCommand, AdminEditOrderItemResult>
{
    /// <inheritdoc />
    public async Task<AdminEditOrderItemResult> Handle(
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
            throw i18n.ContentOrder.NotFound(id: orderId);
        }

        order.EnsureDraft();

        ContentOrderItemEntity? item = order.FindItem(itemId: itemId);

        if (item is null)
        {
            throw i18n.ContentOrder.ItemNotFound(itemId: itemId);
        }

        Guid? newCategoryId = command.CategoryId is not null ? Guid.Parse(command.CategoryId) : null;

        if (newCategoryId.HasValue)
        {
            CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(
                id: newCategoryId.Value,
                cancellationToken: cancellationToken
            );

            category.EnsureCommissionable();
        }

        decimal? promoPriceSnapshot = null;

        if (command.PromotionLevelId.HasValue)
        {
            PromotionLevelEntity promoLevel = await promotionLevelRepository.GetByIdOrThrowAsync(
                id: command.PromotionLevelId.Value,
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
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        OrderItemDto dto = await orderDtoFactory.CreateItemAsync(item, cancellationToken);

        return new AdminEditOrderItemResult(Item: dto);
    }
}
