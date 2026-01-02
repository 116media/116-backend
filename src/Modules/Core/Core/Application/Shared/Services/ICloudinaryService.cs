using _116.Shared.Application.Exceptions;

using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// Service for managing file uploads to Cloudinary cloud storage.
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
