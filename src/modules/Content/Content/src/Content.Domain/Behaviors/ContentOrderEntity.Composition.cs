using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Composition behaviour of <see cref="ContentOrderEntity" />. Its state lives in <c>Entities/ContentOrderEntity.cs</c>.
/// </summary>
public sealed partial class ContentOrderEntity
{
    /// <summary>
    /// Guards that the order is in <c>Draft</c> status.
    /// Called before any mutation that is only permitted while the order is still being built.
    /// </summary>
    /// <exception cref="ContentRuleException">
    /// Thrown when the order is not in <c>Draft</c> status.
    /// </exception>
    public void EnsureDraft()
    {
        if (Status != EnumOrderStatus.Draft)
        {
            throw new ContentRuleException(ContentRuleCodes.CannotAddItemToNonDraftOrder);
        }
    }

    /// <summary>
    /// Updates the customer and/or package on a draft order.
    /// </summary>
    /// <param name="customerId">The new customer ID, or null to keep the current one.</param>
    /// <param name="packageId">The new package ID, or null to clear it.</param>
    public void Update(Guid? customerId, Guid? packageId)
    {
        EnsureDraft();

        if (customerId.HasValue)
        {
            CustomerId = customerId.Value;
        }

        PackageId = packageId;
    }

    /// <summary>
    /// Adds an item to the order and recalculates the total, so no call site can
    /// add an item while leaving <see cref="TotalAmountUsd" /> stale.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(ContentOrderItemEntity item)
    {
        Items.Add(item);
        RecalculateTotalFromItems();
    }

    /// <summary>
    /// Builds an item inside this order and adds it, recalculating the total.
    /// </summary>
    /// <param name="contentKind">The content kind the item pays for.</param>
    /// <param name="categoryId">The category the item is placed in.</param>
    /// <param name="promotionLevelId">The purchased promotion level, or null.</param>
    /// <param name="promoPriceSnapshotUsd">The promotion price frozen at purchase, or null.</param>
    /// <param name="socialBoost">Whether the social boost add-on was purchased.</param>
    /// <param name="isBonus">Whether the item is a free bonus excluded from the total.</param>
    /// <returns>The item that was added.</returns>
    public ContentOrderItemEntity AddItem(
        EnumCoreContentType contentKind,
        Guid categoryId,
        Guid? promotionLevelId,
        decimal? promoPriceSnapshotUsd,
        bool socialBoost,
        bool isBonus
    )
    {
        ContentOrderItemEntity item = ContentOrderItemEntity.Create(
            id: Guid.NewGuid(),
            orderId: Id,
            contentKind: contentKind,
            categoryId: categoryId,
            promotionLevelId: promotionLevelId,
            promoPriceSnapshotUsd: promoPriceSnapshotUsd,
            socialBoost: socialBoost,
            isBonus: isBonus
        );

        AddItem(item: item);

        return item;
    }

    /// <summary>
    /// Attaches a priced tier to one of this order's items and recalculates the total.
    /// </summary>
    /// <param name="item">The item the tier is attached to.</param>
    /// <param name="pricingTierId">The tier being purchased.</param>
    /// <param name="priceSnapshotUsd">The tier price frozen at purchase.</param>
    /// <returns>The tier that was attached.</returns>
    public ContentItemTierEntity AddTier(ContentOrderItemEntity item, Guid pricingTierId, decimal priceSnapshotUsd)
    {
        ContentItemTierEntity tier = ContentItemTierEntity.Create(
            id: Guid.NewGuid(),
            orderItemId: item.Id,
            pricingTierId: pricingTierId,
            priceSnapshotUsd: priceSnapshotUsd
        );

        item.Tiers.Add(tier);
        RecalculateTotalFromItems();

        return tier;
    }

    /// <summary>
    /// Detaches a tier from one of this order's items and recalculates the total, reporting
    /// whether the tier was there.
    /// </summary>
    /// <param name="item">The item the tier is detached from.</param>
    /// <param name="tierId">The tier row to remove.</param>
    /// <returns><c>true</c> if a tier was removed; otherwise <c>false</c>.</returns>
    public bool RemoveTier(ContentOrderItemEntity item, Guid tierId)
    {
        ContentItemTierEntity? tier = item.Tiers.FirstOrDefault(candidate => candidate.Id == tierId);

        if (tier is null)
        {
            return false;
        }

        item.Tiers.Remove(tier);
        RecalculateTotalFromItems();

        return true;
    }

    /// <summary>
    /// Returns this order's item by its identifier, or null when it belongs to another order.
    /// </summary>
    /// <param name="itemId">The item to look up.</param>
    /// <returns>The matching item, or <c>null</c>.</returns>
    public ContentOrderItemEntity? FindItem(Guid itemId)
    {
        return Items.FirstOrDefault(item => item.Id == itemId);
    }

    /// <summary>
    /// Adds several items and recalculates the total once. Adding in a loop with
    /// <see cref="AddItem" /> would rescan every item and tier per addition.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddItems(IEnumerable<ContentOrderItemEntity> items)
    {
        foreach (ContentOrderItemEntity item in items)
        {
            Items.Add(item);
        }

        RecalculateTotalFromItems();
    }

    /// <summary>
    /// Removes an item from the order and recalculates the total.
    /// </summary>
    /// <param name="item">The item to remove.</param>
    public void RemoveItem(ContentOrderItemEntity item)
    {
        Items.Remove(item);
        RecalculateTotalFromItems();
    }

    /// <summary>
    /// Recalculates the order total from scratch using all existing items and their tiers.
    /// Bonus items (<see cref="ContentOrderItemEntity.IsBonus" /> = true) are excluded from the
    /// total. Item mutations go through <see cref="AddItem" />/<see cref="RemoveItem" />; this
    /// stays callable for tier-level mutations only.
    /// </summary>
    public void RecalculateTotalFromItems()
    {
        IEnumerable<ContentOrderItemEntity> billableItems = Items.Where(i => !i.IsBonus);
        decimal tierTotal = billableItems.SelectMany(i => i.Tiers).Sum(t => t.PriceSnapshotUsd);
        decimal promoTotal = billableItems.Sum(i => i.PromoPriceSnapshotUsd ?? 0m);
        TotalAmountUsd = tierTotal + promoTotal;
    }
}
