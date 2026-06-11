using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.Factories;

/// <summary>
/// Factory implementation for fetching and validating order payment records.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
public class OrderPaymentFactory(IContentOrderRepository contentOrderRepository, ContentOrderErrors contentOrderErrors)
    : IOrderPaymentFactory
{
    /// <inheritdoc />
    public async Task<ContentPaymentEntity> GetByOrderIdOrThrowAsync(Guid orderId, CancellationToken ct = default)
    {
        ContentPaymentEntity? payment = await contentOrderRepository.GetPaymentByOrderIdAsync(orderId: orderId, ct: ct);

        if (payment is not null)
        {
            return payment;
        }

        throw contentOrderErrors.PaymentNotFound(orderId: orderId);
    }
}
