using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Sends the invoice-style payment request to the B2B customer when an order
/// is submitted. Re-resolves the order with its customer and item navigations
/// since the handler runs post-commit in a fresh scope.
/// </summary>
/// <param name="contentOrderRepository">Repository resolving the order with its navigations.</param>
/// <param name="customerNotifier">Commerce customer email service.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class OrderSubmittedInvoiceEmailHandler(
    IContentOrderRepository contentOrderRepository,
    ICommerceCustomerNotifier customerNotifier,
    ILogger<OrderSubmittedInvoiceEmailHandler> logger
) : IDomainEventHandler<OrderSubmittedEvent>
{
    /// <inheritdoc />
    public async Task Handle(OrderSubmittedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: domainEvent.OrderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            logger.LogDebug("Order invoice email skipped: order {OrderId} was not found.", domainEvent.OrderId);
            return;
        }

        await customerNotifier.NotifyOrderInvoiceAsync(order: order, cancellationToken: cancellationToken);
    }
}
