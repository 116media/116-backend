using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a content record transitions to <c>Published</c>. Raised for
/// free editorial content too — consumers that only care about commissioned
/// work no-op when <paramref name="CustomerId" /> is <c>null</c>.
/// </summary>
/// <param name="ContentId">The published content record.</param>
/// <param name="ContentType">The kind of content record that was published.</param>
/// <param name="CustomerId">The paying customer behind the content, or <c>null</c> for free editorial content.</param>
/// <param name="Title">The content's display title at publication time.</param>
/// <param name="Slug">The content's public slug at publication time.</param>
public record CommissionedContentPublishedEvent(
    Guid ContentId,
    EnumCoreContentType ContentType,
    Guid? CustomerId,
    string Title,
    string Slug
) : IDomainEvent;
