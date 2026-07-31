using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when a signed-in user requests ownership of an artist profile. The
/// request itself persists as an <c>ArtistClaimRequestEntity</c> row for staff
/// to review; no consumer reacts in v1 — the event records the fact for the
/// future admin claim-review queue.
/// </summary>
/// <param name="ArtistId">The artist profile being claimed.</param>
/// <param name="UserId">The identity user UUID requesting the claim.</param>
public record ArtistClaimRequestedEvent(Guid ArtistId, Guid UserId) : IDomainEvent;
