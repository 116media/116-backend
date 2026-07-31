using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an article leaves the published set (rejection or archival of
/// a published article). Consumed by the popular-articles cache invalidation
/// so the departed article stops appearing in the ranked list.
/// </summary>
/// <param name="ArticleId">The article that left the published set.</param>
public record ArticleUnpublishedEvent(Guid ArticleId) : IDomainEvent;
