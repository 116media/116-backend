using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;

/// <summary>
/// Factory for submitting a draft order: validates items, transitions status,
/// creates the payment record, and persists both atomically.
/// </summary>
public interface ISubmitOrderFactory
{
    /// <summary>
    /// Validates the order has at least one item with a tier, calls order.Submit(),
    /// creates a ContentPaymentEntity, and commits the transaction.
    /// </summary>
    Task SubmitAsync(ContentOrderEntity order, CancellationToken ct);
}
