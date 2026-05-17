using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Factory that populates a draft order with items and tiers from a package's slots.
/// Batch-fetches category pricing upfront to avoid nested async loops.
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
        IEnumerable<PackageSlotEntity> slotsWithCategory = package.Slots.Where(s => s.CategoryId.HasValue);

        IEnumerable<Guid> distinctCategoryIds = slotsWithCategory.Select(s => s.CategoryId!.Value).Distinct();

        Dictionary<Guid, IReadOnlyList<CategoryPricingEntity>> pricingByCategory = new();

        foreach (Guid categoryId in distinctCategoryIds)
        {
            IReadOnlyList<CategoryPricingEntity> pricing = await categoryRepository.GetPricingByCategoryAsync(
                categoryId: categoryId,
                cancellationToken: ct
            );
            pricingByCategory[categoryId] = pricing;
        }

        List<(ContentOrderItemEntity Item, IReadOnlyList<CategoryPricingEntity> Pricing)> itemsToCreate = [];

        foreach (PackageSlotEntity slot in slotsWithCategory)
        {
            Guid categoryId = slot.CategoryId!.Value;
            IReadOnlyList<CategoryPricingEntity> pricing = pricingByCategory.GetValueOrDefault(categoryId) ?? [];

            string contentTypeName = slot.Category!.ContentType.Name;
            if (!Enum.TryParse(contentTypeName, ignoreCase: true, out EnumCoreContentType contentKind))
            {
                contentKind = EnumCoreContentType.Custom;
            }

            for (int i = 0; i < slot.Quantity; i++)
            {
                var item = ContentOrderItemEntity.Create(
                    id: Guid.NewGuid(),
                    orderId: order.Id,
                    contentKind: contentKind,
                    categoryId: categoryId,
                    promotionLevelId: null,
                    promoPriceSnapshotUsd: null,
                    socialBoost: false,
                    isBonus: !slot.IsRequired
                );

                itemsToCreate.Add((item, pricing));
            }
        }

        foreach ((ContentOrderItemEntity item, IReadOnlyList<CategoryPricingEntity> pricing) in itemsToCreate)
        {
            foreach (CategoryPricingEntity p in pricing)
            {
                var tier = ContentItemTierEntity.Create(
                    id: Guid.NewGuid(),
                    orderItemId: item.Id,
                    pricingTierId: p.PricingTierId,
                    priceSnapshotUsd: p.PriceUsd
                );

                item.Tiers.Add(tier);
            }

            order.Items.Add(item);
        }

        order.RecalculateTotalFromItems();

        return itemsToCreate.Count;
    }
}
