using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Represents a B2B content order — the root aggregate of the Commerce sub-module.
/// An order links a customer to one or more commissioned content items (articles or videos)
/// with their pricing tiers. The order drives the full revenue lifecycle from draft to payment.
/// </summary>
public partial class ContentOrderEntity : Aggregate<Guid>
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
    public Money TotalAmountUsd { get; private set; } = null!;

    /// <summary>
    /// Current lifecycle status of the order.
    /// </summary>
    public EnumOrderStatus Status { get; private set; }

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
}
