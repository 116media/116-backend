using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for lyrics data access operations.
/// </summary>
public interface ILyricsRepository : IRepository<LyricsEntity>
{
    /// <summary>
    /// Retrieves a paginated list of all lyrics.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="search">Optional search term to filter lyrics by song title, artist name, or lyrics text.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of lyrics and the total count.</returns>
    Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves a lyric's record by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the lyrics.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyric's entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyric's record by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the lyrics.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyric's entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the lyrics record is not found.</exception>
    Task<LyricsEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyric's record matching the given song title and artist name (case-insensitive).
    /// Returns null if not found.
    /// </summary>
    /// <param name="songTitle">The song title to look up.</param>
    /// <param name="artistName">The artist name to look up.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The lyric's entity if found, otherwise null.</returns>
    Task<LyricsEntity?> GetBySongTitleAndArtistAsync(
        string songTitle,
        string artistName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new lyrics record to the repository.
    /// </summary>
    Task AddAsync(LyricsEntity lyrics, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing lyrics record as modified.
    /// </summary>
    void Update(LyricsEntity lyrics);
}
