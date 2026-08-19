using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Factory that populates a draft order with items and tiers from a package's slots.
/// Every category is priced in a single query, so slot count never drives round-trips.
/// </summary>
/// <param name="categoryRepository">Repository for category data access operations.</param>
public class AdminCreateOrderFactory(ICategoryRepository categoryRepository) : ICreateOrderFactory
{
    /// <inheritdoc />
    public async Task<int> PopulateFromPackageAsync(
        ContentOrderEntity order,
        PackageEntity package,
        CancellationToken ct
    )
    {
        List<PackageSlotEntity> slotsWithCategory = [.. package.Slots.Where(s => s.CategoryId.HasValue)];
        Guid[] categoryIds = [.. slotsWithCategory.Select(s => s.CategoryId!.Value).Distinct()];

        List<CategoryPricingEntity> pricingRows =
        [
            .. await categoryRepository.GetPricingByCategoriesAsync(categoryIds: categoryIds, cancellationToken: ct),
        ];

        Dictionary<Guid, List<CategoryPricingEntity>> pricingByCategory = pricingRows
            .GroupBy(p => p.CategoryId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<ContentOrderItemEntity> items =
        [
            .. slotsWithCategory
                .Select(slot =>
                    (
                        Slot: slot,
                        ContentKind: ResolveContentKind(slot),
                        Pricing: pricingByCategory.GetValueOrDefault(slot.CategoryId!.Value) ?? []
                    )
                )
                .SelectMany(resolved =>
                    Enumerable
                        .Range(0, resolved.Slot.Quantity)
                        .Select(_ =>
                            CreateItem(
                                orderId: order.Id,
                                slot: resolved.Slot,
                                contentKind: resolved.ContentKind,
                                pricing: resolved.Pricing
                            )
                        )
                ),
        ];

        order.AddItems(items);
        return items.Count;
    }

    /// <summary>
    /// Maps a slot's content-type name onto the content-kind enum, falling back to
    /// <see cref="EnumCoreContentType.Custom" /> for a name the enum does not declare.
    /// </summary>
    /// <param name="slot">The package slot being filled.</param>
    /// <returns>The resolved content kind.</returns>
    private static EnumCoreContentType ResolveContentKind(PackageSlotEntity slot)
    {
        string contentTypeName = slot.Category!.ContentType.Name;

        return Enum.TryParse(contentTypeName, ignoreCase: true, out EnumCoreContentType contentKind)
            ? contentKind
            : EnumCoreContentType.Custom;
    }

    /// <summary>
    /// Builds one order item for a slot, with a tier snapshotting each of its category's prices.
    /// </summary>
    /// <param name="orderId">The order the item belongs to.</param>
    /// <param name="slot">The slot being filled; a non-required slot yields a bonus item.</param>
    /// <param name="contentKind">The resolved content kind.</param>
    /// <param name="pricing">The category's pricing rows to snapshot.</param>
    /// <returns>The item, with its tiers attached.</returns>
    private static ContentOrderItemEntity CreateItem(
        Guid orderId,
        PackageSlotEntity slot,
        EnumCoreContentType contentKind,
        IReadOnlyList<CategoryPricingEntity> pricing
    )
    {
        var item = ContentOrderItemEntity.Create(
            id: Guid.NewGuid(),
            orderId: orderId,
            contentKind: contentKind,
            categoryId: slot.CategoryId!.Value,
            promotionLevelId: null,
            promoPriceSnapshotUsd: null,
            socialBoost: false,
            isBonus: !slot.IsRequired
        );

        foreach (CategoryPricingEntity price in pricing)
        {
            item.Tiers.Add(
                ContentItemTierEntity.Create(
                    id: Guid.NewGuid(),
                    orderItemId: item.Id,
                    pricingTierId: price.PricingTierId,
                    priceSnapshotUsd: price.PriceUsd
                )
            );
        }

        return item;
    }
}
