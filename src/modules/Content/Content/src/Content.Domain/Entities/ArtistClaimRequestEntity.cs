using _116.Content.Domain.Events;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// A durable record of a signed-in user requesting ownership of an artist
/// profile. Carries no workflow state — approval remains the separate, manual
/// admin verify-owner action that calls <see cref="ArtistEntity.ClaimOwnership" />;
/// this row exists so the request itself is reviewable instead of a log line.
/// </summary>
public class ArtistClaimRequestEntity : Aggregate<Guid>
{
    /// <summary>
    /// The artist profile being claimed.
    /// </summary>
    public Guid ArtistId { get; private set; }

    /// <summary>
    /// The identity user UUID of the requesting user.
    /// No FK to the identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Private parameterless constructor required by Entity Framework Core.
    /// </summary>
    private ArtistClaimRequestEntity() { }

    /// <summary>
    /// Records a new ownership claim request for an artist profile.
    /// </summary>
    /// <param name="id">The unique identifier for this claim request.</param>
    /// <param name="artistId">The artist profile being claimed.</param>
    /// <param name="userId">The identity user UUID requesting the claim.</param>
    /// <returns>A new <see cref="ArtistClaimRequestEntity" />.</returns>
    public static ArtistClaimRequestEntity Create(Guid id, Guid artistId, Guid userId)
    {
        var request = new ArtistClaimRequestEntity
        {
            Id = id,
            ArtistId = artistId,
            UserId = userId,
        };

        request.AddDomainEvent(new ArtistClaimRequestedEvent(ArtistId: artistId, UserId: userId));

        return request;
    }
}
