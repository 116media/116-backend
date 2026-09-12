using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICloudinaryService"/> for Cloudinary cloud storage operations.
/// Validation and result mapping live here; the provider call and its resilience policy live
/// behind <see cref="ICloudStorageClient" />.
/// </summary>
/// <param name="client">The resilient seam over the provider SDK.</param>
/// <param name="logger">Logger for upload and deletion diagnostics.</param>
/// <param name="i18n">The Core i18n facade for localized errors.</param>
public class CloudinaryService(ICloudStorageClient client, ILogger<CloudinaryService> logger, CoreI18n i18n)
    : ICloudinaryService
{
    /// <inheritdoc />
    public async Task<CloudinaryUploadResult> UploadImageAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        // Validate file
        ValidateFile(file);

        try
        {
            CloudStorageAsset asset = await client.UploadAsync(
                Describe(file, publicId, folder, EnumStoredFileKind.Image),
                cancellationToken
            );

            logger.LogInformation(
                "Successfully uploaded file to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                asset.PublicId,
                asset.Bytes
            );

            return Project(asset);
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            logger.LogError(ex, "Unexpected error during Cloudinary upload");
            throw i18n.File.FileUploadFailed(ex.Message);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(
        string publicId,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        return client.DeleteAsync(publicId, kind, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> DeleteManyAsync(
        IEnumerable<string> publicIds,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        return client.DeleteManyAsync(publicIds, kind, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CloudinaryUploadResult> UploadRawAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        ValidateRawFile(file);

        // A PDF must go up as a raw asset; anything else in this path is still an image to the
        // provider, and uploading it as raw would lose the derived dimensions.
        bool isPdf = Path.GetExtension(file.FileName).ToLowerInvariant() == ".pdf";
        EnumStoredFileKind kind = isPdf ? EnumStoredFileKind.Raw : EnumStoredFileKind.Image;

        try
        {
            CloudStorageAsset asset = await client.UploadAsync(
                Describe(file, publicId, folder, kind),
                cancellationToken
            );

            logger.LogInformation(
                "Successfully uploaded raw {Kind} to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                kind,
                asset.PublicId,
                asset.Bytes
            );

            return Project(asset);
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            logger.LogError(ex, "Unexpected error during Cloudinary raw file upload");
            throw i18n.File.FileUploadFailed(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<CloudinaryUploadResult> UploadVideoAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        ValidateVideoFile(file);

        try
        {
            CloudStorageAsset asset = await client.UploadAsync(
                Describe(file, publicId, folder, EnumStoredFileKind.Video),
                cancellationToken
            );

            logger.LogInformation(
                "Successfully uploaded video to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                asset.PublicId,
                asset.Bytes
            );

            return Project(asset);
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            logger.LogError(ex, "Unexpected error during Cloudinary video upload");
            throw i18n.File.FileUploadFailed(ex.Message);
        }
    }

    /// <summary>
    /// Describes a validated upload for the storage client.
    /// </summary>
    /// <param name="file">The submitted file.</param>
    /// <param name="publicId">The identifier to store it under.</param>
    /// <param name="folder">The folder to store it in, or null for the root.</param>
    /// <param name="kind">What the asset is.</param>
    /// <returns>The upload request.</returns>
    private static CloudStorageUpload Describe(
        IFormFile file,
        string publicId,
        string? folder,
        EnumStoredFileKind kind
    ) => new(file.OpenReadStream(), file.FileName, publicId, folder, kind);

    /// <summary>
    /// Projects a stored asset onto the result this service returns.
    /// </summary>
    /// <param name="asset">The stored asset.</param>
    /// <returns>The upload result.</returns>
    private static CloudinaryUploadResult Project(CloudStorageAsset asset) =>
        new(
            PublicId: asset.PublicId,
            SecureUrl: asset.SecureUrl,
            Format: asset.Format,
            Width: asset.Width,
            Height: asset.Height,
            Bytes: asset.Bytes,
            ResourceType: asset.ResourceType
        );

    /// <summary>
    /// Validates the uploaded file for size and type constraints.
    /// </summary>
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw i18n.File.FileRequired();
        }

        // Check file size
        if (file.Length > FileConstants.MaxAvatarFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024);
            throw i18n.File.FileTooLarge(maxSizeMb);
        }

        // Check file extension first (more reliable than MIME type for mobile uploads)
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedAvatarExtensions.Contains(extension))
        {
            throw i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedAvatarExtensions));
        }

        // Extract content type without parameters (e.g., "image/jpeg" from "image/jpeg; boundary=...")
        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        // Allow if content type is in allowed list OR if it's a generic type (mobile uploads)
        bool isValidContentType =
            FileConstants.AllowedAvatarMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedAvatarMimeTypes));
        }
    }

    /// <summary>
    /// Validates a raw file against size (5 MB) and type (images + PDF) constraints.
    /// </summary>
    private void ValidateRawFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw i18n.File.FileRequired();
        }

        if (file.Length > FileConstants.MaxRawFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxRawFileSizeBytes / (1024 * 1024);
            throw i18n.File.FileTooLarge(maxSizeMb);
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedRawFileExtensions.Contains(extension))
        {
            throw i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedRawFileExtensions));
        }

        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        bool isValidContentType =
            FileConstants.AllowedRawFileMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedRawFileMimeTypes));
        }
    }

    /// <summary>
    /// Validates a video file against size (100 MB) and type (video formats) constraints.
    /// </summary>
    private void ValidateVideoFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw i18n.File.FileRequired();
        }

        if (file.Length > FileConstants.MaxVideoFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxVideoFileSizeBytes / (1024 * 1024);
            throw i18n.File.FileTooLarge(maxSizeMb);
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedVideoExtensions.Contains(extension))
        {
            throw i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedVideoExtensions));
        }

        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        bool isValidContentType =
            FileConstants.AllowedVideoMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedVideoMimeTypes));
        }
    }
}
