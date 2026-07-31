using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an order's payment proof is rejected during review. The order
/// stays in <c>PendingPayment</c> so a corrected proof can be resubmitted.
/// </summary>
/// <param name="OrderId">The order whose payment was rejected.</param>
/// <param name="PaymentId">The rejected payment.</param>
/// <param name="Notes">The reviewer's notes explaining the rejection, or <c>null</c> when none were provided.</param>
public record PaymentRejectedEvent(Guid OrderId, Guid PaymentId, string? Notes) : IDomainEvent;
