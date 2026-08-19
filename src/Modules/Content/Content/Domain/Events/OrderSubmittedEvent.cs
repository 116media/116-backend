using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a draft order is submitted and enters <c>PendingPayment</c>.
/// Consumers send the invoice-style payment request to the B2B customer.
/// </summary>
/// <param name="OrderId">The submitted order.</param>
public record OrderSubmittedEvent(Guid OrderId) : IDomainEvent;
