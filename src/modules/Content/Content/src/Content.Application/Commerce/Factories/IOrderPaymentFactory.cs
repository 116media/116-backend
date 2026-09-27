using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Shared factory for fetching and validating order payment records.
/// Centralizes the common "fetch payment or throw" logic used across
/// AttachPaymentProof, VerifyPayment, RejectPayment, and GetOrderPayment.
/// </summary>
public interface IOrderPaymentFactory
{
    /// <summary>
    /// Retrieves the payment record for an order, throwing if not found.
    /// </summary>
    Task<ContentPaymentEntity> GetByOrderIdOrThrowAsync(Guid orderId, CancellationToken ct = default);
}
