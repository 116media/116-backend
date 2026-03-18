using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Handles the <see cref="AdminSubmitOrderCommand" /> to submit a Draft order for payment.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminSubmitOrderHandler(IContentOrderRepository contentOrderRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminSubmitOrderCommand, AdminSubmitOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminSubmitOrderResult> Handle(
        AdminSubmitOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: orderId);
        }

        bool hasItemWithTier = order.Items.Any(i => i.Tiers.Count > 0);

        if (!hasItemWithTier || order.Items.Count == 0)
        {
            throw ContentOrderErrors.MustHaveAtLeastOneItemWithTier();
        }

        order.Submit();

        var payment = ContentPaymentEntity.Create(
            id: Guid.NewGuid(),
            orderId: orderId,
            amountUsd: order.TotalAmountUsd
        );

        await contentOrderRepository.AddPaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSubmitOrderResult(IsSuccess: true);
    }
}
