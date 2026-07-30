using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Sends the payment receipt to the B2B customer when an order is marked
/// paid. Re-resolves the order and its payment record since the handler
/// runs post-commit in a fresh scope.
/// </summary>
/// <param name="contentOrderRepository">Repository resolving the order with its navigations.</param>
/// <param name="customerNotifier">Commerce customer email service.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class OrderPaidReceiptEmailHandler(
    IContentOrderRepository contentOrderRepository,
    ICommerceCustomerNotifier customerNotifier,
    ILogger<OrderPaidReceiptEmailHandler> logger
) : IDomainEventHandler<OrderPaidEvent>
{
    /// <inheritdoc />
    public async Task Handle(OrderPaidEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: domainEvent.OrderId,
            ct: cancellationToken
        );

        ContentPaymentEntity? payment = await contentOrderRepository.GetPaymentByOrderIdAsync(
            orderId: domainEvent.OrderId,
            ct: cancellationToken
        );

        if (order is null || payment is null)
        {
            logger.LogDebug(
                "Payment receipt email skipped: order {OrderId} has no resolvable order or payment record.",
                domainEvent.OrderId
            );
            return;
        }

        await customerNotifier.NotifyPaymentReceiptAsync(
            order: order,
            payment: payment,
            cancellationToken: cancellationToken
        );
    }
}
