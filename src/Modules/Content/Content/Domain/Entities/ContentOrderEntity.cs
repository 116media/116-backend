using _116.Content.Application.Shared.Errors;
using _116.Content.Domain.Enums;
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
    /// Recomputed each time a tier is added via <see cref="RecalculateTotal" />.
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
    /// <exception cref="_116.Shared.Application.Exceptions.BadRequestException">
    /// Thrown when the order is not in <c>Draft</c> status.
    /// </exception>
    public void EnsureDraft()
    {
        if (Status != EnumOrderStatus.Draft)
        {
            throw ContentOrderErrors.CannotAddItemToNonDraftOrder();
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
    /// Recalculates the order total from scratch using all existing items and their tiers.
    /// Bonus items (<see cref="ContentOrderItemEntity.IsBonus" /> = true) are excluded
    /// from the total — they are complimentary and do not contribute to the price.
    /// Called after removing an item or tier.
    /// </summary>
    public void RecalculateTotalFromItems()
    {
        IEnumerable<ContentOrderItemEntity> billableItems = Items.Where(i => !i.IsBonus);
        decimal tierTotal = billableItems.SelectMany(i => i.Tiers).Sum(t => t.PriceSnapshotUsd);
        decimal promoTotal = billableItems.Sum(i => i.PromoPriceSnapshotUsd ?? 0m);
        TotalAmountUsd = tierTotal + promoTotal;
    }

    /// <summary>
    /// Recomputes the order total after a new pricing tier is added.
    /// Bonus items are excluded from the total. If the item receiving the new tier
    /// is a bonus item, <paramref name="newTierPrice" /> is not added.
    /// </summary>
    /// <param name="newTierPrice">The price snapshot of the tier being added in this operation.</param>
    /// <param name="isBonusItem">Whether the item receiving the tier is a bonus item.</param>
    public void RecalculateTotal(decimal newTierPrice, bool isBonusItem = false)
    {
        IEnumerable<ContentOrderItemEntity> billableItems = Items.Where(i => !i.IsBonus);
        decimal tierTotal = billableItems.SelectMany(i => i.Tiers).Sum(t => t.PriceSnapshotUsd);
        decimal promoTotal = billableItems.Sum(i => i.PromoPriceSnapshotUsd ?? 0m);
        TotalAmountUsd = tierTotal + promoTotal + (isBonusItem ? 0m : newTierPrice);
    }

    /// <summary>
    /// Submits the order, transitioning it from <c>Draft</c> to <c>PendingPayment</c>.
    /// After submission, no new items or tiers may be added.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the order is already submitted or in a later status.
    /// </exception>
    public void Submit()
    {
        if (Status != EnumOrderStatus.Draft)
        {
            throw ContentOrderErrors.AlreadySubmitted();
        }

        Status = EnumOrderStatus.PendingPayment;
    }

    /// <summary>
    /// Marks the order as <c>Paid</c> after payment has been verified.
    /// Called by <c>VerifyPaymentHandler</c> alongside payment entity verification.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the order is not in <c>PendingPayment</c> status.
    /// </exception>
    public void MarkPaid()
    {
        if (Status != EnumOrderStatus.PendingPayment)
        {
            throw ContentOrderErrors.AlreadyPaid();
        }

        Status = EnumOrderStatus.Paid;
    }

    /// <summary>
    /// Cancels the order. Allowed from <c>Draft</c> or <c>PendingPayment</c> status only.
    /// Paid orders cannot be cancelled because the content creation workflow has already started.
    /// </summary>
    /// <exception cref="_116.Shared.Application.Exceptions.BadRequestException">
    /// Thrown when the order is already <c>Paid</c>.
    /// </exception>
    /// <exception cref="_116.Shared.Application.Exceptions.ConflictException">
    /// Thrown when the order is already <c>Cancelled</c>.
    /// </exception>
    public void Cancel()
    {
        Status = Status switch
        {
            EnumOrderStatus.Paid => throw ContentOrderErrors.CannotCancelPaidOrder(),
            EnumOrderStatus.Cancelled => throw ContentOrderErrors.AlreadyCancelled(),
            _ => EnumOrderStatus.Cancelled,
        };
    }
}
