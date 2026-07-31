using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an active paid promotion is force-removed from a content
/// record, whatever its type. One event shape covers articles, videos and
/// lyrics pages; consumers branch on <paramref name="ContentType" />.
/// </summary>
/// <param name="ContentId">The content record whose promotion was removed.</param>
/// <param name="ContentType">The kind of content record the promotion was removed from.</param>
/// <param name="CustomerId">The paying customer behind the placement, or <c>null</c> for free editorial content.</param>
/// <param name="Title">The content's display title at removal time.</param>
/// <param name="Reason">The admin-provided removal reason.</param>
public record ContentPromotionRemovedEvent(
    Guid ContentId,
    EnumCoreContentType ContentType,
    Guid? CustomerId,
    string Title,
    string Reason
) : IDomainEvent;
