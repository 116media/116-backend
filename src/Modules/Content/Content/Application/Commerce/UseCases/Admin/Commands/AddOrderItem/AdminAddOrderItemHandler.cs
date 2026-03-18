using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Handles the <see cref="AdminAddOrderItemCommand" /> to add a commissioned content item to a draft order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lookupRepository">Repository for lookup entities including promotion levels.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminAddOrderItemHandler(
    IContentOrderRepository contentOrderRepository,
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
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
        order.EnsureDraft();

        CategoryEntity? category = await categoryRepository.GetByIdAsync(
            id: categoryId,
            cancellationToken: cancellationToken
        );

        if (category is null)
        {
            throw CategoryErrors.NotFound(id: categoryId);
        }

        category.EnsureCommissionable();

        decimal? promoPriceSnapshot = null;
        PromotionLevelEntity? promoLevel = null;

        if (command.PromotionLevelId.HasValue)
        {
            promoLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
                id: command.PromotionLevelId.Value,
                cancellationToken: cancellationToken
            );

            promoLevel.EnsureActive();
            promoPriceSnapshot = promoLevel.PriceUsd;
        }

        var item = ContentOrderItemEntity.Create(
            id: Guid.NewGuid(),
            orderId: orderId,
            contentKind: command.ContentKind,
            categoryId: categoryId,
            promotionLevelId: command.PromotionLevelId,
            promoPriceSnapshotUsd: promoPriceSnapshot,
            socialBoost: command.SocialBoost,
            isBonus: command.IsBonus
        );

        await contentOrderRepository.AddItemAsync(item: item, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = new OrderItemDto(
            Id: item.Id,
            ContentKind: item.ContentKind.ToString(),
            CategoryName: category.Name,
            PromotionLevelName: promoLevel?.Name,
            PromoPriceUsd: promoPriceSnapshot,
            SocialBoost: item.SocialBoost,
            IsBonus: item.IsBonus,
            Tiers: []
        );

        return new AdminAddOrderItemResult(Item: dto);
    }
}
