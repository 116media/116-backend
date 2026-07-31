using _116.Shared.Domain;

namespace _116.Content.Domain.Events;

/// <summary>
/// Raised when an artist profile's ownership claim is verified and the profile
/// linked to the claimant's account. Consumed post-commit to congratulate the
/// new owner over email and the in-app feed.
/// </summary>
/// <param name="ArtistId">The verified artist profile.</param>
/// <param name="UserId">The identity user UUID that now owns the profile.</param>
public record ArtistOwnershipVerifiedEvent(Guid ArtistId, Guid UserId) : IDomainEvent;
