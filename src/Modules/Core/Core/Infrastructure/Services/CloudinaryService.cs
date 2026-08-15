using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Errors.Facade;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="ICloudinaryService"/> for Cloudinary cloud storage operations.
/// </summary>
public class CloudinaryService : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryService> _logger;
    private readonly CoreI18n _i18n;

    public CloudinaryService(CloudinarySettings config, ILogger<CloudinaryService> logger, CoreI18n i18n)
    {
        _logger = logger;
        _i18n = i18n;

        // Initialize Cloudinary Account
        var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);

        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

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
            // Prepare upload parameters
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = publicId,
                Folder = folder,
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = false,
            };

            // Perform signed upload
            ImageUploadResult result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            // Check for errors
            if (result.Error != null)
            {
                _logger.LogError("Cloudinary upload failed: {ErrorMessage}", result.Error.Message);
                throw _i18n.File.FileUploadFailed(result.Error.Message);
            }

            _logger.LogInformation(
                "Successfully uploaded file to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                result.PublicId,
                result.Bytes
            );

            return new CloudinaryUploadResult(
                PublicId: result.PublicId,
                SecureUrl: result.SecureUrl.ToString(),
                Format: result.Format,
                Width: result.Width,
                Height: result.Height,
                Bytes: result.Bytes,
                ResourceType: result.ResourceType
            );
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "Unexpected error during Cloudinary upload");
            throw _i18n.File.FileUploadFailed(ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        try
        {
            var deletionParams = new DeletionParams(publicId);
            DeletionResult result = await _cloudinary.DestroyAsync(deletionParams);

            if (result.Error != null)
            {
                _logger.LogWarning(
                    "Cloudinary deletion warning for publicId {PublicId}: {ErrorMessage}",
                    publicId,
                    result.Error.Message
                );
                return false;
            }

            bool deleted = result.Result == "ok";
            _logger.LogInformation(
                "Cloudinary deletion result for publicId {PublicId}: {Result}",
                publicId,
                result.Result
            );
            return deleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting Cloudinary resource {PublicId}", publicId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteImagesAsync(
        IEnumerable<string> publicIds,
        CancellationToken cancellationToken = default
    )
    {
        List<string> keys = publicIds.ToList();
        if (keys.Count == 0)
        {
            return true;
        }

        // Cloudinary batch delete supports a maximum of 100 public IDs per request.
        // All batches are dispatched concurrently via Task.WhenAll — same approach as
        // firing Promise.all() in JavaScript — so a 500-item list sends 5 requests in parallel
        // instead of 5 sequential round trips.
        const int batchSize = 100;

        IEnumerable<Task<bool>> batchTasks = Enumerable
            .Range(0, (keys.Count + batchSize - 1) / batchSize)
            .Select(batchIndex =>
            {
                List<string> batch = keys.Skip(batchIndex * batchSize).Take(batchSize).ToList();
                return DeleteBatchAsync(batch, batchIndex);
            });

        bool[] results = await Task.WhenAll(batchTasks);
        return results.All(r => r);
    }

    /// <summary>
    /// Sends a single Cloudinary batch-delete request for up to 100 public IDs.
    /// </summary>
    private async Task<bool> DeleteBatchAsync(List<string> batch, int batchIndex)
    {
        try
        {
            var delParams = new DelResParams
            {
                PublicIds = batch,
                Type = "upload",
                ResourceType = ResourceType.Image,
            };

            DelResResult result = await _cloudinary.DeleteResourcesAsync(delParams);

            if (result.Error != null)
            {
                _logger.LogWarning(
                    "Cloudinary batch deletion warning (batch {BatchIndex}): {ErrorMessage}",
                    batchIndex,
                    result.Error.Message
                );
                return false;
            }

            _logger.LogInformation(
                "Successfully deleted {Count} Cloudinary resources in batch {BatchIndex}",
                batch.Count,
                batchIndex
            );
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during Cloudinary batch deletion (batch {BatchIndex})", batchIndex);
            return false;
        }
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

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        bool isPdf = extension == ".pdf";

        try
        {
            if (isPdf)
            {
                var uploadParams = new RawUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    PublicId = publicId,
                    Folder = folder,
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false,
                };

                RawUploadResult result = await Task.Run(() => _cloudinary.Upload(uploadParams), cancellationToken);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {ErrorMessage}", result.Error.Message);
                    throw _i18n.File.FileUploadFailed(result.Error.Message);
                }

                _logger.LogInformation(
                    "Successfully uploaded raw PDF to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                    result.PublicId,
                    result.Bytes
                );

                return new CloudinaryUploadResult(
                    PublicId: result.PublicId,
                    SecureUrl: result.SecureUrl.ToString(),
                    Format: result.Format,
                    Width: 0,
                    Height: 0,
                    Bytes: result.Bytes,
                    ResourceType: result.ResourceType
                );
            }
            else
            {
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    PublicId = publicId,
                    Folder = folder,
                    Overwrite = true,
                    UniqueFilename = false,
                    UseFilename = false,
                };

                ImageUploadResult result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary upload failed: {ErrorMessage}", result.Error.Message);
                    throw _i18n.File.FileUploadFailed(result.Error.Message);
                }

                _logger.LogInformation(
                    "Successfully uploaded raw image to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                    result.PublicId,
                    result.Bytes
                );

                return new CloudinaryUploadResult(
                    PublicId: result.PublicId,
                    SecureUrl: result.SecureUrl.ToString(),
                    Format: result.Format,
                    Width: result.Width,
                    Height: result.Height,
                    Bytes: result.Bytes,
                    ResourceType: result.ResourceType
                );
            }
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "Unexpected error during Cloudinary raw file upload");
            throw _i18n.File.FileUploadFailed(ex.Message);
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
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = publicId,
                Folder = folder,
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = false,
            };

            VideoUploadResult result = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

            if (result.Error != null)
            {
                _logger.LogError("Cloudinary video upload failed: {ErrorMessage}", result.Error.Message);
                throw _i18n.File.FileUploadFailed(result.Error.Message);
            }

            _logger.LogInformation(
                "Successfully uploaded video to Cloudinary: {PublicId}, Size: {Bytes} bytes",
                result.PublicId,
                result.Bytes
            );

            return new CloudinaryUploadResult(
                PublicId: result.PublicId,
                SecureUrl: result.SecureUrl.ToString(),
                Format: result.Format,
                Width: result.Width,
                Height: result.Height,
                Bytes: result.Bytes,
                ResourceType: result.ResourceType
            );
        }
        catch (Exception ex) when (ex is not BadRequestException)
        {
            _logger.LogError(ex, "Unexpected error during Cloudinary video upload");
            throw _i18n.File.FileUploadFailed(ex.Message);
        }
    }

    /// <summary>
    /// Validates the uploaded file for size and type constraints.
    /// </summary>
    private void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw _i18n.File.FileRequired();
        }

        // Check file size
        if (file.Length > FileConstants.MaxAvatarFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024);
            throw _i18n.File.FileTooLarge(maxSizeMb);
        }

        // Check file extension first (more reliable than MIME type for mobile uploads)
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedAvatarExtensions.Contains(extension))
        {
            throw _i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedAvatarExtensions));
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
            throw _i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedAvatarMimeTypes));
        }
    }

    /// <summary>
    /// Validates a raw file against size (5 MB) and type (images + PDF) constraints.
    /// </summary>
    private void ValidateRawFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw _i18n.File.FileRequired();
        }

        if (file.Length > FileConstants.MaxRawFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxRawFileSizeBytes / (1024 * 1024);
            throw _i18n.File.FileTooLarge(maxSizeMb);
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedRawFileExtensions.Contains(extension))
        {
            throw _i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedRawFileExtensions));
        }

        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        bool isValidContentType =
            FileConstants.AllowedRawFileMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw _i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedRawFileMimeTypes));
        }
    }

    /// <summary>
    /// Validates a video file against size (100 MB) and type (video formats) constraints.
    /// </summary>
    private void ValidateVideoFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw _i18n.File.FileRequired();
        }

        if (file.Length > FileConstants.MaxVideoFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxVideoFileSizeBytes / (1024 * 1024);
            throw _i18n.File.FileTooLarge(maxSizeMb);
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedVideoExtensions.Contains(extension))
        {
            throw _i18n.File.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedVideoExtensions));
        }

        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        bool isValidContentType =
            FileConstants.AllowedVideoMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw _i18n.File.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedVideoMimeTypes));
        }
    }
}
