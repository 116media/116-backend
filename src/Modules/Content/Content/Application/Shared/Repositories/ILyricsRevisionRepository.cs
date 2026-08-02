using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for lyrics-text community correction revision data access operations.
/// </summary>
public interface ILyricsRevisionRepository : IRepository<LyricsRevisionEntity>
{
    /// <summary>
    /// Retrieves a lyrics revision by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the revision.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The revision entity if found, otherwise null.</returns>
    Task<LyricsRevisionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyrics revision by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the revision.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The revision entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the revision is not found.</exception>
    Task<LyricsRevisionEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new lyrics revision to the repository.
    /// </summary>
    /// <param name="revision">The revision entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(LyricsRevisionEntity revision, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing lyrics revision as modified.
    /// </summary>
    /// <param name="revision">The revision entity to update.</param>
    void Update(LyricsRevisionEntity revision);
}
