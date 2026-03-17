using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Factory implementation for adding order items.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminAddOrderItemFactory(
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork
) : IAddOrderItemFactory
{
    /// <inheritdoc />
    public async Task<(ContentOrderItemEntity Item, string CategoryName, string? PromotionLevelName)> CreateItemAsync(
        ContentOrderEntity order,
        EnumCoreContentType contentKind,
        Guid categoryId,
        Guid? promotionLevelId,
        bool socialBoost,
        bool isBonus,
        CancellationToken cancellationToken
    )
    {
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

        if (promotionLevelId.HasValue)
        {
            promoLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
                id: promotionLevelId.Value,
                cancellationToken: cancellationToken
            );

            promoLevel.EnsureActive();
            promoPriceSnapshot = promoLevel.PriceUsd;
        }

        var item = ContentOrderItemEntity.Create(
            id: Guid.NewGuid(),
            orderId: order.Id,
            contentKind: contentKind,
            categoryId: categoryId,
            promotionLevelId: promotionLevelId,
            promoPriceSnapshotUsd: promoPriceSnapshot,
            socialBoost: socialBoost,
            isBonus: isBonus
        );

        await contentOrderRepository.AddItemAsync(item: item, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return (item, category.Name, promoLevel?.Name);
    }
}
