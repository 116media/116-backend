using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.AddOrderItem;

/// <summary>
/// Factory implementation for adding order items.
/// Forces IsBonus = true when the order's package slot capacity is exceeded.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="packageRepository">Repository for package data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="categoryErrors">Category domain error factory.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
/// <param name="promotionLevelErrors">Promotion level domain error factory.</param>
public class AdminAddOrderItemFactory(
    ICategoryRepository categoryRepository,
    ILookupRepository lookupRepository,
    IPackageRepository packageRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    CategoryErrors categoryErrors,
    ContentOrderErrors contentOrderErrors,
    PromotionLevelErrors promotionLevelErrors
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

        if (category is not null)
        {
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

            bool forcedBonus = isBonus;

            if (order.PackageId.HasValue)
            {
                PackageEntity? package = await packageRepository.GetByIdWithSlotsAsync(
                    id: order.PackageId.Value,
                    cancellationToken: cancellationToken
                );

                if (package is not null)
                {
                    int slotCapacity = package.Slots.Sum(s => s.Quantity);
                    int currentItemCount = order.Items.Count;
                    if (currentItemCount >= slotCapacity)
                    {
                        forcedBonus = true;
                    }
                }
            }

            var item = ContentOrderItemEntity.Create(
                id: Guid.NewGuid(),
                orderId: order.Id,
                contentKind: contentKind,
                categoryId: categoryId,
                promotionLevelId: promotionLevelId,
                promoPriceSnapshotUsd: promoPriceSnapshot,
                socialBoost: socialBoost,
                isBonus: forcedBonus
            );

            await contentOrderRepository.AddItemAsync(item: item, ct: cancellationToken);

            order.AddItem(item);
            await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

            return (item, category.Name, promoLevel?.Name);
        }

        throw categoryErrors.NotFound(id: categoryId);
    }
}
