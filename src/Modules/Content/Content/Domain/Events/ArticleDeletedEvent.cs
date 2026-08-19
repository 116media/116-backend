using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an article record is removed. The storage keys are captured
/// before removal so post-commit consumers can clean the remote assets
/// without re-querying rows that no longer exist. Consumed today by the
/// popular-articles cache invalidation; the remote-asset cleanup consumer
/// attaches to the same event.
/// </summary>
/// <param name="ArticleId">The article that was removed.</param>
/// <param name="CoverFileId">The cover image file record, or <c>null</c> when the article had no cover.</param>
/// <param name="BodyImageStorageKeys">The storage keys of the body images captured before removal.</param>
public record ArticleDeletedEvent(Guid ArticleId, Guid? CoverFileId, IReadOnlyList<string> BodyImageStorageKeys)
    : IDomainEvent;
