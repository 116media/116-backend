using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Tells the B2B customer their payment proof was rejected, quoting the
/// review notes, so a corrected payment can be sent. Re-resolves the order
/// with its customer navigation since the handler runs post-commit in a
/// fresh scope.
/// </summary>
/// <param name="contentOrderRepository">Repository resolving the order with its navigations.</param>
/// <param name="customerNotifier">Commerce customer email service.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class PaymentRejectedEmailHandler(
    IContentOrderRepository contentOrderRepository,
    ICommerceCustomerNotifier customerNotifier,
    ILogger<PaymentRejectedEmailHandler> logger
) : IDomainEventHandler<PaymentRejectedEvent>
{
    /// <inheritdoc />
    public async Task Handle(PaymentRejectedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: domainEvent.OrderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            logger.LogDebug("Payment rejected email skipped: order {OrderId} was not found.", domainEvent.OrderId);
            return;
        }

        await customerNotifier.NotifyPaymentRejectedAsync(
            order: order,
            notes: domainEvent.Notes,
            cancellationToken: cancellationToken
        );
    }
}
