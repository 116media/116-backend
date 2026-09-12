using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Puts assets into cloud storage and records them. The two halves are separate so the network
/// call stays outside the transaction that records the file and the row referencing it.
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Uploads an image and extracts its colours. Performs no database work; the returned file
    /// has no row behind it until <see cref="RecordAsync" /> runs.
    /// </summary>
    /// <param name="file">The image to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded file describing what now sits in storage.</returns>
    Task<FileEntity> UploadImageAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a video. Performs no database work.
    /// </summary>
    /// <param name="file">The video to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded file describing what now sits in storage.</returns>
    Task<FileEntity> UploadVideoAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a raw asset such as a PDF. Performs no database work.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded file describing what now sits in storage.</returns>
    Task<FileEntity> UploadRawAsync(
        IFormFile file,
        string publicId,
        string folder,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Uploads a user avatar. Performs no database work.
    /// </summary>
    /// <param name="avatarFile">The image to upload.</param>
    /// <param name="userId">The owner, used as the storage public id.</param>
    /// <param name="originalFileName">The filename as submitted.</param>
    /// <param name="mimeType">The uploaded file's MIME type.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded file describing what now sits in storage.</returns>
    Task<FileEntity> UploadAvatarAsync(
        IFormFile avatarFile,
        string userId,
        string originalFileName,
        string mimeType,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches an avatar from a URL. Performs no database work. Returns null when the current
    /// avatar already points at that URL.
    /// </summary>
    /// <param name="currentAvatarFileId">The avatar in use, or null when there is none.</param>
    /// <param name="avatarUrl">The URL to fetch from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded file, or null when no update was needed.</returns>
    Task<FileEntity?> UploadAvatarFromUrlAsync(
        Guid? currentAvatarFileId,
        string avatarUrl,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages the file's row and the replacement of the file it supersedes. Call inside the
    /// transaction that also writes the row referencing it, so both land together.
    /// </summary>
    /// <param name="file">The unrecorded file returned by an upload.</param>
    /// <param name="supersededFileId">The file this one replaces, or null when none.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The staged file.</returns>
    /// <exception cref="Domain.Exceptions.CoreRuleException">The file already has a row behind it.</exception>
    Task<FileEntity> RecordAsync(
        FileEntity file,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    );
}
