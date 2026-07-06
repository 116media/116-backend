using _116.Core.Domain.Entities;
using _116.Shared.Domain;
using Microsoft.AspNetCore.Http;

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
    /// <remarks>
    /// This method marks the file as modified in the context. Call SaveChangesAsync() to persist changes.
    /// </remarks>
    Task UpdateAsync(FileEntity file, CancellationToken cancellationToken = default);

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
    /// Persists all pending changes to the database.
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads avatar file to cloud storage and persists file metadata to database.
    /// Calls FileService internally to upload to Cloudinary.
    /// </summary>
    /// <param name="avatarFile">Avatar file to upload.</param>
    /// <param name="userId">User ID to use as publicId.</param>
    /// <param name="originalFileName">Original filename.</param>
    /// <param name="mimeType">MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created file entity with persisted metadata.</returns>
    Task<FileEntity> UploadAndStoreAvatarAsync(
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Downloads avatar from URL and persists file metadata to database.
    /// Calls FileService internally to download from social provider.
    /// </summary>
    /// <param name="avatarUrl">Avatar URL from social provider.</param>
    /// <param name="userId">User ID for file naming.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created file entity with persisted metadata.</returns>
    Task<FileEntity> DownloadAndStoreAvatarFromUrlAsync(
        string avatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates user avatar from URL if the URL has changed.
    /// Checks if the user already has an avatar with the same URL to avoid redundant downloads.
    /// If the URL is different, deletes the old avatar file entity and downloads the new one.
    /// </summary>
    /// <param name="currentAvatarFileId">The current avatar file ID from the user entity (may be null).</param>
    /// <param name="newAvatarUrl">The new avatar URL from social provider.</param>
    /// <param name="userId">User ID for file naming.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new file entity if avatar was updated; otherwise, null if no update was needed.</returns>
    /// <remarks>
    /// Returns null if the user already has an avatar with the same URL.
    /// Otherwise, returns the newly created file entity after downloading from the URL.
    /// </remarks>
    Task<FileEntity?> UpdateAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string newAvatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates user avatar from the uploaded file.
    /// Deletes the old avatar file entity if it exists, then uploads and stores the new avatar.
    /// </summary>
    /// <param name="currentAvatarFileId">The current avatar file ID from the user entity (may be null).</param>
    /// <param name="avatarFile">The new avatar file to upload.</param>
    /// <param name="userId">User ID to use as publicId.</param>
    /// <param name="originalFileName">Original filename.</param>
    /// <param name="mimeType">MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created file entity after upload.</returns>
    /// <remarks>
    /// This method always uploads a new avatar and deletes the old one if it exists.
    /// </remarks>
    Task<FileEntity> UpdateAvatarFromFileAsync(
        Guid? currentAvatarFileId,
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates avatar from provider URL if allowed.
    /// Only updates if avatarUrl is provided and avatar is not manually uploaded.
    /// </summary>
    /// <param name="currentAvatarFileId">The current avatar file ID.</param>
    /// <param name="avatarUrl">Avatar URL from social provider.</param>
    /// <param name="userId">User ID for file naming.</param>
    /// <param name="isAvatarSourceManual">True if avatar was manually uploaded; false if from provider.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new file entity if updated; otherwise, null.</returns>
    Task<FileEntity?> UpdateAvatarUrlFromSourceAsync(
        Guid? currentAvatarFileId,
        string? avatarUrl,
        string userId,
        bool isAvatarSourceManual,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads an image file to cloud storage and persists file metadata to the database.
    /// </summary>
    /// <param name="file">The image file to upload.</param>
    /// <param name="publicId">The public ID used in cloud storage.</param>
    /// <param name="folder">The destination folder in cloud storage.</param>
    /// <param name="originalFileName">The original filename as submitted by the client.</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FileEntity" /> with persisted metadata.</returns>
    Task<FileEntity> UploadAndStoreImageFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a video file to cloud storage and persists file metadata to the database.
    /// </summary>
    /// <param name="file">The video file to upload.</param>
    /// <param name="publicId">The public ID used in cloud storage.</param>
    /// <param name="folder">The destination folder in cloud storage.</param>
    /// <param name="originalFileName">The original filename as submitted by the client.</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FileEntity" /> with persisted metadata.</returns>
    Task<FileEntity> UploadAndStoreVideoFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces a tracked file: soft-deletes the old FileEntity if it exists, then uploads a new image.
    /// </summary>
    /// <param name="currentFileId">The ID of the current file to replace, or null if none exists.</param>
    /// <param name="file">The new image file to upload.</param>
    /// <param name="publicId">The public ID used in cloud storage.</param>
    /// <param name="folder">The destination folder in cloud storage.</param>
    /// <param name="originalFileName">The original filename as submitted by the client.</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FileEntity" /> for the new upload.</returns>
    Task<FileEntity> ReplaceImageFileAsync(
        Guid? currentFileId,
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces a tracked video file: soft-deletes the old FileEntity if it exists, then uploads a new video.
    /// </summary>
    /// <param name="currentFileId">The ID of the current video file to replace, or null if none exists.</param>
    /// <param name="file">The new video file to upload.</param>
    /// <param name="publicId">The public ID used in cloud storage.</param>
    /// <param name="folder">The destination folder in cloud storage.</param>
    /// <param name="originalFileName">The original filename as submitted by the client.</param>
    /// <param name="mimeType">The MIME type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FileEntity" /> for the new upload.</returns>
    Task<FileEntity> ReplaceVideoFileAsync(
        Guid? currentFileId,
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Soft-deletes a file entity by its ID.
    /// </summary>
    /// <param name="fileId">The unique identifier of the file to soft-delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file was successfully soft-deleted; false if not found or already deleted.</returns>
    Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a raw file (image or PDF) to cloud storage and persists file metadata to the database.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="publicId">The public ID used in cloud storage.</param>
    /// <param name="folder">The destination folder in cloud storage.</param>
    /// <param name="originalFileName">The original filename as submitted by the client.</param>
    /// <param name="mimeType">The MIME type of the file (e.g., "image/jpeg", "application/pdf").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created <see cref="FileEntity" /> with persisted metadata.</returns>
    Task<FileEntity> UploadAndStoreRawFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );
}
