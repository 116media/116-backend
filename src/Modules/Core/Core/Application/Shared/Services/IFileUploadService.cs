using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Uploads and replaces stored assets: talks to cloud storage, then records the resulting
/// <see cref="FileEntity" />. Reads and staging belong to the file repository.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Replaces the avatar with an uploaded file, marking the current one replaced.
    /// </summary>
    /// <param name="currentAvatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="avatarFile">The file to upload.</param>
    /// <param name="userId">The owner, used as the storage public id.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    Task<FileEntity> UpdateAvatarFromFileAsync(
        Guid? currentAvatarFileId,
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces the avatar with one fetched from a URL, marking the current one replaced.
    /// Returns null when the current avatar already points at that URL.
    /// </summary>
    /// <param name="currentAvatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="newAvatarUrl">The URL to fetch the avatar from.</param>
    /// <param name="userId">The owner, used as the storage public id.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file, or null when no update was needed.</returns>
    Task<FileEntity?> UpdateAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string newAvatarUrl,
        string userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Replaces a tracked image, marking the current one replaced.
    /// </summary>
    /// <param name="currentFileId">The image in use, or null when there is none.</param>
    /// <param name="file">The image to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
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
    /// Replaces a tracked video, marking the current one replaced.
    /// </summary>
    /// <param name="currentFileId">The video in use, or null when there is none.</param>
    /// <param name="file">The video to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
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
    /// Uploads a raw asset such as an image or PDF and records it.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored file.</returns>
    Task<FileEntity> UploadAndStoreRawFileAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );
}
