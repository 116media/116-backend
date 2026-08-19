using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an article enters the published set. Consumed by the
/// popular-articles cache invalidation so the ranked list reflects the new
/// membership on the next read.
/// </summary>
/// <param name="ArticleId">The article that was published.</param>
public record ArticlePublishedEvent(Guid ArticleId) : IDomainEvent;
