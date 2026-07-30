using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Tells the paying customer their commissioned content failed editorial
/// review and why. The notifier no-ops when the content has no customer, so
/// free editorial content raises the event without producing an email.
/// </summary>
/// <param name="customerNotifier">Commerce customer email service.</param>
public class CommissionedContentRejectedEmailHandler(ICommerceCustomerNotifier customerNotifier)
    : IDomainEventHandler<CommissionedContentRejectedEvent>
{
    /// <inheritdoc />
    public async Task Handle(
        CommissionedContentRejectedEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        await customerNotifier.NotifyContentRejectedAsync(
            customerId: domainEvent.CustomerId,
            contentTitle: domainEvent.Title,
            reason: domainEvent.Reason,
            cancellationToken: cancellationToken
        );
    }
}
