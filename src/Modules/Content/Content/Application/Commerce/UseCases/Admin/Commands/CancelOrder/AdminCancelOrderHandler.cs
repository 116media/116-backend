using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.CancelOrder;

/// <summary>
/// Handles the <see cref="AdminCancelOrderCommand" /> to cancel a Draft or PendingPayment order.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminCancelOrderHandler(IContentOrderRepository contentOrderRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminCancelOrderCommand, AdminCancelOrderResult>
{
    /// <inheritdoc />
    public async Task<AdminCancelOrderResult> Handle(
        AdminCancelOrderCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity order = await contentOrderRepository.GetByIdOrThrowAsync(id: orderId, ct: cancellationToken);

        order.Cancel();

        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminCancelOrderResult(IsSuccess: true);
    }
}
