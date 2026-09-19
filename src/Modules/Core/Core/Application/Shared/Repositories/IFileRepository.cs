using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Repositories;

/// <summary>
/// Repository interface for managing file entities and their metadata.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Gets a file by its unique identifier.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The file entity if found; otherwise, null.</returns>
    Task<FileEntity?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves multiple non-deleted files by id in a single query, keyed by file id.
    /// Ids that do not resolve (missing or deleted) are simply absent from the dictionary.
    /// </summary>
    /// <param name="fileIds">The file identifiers to fetch.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A read-only map of file id to <see cref="FileEntity" />.</returns>
    Task<IReadOnlyDictionary<Guid, FileEntity>> GetByIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new file entity to the repository.
    /// </summary>
    /// <param name="file">The file entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(FileEntity file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a file entity from the repository (hard delete).
    /// </summary>
    /// <param name="file">The file entity to remove.</param>
    void Remove(FileEntity file);

    /// <summary>
    /// Gets an avatar file by user's avatar file ID if it exists.
    /// </summary>
    /// <param name="avatarFileId">The avatar file ID from user entity.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The avatar file entity if found; otherwise, null.</returns>
    Task<FileEntity?> GetAvatarFileAsync(Guid? avatarFileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stages the soft deletion of a file. The caller's transaction commits it.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to soft-delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file was staged for deletion; false if not found or already deleted.</returns>
    Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
}
