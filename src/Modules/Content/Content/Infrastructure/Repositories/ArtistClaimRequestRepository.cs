using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArtistClaimRequestRepository" /> for managing artist ownership
/// claim request entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArtistClaimRequestRepository(ContentDbContext context) : IArtistClaimRequestRepository
{
    /// <inheritdoc />
    public async Task AddAsync(ArtistClaimRequestEntity claimRequest, CancellationToken cancellationToken = default)
    {
        await context.ArtistClaimRequests.AddAsync(claimRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsForArtistAndUserAsync(
        Guid artistId,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await context.ArtistClaimRequests.AnyAsync(
            request => request.ArtistId == artistId && request.UserId == userId,
            cancellationToken
        );
    }
}
