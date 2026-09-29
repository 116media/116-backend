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
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(id: orderId, ct: ct);

        if (order?.Payment is not null)
        {
            return order.Payment;
        }

        throw contentOrderErrors.PaymentNotFound(orderId: orderId);
    }
}
