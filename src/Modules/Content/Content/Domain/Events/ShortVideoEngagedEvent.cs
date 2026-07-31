using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised by the short-video interaction aggregates (like, bookmark, share,
/// counted view) when an interaction row is created or removed. Consumed by
/// the short-video engagement handler, which applies the matching
/// denormalized counter on the short video. No shorts cache exists, so the
/// event has no cache consumer today; a future cache is one handler away.
/// </summary>
/// <param name="ShortVideoId">The short video the interaction targets.</param>
/// <param name="Kind">The kind of engagement performed.</param>
/// <param name="Delta"><c>+1</c> for a created interaction row, <c>-1</c> for a removed one.</param>
public record ShortVideoEngagedEvent(Guid ShortVideoId, EnumEngagementKind Kind, int Delta) : IDomainEvent;
