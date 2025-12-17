using _116.Shared.Application.Exceptions;

using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Result of a file upload operation.
/// </summary>
/// <param name="FileId">The generated unique identifier for the file.</param>
/// <param name="SecureUrl">The HTTPS URL to access the uploaded file.</param>
/// <param name="Format">The file format (e.g., "jpg", "png").</param>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
/// <param name="Bytes">The file size in bytes.</param>
public record FileUploadResult(
    Guid FileId,
    string SecureUrl,
    string Format,
    int Width,
    int Height,
    long Bytes
);

/// <summary>
/// Result of a file download operation containing metadata about the downloaded file.
/// </summary>
/// <param name="FileId">The generated unique identifier for the file.</param>
/// <param name="FileName">The generated filename with extension (e.g., "guid.jpg").</param>
/// <param name="OriginalFileName">The original filename from the URL or default name.</param>
/// <param name="MimeType">The resolved MIME type of the file.</param>
/// <param name="StorageUrl">The original URL where the file is stored.</param>
/// <param name="SizeInBytes">The file size in bytes.</param>
public record FileDownloadResult(
    Guid FileId,
    string FileName,
    string OriginalFileName,
    string MimeType,
    string StorageUrl,
    long SizeInBytes
);

/// <summary>
/// Service interface for file operations including download, storage, and management.
/// </summary>
/// <remarks>
/// This service handles file operations such as downloading files from URLs,
/// storing them locally, and managing file metadata.
/// </remarks>
public interface IFileService
{
    /// <summary>
    /// Uploads a file to cloud storage and returns the upload result with metadata.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="publicId">The public ID for the file (typically userId for avatars).</param>
    /// <param name="folder">Optional folder path in cloud storage.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result containing public URL, metadata, and generated file ID.</returns>
    /// <exception cref="BadRequestException">Thrown for invalid file type or size.</exception>
    /// <remarks>
    /// This method uploads files to Cloudinary cloud storage and:
    /// - Validates file type and size
    /// - Uploads to Cloudinary with optimization and overwrite enabled
    /// - Returns metadata including secure URL, format, dimensions, and size
    /// - Does NOT persist to repository (caller is responsible for persistence)
    /// </remarks>
    Task<FileUploadResult> UploadFileAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Downloads file metadata from a specified URL.
    /// </summary>
    /// <param name="fileUrl">The URL of the file to download metadata for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>File metadata including ID, filename, MIME type, size, and storage URL.</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is invalid.</exception>
    /// <exception cref="HttpRequestException">Thrown when the file metadata cannot be retrieved.</exception>
    /// <exception cref="InvalidOperationException">Thrown when metadata extraction fails.</exception>
    /// <remarks>
    /// This method retrieves metadata from external file sources (like social provider avatars)
    /// without actually downloading the file content. The metadata can be used to create
    /// a FileEntity that references the external URL.
    /// </remarks>
    Task<FileDownloadResult> DownloadFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}
