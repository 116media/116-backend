using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for artist ownership claim request data access operations.
/// </summary>
public interface IArtistClaimRequestRepository : IRepository<ArtistClaimRequestEntity>
{
    /// <summary>
    /// Adds a new artist ownership claim request to the repository.
    /// </summary>
    /// <param name="claimRequest">The claim request entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(ArtistClaimRequestEntity claimRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the given user has already filed a claim request for the given artist.
    /// </summary>
    /// <param name="artistId">The artist profile being claimed.</param>
    /// <param name="userId">The identity user UUID of the requesting user.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns><c>true</c> when a request already exists for the pair; otherwise <c>false</c>.</returns>
    Task<bool> ExistsForArtistAndUserAsync(Guid artistId, Guid userId, CancellationToken cancellationToken = default);
}
