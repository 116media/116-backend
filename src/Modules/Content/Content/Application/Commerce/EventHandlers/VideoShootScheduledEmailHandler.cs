using _116.Content.Application.Commerce.Services;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Tells the paying customer the shoot date for their pre-booked video
/// production. The notifier no-ops when the video has no customer, so free
/// editorial videos raise the event without producing an email.
/// </summary>
/// <param name="customerNotifier">Commerce customer email service.</param>
public class VideoShootScheduledEmailHandler(ICommerceCustomerNotifier customerNotifier)
    : IDomainEventHandler<VideoShootScheduledEvent>
{
    /// <inheritdoc />
    public async Task Handle(VideoShootScheduledEvent domainEvent, CancellationToken cancellationToken = default)
    {
        await customerNotifier.NotifyShootScheduledAsync(
            customerId: domainEvent.CustomerId,
            contentTitle: domainEvent.Title,
            shootDate: domainEvent.ShootDate.UtcDateTime,
            cancellationToken: cancellationToken
        );
    }
}
