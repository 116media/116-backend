using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a payment is verified and its order marked paid. Consumers
/// send the payment receipt and stamp the paid-for effects (promotion,
/// social boost, pending review) onto the content fulfilling each item.
/// </summary>
/// <param name="OrderId">The paid order.</param>
/// <param name="PaymentId">The verified payment.</param>
/// <param name="PaidAt">
/// The instant the order was marked paid, truncated to whole milliseconds.
/// Consumers compare it against later promotion decisions (a force-unpromote,
/// for instance) to tell an unapplied effect from one that has since been
/// deliberately undone.
/// </param>
/// <param name="Items">The paid items with their effects computed at raise time.</param>
public record OrderPaidEvent(Guid OrderId, Guid PaymentId, DateTimeOffset PaidAt, IReadOnlyList<PaidItemEffect> Items)
    : IDomainEvent;
