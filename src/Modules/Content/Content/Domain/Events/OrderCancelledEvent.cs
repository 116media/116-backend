using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a <c>Draft</c> or <c>PendingPayment</c> order is cancelled.
/// </summary>
/// <param name="OrderId">The cancelled order.</param>
public record OrderCancelledEvent(Guid OrderId) : IDomainEvent;
