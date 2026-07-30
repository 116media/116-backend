using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Tells the B2B customer their order was cancelled. Re-resolves the order
/// since the handler runs post-commit in a fresh scope; the notifier then
/// resolves the customer by id.
/// </summary>
/// <param name="contentOrderRepository">Repository resolving the cancelled order.</param>
/// <param name="customerNotifier">Commerce customer email service.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class OrderCancelledEmailHandler(
    IContentOrderRepository contentOrderRepository,
    ICommerceCustomerNotifier customerNotifier,
    ILogger<OrderCancelledEmailHandler> logger
) : IDomainEventHandler<OrderCancelledEvent>
{
    /// <inheritdoc />
    public async Task Handle(OrderCancelledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: domainEvent.OrderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            logger.LogDebug("Order cancelled email skipped: order {OrderId} was not found.", domainEvent.OrderId);
            return;
        }

        await customerNotifier.NotifyOrderCancelledAsync(order: order, cancellationToken: cancellationToken);
    }
}
