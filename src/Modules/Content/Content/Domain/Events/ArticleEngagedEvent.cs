using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised by the article interaction aggregates (like, bookmark, share,
/// comment) when an interaction row is created or removed. Consumed by the
/// article engagement handler, which applies the matching denormalized
/// counter on the article and invalidates the popular-articles cache.
/// </summary>
/// <param name="ArticleId">The article the interaction targets.</param>
/// <param name="Kind">The kind of engagement performed.</param>
/// <param name="Delta"><c>+1</c> for a created interaction row, <c>-1</c> for a removed one.</param>
public record ArticleEngagedEvent(Guid ArticleId, EnumEngagementKind Kind, int Delta) : IDomainEvent;
