using _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.SubmitOrder;

/// <summary>
/// Handles the <see cref="AdminSubmitOrderCommand" /> to submit a Draft order for payment.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="submitOrderFactory">Factory for the order submission flow.</param>
public class AdminSubmitOrderHandler(
    IContentOrderRepository contentOrderRepository,
    ISubmitOrderFactory submitOrderFactory
) : ICommandHandler<AdminSubmitOrderCommand, AdminSubmitOrderResult>
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

        await submitOrderFactory.SubmitAsync(order: order, ct: cancellationToken);

        return new AdminSubmitOrderResult(IsSuccess: true);
    }
}
