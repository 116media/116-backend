using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Tells the paying customer their commissioned content is live, linking the
/// public page built from the slug on the payload. The notifier no-ops when
/// the content has no customer, so free editorial content raises the event
/// without producing an email.
/// </summary>
/// <param name="customerNotifier">Commerce customer email service.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class CommissionedContentPublishedEmailHandler(
    ICommerceCustomerNotifier customerNotifier,
    ILogger<CommissionedContentPublishedEmailHandler> logger
) : IDomainEventHandler<CommissionedContentPublishedEvent>
{
    /// <inheritdoc />
    public async Task Handle(
        CommissionedContentPublishedEvent domainEvent,
        CancellationToken cancellationToken = default
    )
    {
        string? publicUrl = domainEvent.ContentType switch
        {
            EnumCoreContentType.Article => ContentPublicLinks.Article(domainEvent.Slug),
            EnumCoreContentType.Video => ContentPublicLinks.Video(domainEvent.Slug),
            EnumCoreContentType.Lyrics => ContentPublicLinks.Lyrics(domainEvent.Slug),
            _ => null,
        };

        if (publicUrl is null)
        {
            logger.LogDebug(
                "Content published email skipped: content {ContentId} has unsupported type {ContentType}.",
                domainEvent.ContentId,
                domainEvent.ContentType
            );
            return;
        }

        await customerNotifier.NotifyContentPublishedAsync(
            customerId: domainEvent.CustomerId,
            contentTitle: domainEvent.Title,
            publicUrl: publicUrl,
            cancellationToken: cancellationToken
        );
    }
}
