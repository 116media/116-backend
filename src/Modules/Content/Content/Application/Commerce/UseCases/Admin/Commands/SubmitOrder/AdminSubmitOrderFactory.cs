using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Factory implementation for the order submission flow.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminSubmitOrderFactory(IContentOrderRepository contentOrderRepository, IContentUnitOfWork unitOfWork)
    : ISubmitOrderFactory
{
    /// <inheritdoc />
    public async Task SubmitAsync(ContentOrderEntity order, CancellationToken cancellationToken)
    {
        if (!order.Items.Any(i => i.Tiers.Count > 0))
        {
            throw ContentOrderErrors.MustHaveAtLeastOneItemWithTier();
        }

        order.Submit();

        var payment = ContentPaymentEntity.Create(
            id: Guid.NewGuid(),
            orderId: order.Id,
            amountUsd: order.TotalAmountUsd
        );

        await contentOrderRepository.AddPaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
