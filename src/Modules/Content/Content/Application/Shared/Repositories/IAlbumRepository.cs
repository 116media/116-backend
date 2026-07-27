using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for album data access operations.
/// </summary>
public interface IAlbumRepository : IRepository<AlbumEntity>
{
    /// <summary>
    /// Retrieves an album by its unique identifier. Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the album.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The album entity if found, otherwise null.</returns>
    Task<AlbumEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an album by its unique identifier. Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the album.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The album entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the album is not found.</exception>
    Task<AlbumEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of albums with an optional search filter.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter albums by name or label.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of albums and the total count.</returns>
    Task<(List<AlbumEntity> Albums, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a paginated page of an artist's releases of a given type, newest first
    /// with a deterministic name tie-break and unknown years last.
    /// </summary>
    /// <param name="artistId">The artist profile the releases belong to.</param>
    /// <param name="releaseType">The release type to filter to.</param>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the page of albums and the total count.</returns>
    Task<(List<AlbumEntity> Albums, int TotalCount)> GetByArtistAsync(
        Guid artistId,
        EnumReleaseType releaseType,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new album to the repository.
    /// </summary>
    Task AddAsync(AlbumEntity album, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing album as modified.
    /// </summary>
    void Update(AlbumEntity album);
}
