using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for artist profile data access operations.
/// </summary>
public interface IArtistRepository : IRepository<ArtistEntity>
{
    /// <summary>
    /// Retrieves an artist profile by its URL-safe slug. Returns null if not found.
    /// </summary>
    /// <param name="slug">The URL-safe slug of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity if found, otherwise null.</returns>
    Task<ArtistEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist profile by its unique identifier. Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity if found, otherwise null.</returns>
    Task<ArtistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an artist profile by its unique identifier. Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the artist profile.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The artist entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the artist profile is not found.</exception>
    Task<ArtistEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the artist profile claimed by a given identity user. Returns null if the
    /// user has not claimed any profile — needed by the verified-artist fast path.
    /// </summary>
    /// <param name="userId">The identity user UUID to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The claimed artist entity if one exists, otherwise null.</returns>
    Task<ArtistEntity?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of artist profiles with an optional search filter.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter artists by name or bio.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of artist profiles and the total count.</returns>
    Task<(List<ArtistEntity> Artists, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new artist profile to the repository.
    /// </summary>
    Task AddAsync(ArtistEntity artist, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing artist profile as modified.
    /// </summary>
    void Update(ArtistEntity artist);
}
