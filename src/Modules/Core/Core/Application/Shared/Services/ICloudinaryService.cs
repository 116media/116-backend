using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Service for managing file uploads and deletions in Cloudinary cloud storage.
/// </summary>
public interface ICloudinaryService
{
    /// <summary>
    /// Uploads an image file to Cloudinary using signed upload with Overwrite enabled.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="publicId">The public ID for the file (typically userId for avatars).</param>
    /// <param name="folder">Optional folder path in Cloudinary (e.g., "avatars").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Upload result containing the public URL, public ID, and metadata.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the file is null.</exception>
    /// <exception cref="BadGatewayException">Thrown for an invalid file type or size.</exception>
    Task<CloudinaryUploadResult> UploadImageAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a single image from Cloudinary by its storage key (public ID).
    /// Named <c>storageKey</c> rather than <c>cloudinaryPublicId</c> to remain CDN-agnostic:
    /// if the storage provider changes (e.g. S3, Bunny CDN), the call site does not need to change.
    /// </summary>
    /// <param name="storageKey">The provider-agnostic storage key (Cloudinary public ID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if the resource was deleted; <c>false</c> if it was not found.</returns>
    Task<bool> DeleteImageAsync(string storageKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple images from Cloudinary in a single batch request.
    /// Named <c>storageKeys</c> rather than <c>cloudinaryPublicIds</c> to remain CDN-agnostic.
    /// Cloudinary batch delete supports a maximum of 100 keys per call; this implementation
    /// automatically splits larger collections into batches of 100.
    /// </summary>
    /// <param name="storageKeys">The provider-agnostic storage keys (Cloudinary public IDs) to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><c>true</c> if all deletions succeeded; <c>false</c> if any resource was not found.</returns>
    Task<bool> DeleteImagesAsync(IEnumerable<string> storageKeys, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a Cloudinary upload operation.
/// </summary>
/// <param name="PublicId">The Cloudinary public ID of the uploaded file.</param>
/// <param name="SecureUrl">The HTTPS URL to access the uploaded file.</param>
/// <param name="Format">The file format (e.g., "jpg", "png").</param>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
/// <param name="Bytes">The file size in bytes.</param>
/// <param name="ResourceType">The resource type (e.g., "image").</param>
public record CloudinaryUploadResult(
    string PublicId,
    string SecureUrl,
    string Format,
    int Width,
    int Height,
    long Bytes,
    string ResourceType
);
