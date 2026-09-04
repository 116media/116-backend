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
    /// Returns the storage URLs for the given file IDs, keyed by file ID.
    /// Missing or soft-deleted files are absent from the result.
    /// </summary>
    /// <param name="fileIds">The file UUIDs to resolve. Duplicates and unknown IDs are ignored.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A dictionary keyed by file ID containing each file's storage URL.</returns>
    Task<IReadOnlyDictionary<Guid, string>> GetStorageUrlsByIdsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Records that a referencing row now owns the file, so the reaper leaves it alone. Call it
    /// once the write that references the file has committed.
    /// </summary>
    /// <param name="fileId">The file that is now referenced.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the claim was recorded; false when the file is missing or already claimed.</returns>
    Task<bool> ClaimAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the uploads that were never claimed within the grace period, oldest first.
    /// </summary>
    /// <param name="olderThan">Only files created before this moment are returned.</param>
    /// <param name="batchSize">Maximum number of files to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The abandoned uploads.</returns>
    Task<IReadOnlyList<FileEntity>> GetUnclaimedBeforeAsync(
        DateTime olderThan,
        int batchSize,
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
    /// Soft-deletes a file entity by its ID.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to soft-delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file was successfully soft-deleted; false if not found or already deleted.</returns>
    Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
}
