using _116.Core.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Core.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing file entities and their metadata.
/// </summary>
public interface IFileRepository : IRepository<FileEntity>
{
    /// <summary>
    /// Gets a file by its unique identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The file entity if found; otherwise, null.</returns>
    Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new file entity to the repository.
    /// </summary>
    /// <param name="file">The file entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(FileEntity file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing file entity in the repository.
    /// </summary>
    /// <param name="file">The file entity to update.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
