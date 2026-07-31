using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a body update drops images from an article's rich-text HTML.
/// The storage keys are captured at raise time from the pre-update image set
/// so the post-commit cleanup consumer can remove the orphaned
/// <c>article_images</c> rows and the remote assets in its own scope, with
/// the same retry story for both.
/// </summary>
/// <param name="ArticleId">The article whose body update orphaned the images.</param>
/// <param name="StorageKeys">The storage keys of the body images that dropped out of the new body.</param>
public record ArticleBodyImagesOrphanedEvent(Guid ArticleId, IReadOnlyList<string> StorageKeys) : IDomainEvent;
