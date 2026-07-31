using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised by the video interaction aggregates (share, rating) when an
/// interaction row is created or changed. Consumed by the video engagement
/// handler, which applies the matching denormalized counter (or recomputes
/// the rating aggregates from the rating rows) and invalidates the
/// popular-videos cache.
/// </summary>
/// <param name="VideoId">The video the interaction targets.</param>
/// <param name="Kind">The kind of engagement performed.</param>
/// <param name="Delta">
/// <c>+1</c> for a created interaction row, <c>-1</c> for a removed one and <c>0</c> for an
/// in-place change (a restarred rating). Ignored for ratings, whose aggregates are recomputed
/// from the rows.
/// </param>
public record VideoEngagedEvent(Guid VideoId, EnumEngagementKind Kind, int Delta) : IDomainEvent;
