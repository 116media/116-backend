using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a content record fails editorial review and transitions to
/// <c>Rejected</c>. Raised for free editorial content too — consumers that
/// only care about commissioned work no-op when
/// <paramref name="CustomerId" /> is <c>null</c>.
/// </summary>
/// <param name="ContentId">The rejected content record.</param>
/// <param name="ContentType">The kind of content record that was rejected.</param>
/// <param name="CustomerId">The paying customer behind the content, or <c>null</c> for free editorial content.</param>
/// <param name="Title">The content's display title at rejection time.</param>
/// <param name="Reason">The captured rejection reason.</param>
public record CommissionedContentRejectedEvent(
    Guid ContentId,
    EnumCoreContentType ContentType,
    Guid? CustomerId,
    string Title,
    string Reason
) : IDomainEvent;
