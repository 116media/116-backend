using _116.BuildingBlocks.Constants;
using _116.Core.Application.Shared.Errors;
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

    public CloudinaryService(CloudinarySettings config, ILogger<CloudinaryService> logger)
    {
        _logger = logger;

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
                throw CoreErrors.FileUploadFailed(result.Error.Message);
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
            throw CoreErrors.FileUploadFailed(ex.Message);
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
                    throw CoreErrors.FileUploadFailed(result.Error.Message);
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
                    throw CoreErrors.FileUploadFailed(result.Error.Message);
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
            throw CoreErrors.FileUploadFailed(ex.Message);
        }
    }

    /// <summary>
    /// Validates the uploaded file for size and type constraints.
    /// </summary>
    private static void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw CoreErrors.FileRequired();
        }

        // Check file size
        if (file.Length > FileConstants.MaxAvatarFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024);
            throw CoreErrors.FileTooLarge(file.Length, FileConstants.MaxAvatarFileSizeBytes, maxSizeMb);
        }

        // Check file extension first (more reliable than MIME type for mobile uploads)
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedAvatarExtensions.Contains(extension))
        {
            throw CoreErrors.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedAvatarExtensions));
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
            throw CoreErrors.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedAvatarMimeTypes));
        }
    }

    /// <summary>
    /// Validates a raw file against size (5 MB) and type (images + PDF) constraints.
    /// </summary>
    private static void ValidateRawFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw CoreErrors.FileRequired();
        }

        if (file.Length > FileConstants.MaxRawFileSizeBytes)
        {
            const long maxSizeMb = FileConstants.MaxRawFileSizeBytes / (1024 * 1024);
            throw CoreErrors.FileTooLarge(file.Length, FileConstants.MaxRawFileSizeBytes, maxSizeMb);
        }

        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!FileConstants.AllowedRawFileExtensions.Contains(extension))
        {
            throw CoreErrors.InvalidFileExtension(extension, string.Join(", ", FileConstants.AllowedRawFileExtensions));
        }

        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        bool isValidContentType =
            FileConstants.AllowedRawFileMimeTypes.Contains(contentType)
            || string.IsNullOrEmpty(contentType)
            || contentType == "application/octet-stream"
            || contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw CoreErrors.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedRawFileMimeTypes));
        }
    }
}
