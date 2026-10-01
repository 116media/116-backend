using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Payment behaviour of <see cref="ContentOrderEntity" />. Its state lives in <c>Entities/ContentOrderEntity.cs</c>.
/// </summary>
public sealed partial class ContentOrderEntity
{
    /// <summary>
    /// Creates the payment slot for this order's frozen total. Called by the submission flow
    /// right after <see cref="Submit" />.
    /// </summary>
    /// <returns>The payment that was attached.</returns>
    public ContentPaymentEntity AttachPayment()
    {
        ContentPaymentEntity payment = ContentPaymentEntity.Create(
            id: Guid.NewGuid(),
            orderId: Id,
            amountUsd: TotalAmountUsd
        );

        Payment = payment;

        return payment;
    }

    /// <summary>
    /// Rejects this order's payment with the moderator's notes, raising the rejection fact the
    /// resubmission email flow consumes.
    /// </summary>
    /// <param name="notes">The reason the proof was rejected, or null.</param>
    public void RejectPayment(string? notes)
    {
        Payment!.Reject(notes: notes);

        AddDomainEvent(new PaymentRejectedEvent(OrderId: Id, PaymentId: Payment.Id, Notes: notes));
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
