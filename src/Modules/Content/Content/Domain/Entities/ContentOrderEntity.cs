using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a B2B content order — the root aggregate of the Commerce sub-module.
/// An order links a customer to one or more commissioned content items (articles or videos)
/// with their pricing tiers. The order drives the full revenue lifecycle from draft to payment.
/// </summary>
public class ContentOrderEntity : Aggregate<Guid>
{
    /// <summary>
    /// The B2B customer who placed this order.
    /// </summary>
    public Guid CustomerId { get; private set; }

    /// <summary>
    /// The optional package applied to this order.
    /// When set, the admin is applying a pre-configured bundle deal instead of building the order item-by-item.
    /// </summary>
    public Guid? PackageId { get; private set; }

    /// <summary>
    /// The running total of all tier price snapshots plus promotion level prices across all items.
    /// Recomputed each time a tier or item is added or removed via <see cref="RecalculateTotalFromItems" />.
    /// Starts at <c>0</c> and is never negative.
    /// </summary>
    public decimal TotalAmountUsd { get; private set; }

    /// <summary>
    /// Current lifecycle status of the order.
    /// </summary>
    public EnumOrderStatus Status { get; private set; }

    /// <summary>
    /// The customer who placed this order.
    /// </summary>
    public CustomerEntity Customer { get; private set; } = null!;

    /// <summary>
    /// The package applied to this order, or <c>null</c> if no package was selected.
    /// </summary>
    public PackageEntity? Package { get; private set; }

    /// <summary>
    /// The line items of this order (one per commissioned content piece).
    /// </summary>
    public ICollection<ContentOrderItemEntity> Items { get; } = new List<ContentOrderItemEntity>();

    /// <summary>
    /// The payment record created when the order is submitted.
    /// <c>null</c> while the order is still in <c>Draft</c> status.
    /// </summary>
    public ContentPaymentEntity? Payment { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ContentOrderEntity() { }

    /// <summary>
    /// Creates a new content order in <c>Draft</c> status with a zero total.
    /// </summary>
    /// <param name="id">The unique identifier for the order.</param>
    /// <param name="customerId">The B2B customer placing the order.</param>
    /// <param name="packageId">The optional pre-configured package to apply to this order.</param>
    /// <returns>A new <see cref="ContentOrderEntity" /> in <c>Draft</c> status.</returns>
    public static ContentOrderEntity Create(Guid id, Guid customerId, Guid? packageId)
    {
        return new ContentOrderEntity
        {
            Id = id,
            CustomerId = customerId,
            PackageId = packageId,
            TotalAmountUsd = 0,
            Status = EnumOrderStatus.Draft,
        };
    }

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

    /// <summary>
    /// Submits the order, transitioning it from <c>Draft</c> to <c>PendingPayment</c>.
    /// After submission, no new items or tiers may be added.
    /// </summary>
    /// <exception cref="ContentRuleException">
    /// Thrown when the order is already submitted or in a later status.
    /// </exception>
    public void Submit()
    {
        if (Status != EnumOrderStatus.Draft)
        {
            throw new ContentRuleException(ContentRuleCodes.OrderAlreadySubmitted);
        }

        Status = EnumOrderStatus.PendingPayment;
        AddDomainEvent(new OrderSubmittedEvent(OrderId: Id));
    }

    /// <summary>
    /// Marks the order as <c>Paid</c> after payment has been verified and raises
    /// <see cref="OrderPaidEvent" /> carrying one <see cref="PaidItemEffect" /> per item.
    /// The promotion window of every promoted item is computed here, at raise time,
    /// from the payment's verification instant, so consumers apply the original
    /// paid-for window and never recompute it.
    /// Both the paid instant and the windows derived from it are truncated to whole
    /// milliseconds: <c>timestamptz</c> stores microseconds, so a truncated value
    /// round-trips through the database unchanged and a reloaded content record can be
    /// compared to the payload for equality.
    /// Called by <c>VerifyPaymentHandler</c> alongside payment entity verification.
    /// </summary>
    /// <param name="paymentId">The verified payment that settled this order.</param>
    /// <param name="verifiedAt">The instant the payment was verified, from the payment record.</param>
    /// <param name="promotionDurationsByLevelId">
    /// Promotion duration in days per promotion level id, covering every level
    /// referenced by this order's items.
    /// </param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the order is not in <c>PendingPayment</c> status.
    /// </exception>
    /// <exception cref="ContentRuleException">
    /// Thrown when an item references a promotion level absent from
    /// <paramref name="promotionDurationsByLevelId" />.
    /// </exception>
    public void MarkPaid(
        Guid paymentId,
        DateTimeOffset verifiedAt,
        IReadOnlyDictionary<Guid, int> promotionDurationsByLevelId
    )
    {
        if (Status != EnumOrderStatus.PendingPayment)
        {
            throw new ContentRuleException(ContentRuleCodes.OrderAlreadyPaid);
        }

        Status = EnumOrderStatus.Paid;

        DateTimeOffset paidAt = TruncateToMilliseconds(verifiedAt);
        List<PaidItemEffect> paidItemEffects =
        [
            .. Items.Select(item => new PaidItemEffect(
                OrderItemId: item.Id,
                PromotionLevelId: item.PromotionLevelId,
                PromotionUntil: ResolvePromotionUntil(
                    paidAt: paidAt,
                    promotionLevelId: item.PromotionLevelId,
                    promotionDurationsByLevelId: promotionDurationsByLevelId
                ),
                SocialBoost: item.SocialBoost
            )),
        ];

        AddDomainEvent(new OrderPaidEvent(OrderId: Id, PaymentId: paymentId, PaidAt: paidAt, Items: paidItemEffects));
    }

    /// <summary>
    /// Computes the promotion expiry of a single item from its purchased level's
    /// duration, or <c>null</c> when the item carries no promotion.
    /// </summary>
    /// <param name="paidAt">The truncated instant the order was paid.</param>
    /// <param name="promotionLevelId">The item's purchased promotion level, if any.</param>
    /// <param name="promotionDurationsByLevelId">Promotion duration in days per promotion level id.</param>
    /// <returns>The promotion expiry, or <c>null</c> when the item carries no promotion.</returns>
    /// <exception cref="ContentRuleException">
    /// Thrown when the level's duration is missing from the supplied map.
    /// </exception>
    private static DateTimeOffset? ResolvePromotionUntil(
        DateTimeOffset paidAt,
        Guid? promotionLevelId,
        IReadOnlyDictionary<Guid, int> promotionDurationsByLevelId
    )
    {
        if (!promotionLevelId.HasValue)
        {
            return null;
        }

        if (!promotionDurationsByLevelId.TryGetValue(promotionLevelId.Value, out int durationDays))
        {
            throw new ContentRuleException(ContentRuleCodes.PromotionDurationUnavailable);
        }

        return paidAt.AddDays(durationDays);
    }

    /// <summary>
    /// Drops the sub-millisecond part of an instant so the value survives a
    /// <c>timestamptz</c> round-trip unchanged.
    /// </summary>
    /// <param name="instant">The instant to truncate.</param>
    /// <returns>The instant with its sub-millisecond ticks removed.</returns>
    private static DateTimeOffset TruncateToMilliseconds(DateTimeOffset instant)
    {
        return instant.AddTicks(-(instant.Ticks % TimeSpan.TicksPerMillisecond));
    }

    /// <summary>
    /// Cancels the order. Allowed from <c>Draft</c> or <c>PendingPayment</c> status only.
    /// Paid orders cannot be cancelled because the content creation workflow has already started.
    /// </summary>
    /// <exception cref="ContentRuleException">
    /// Thrown when the order is already <c>Paid</c>.
    /// </exception>
    /// <exception cref="ContentRuleException">
    /// Thrown when the order is already <c>Cancelled</c>.
    /// </exception>
    public void Cancel()
    {
        Status = Status switch
        {
            EnumOrderStatus.Paid => throw new ContentRuleException(ContentRuleCodes.CannotCancelPaidOrder),
            EnumOrderStatus.Cancelled => throw new ContentRuleException(ContentRuleCodes.OrderAlreadyCancelled),
            _ => EnumOrderStatus.Cancelled,
        };

        AddDomainEvent(new OrderCancelledEvent(OrderId: Id));
    }
}
