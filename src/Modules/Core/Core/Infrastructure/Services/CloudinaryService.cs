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
        var account = new Account(
            config.CloudName,
            config.ApiKey,
            config.ApiSecret
        );

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
                File = new FileDescription(
                    file.FileName,
                    file.OpenReadStream()
                ),
                PublicId = publicId,
                Folder = folder,
                Overwrite = true,
                UniqueFilename = false,
                UseFilename = false
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
            throw CoreErrors.InvalidFileExtension(
                extension,
                string.Join(", ", FileConstants.AllowedAvatarExtensions)
            );
        }

        // Extract content type without parameters (e.g., "image/jpeg" from "image/jpeg; boundary=...")
        string contentType = (file.ContentType?.Split(';')[0] ?? string.Empty).Trim().ToLowerInvariant();

        // Allow if content type is in allowed list OR if it's a generic type (mobile uploads)
        bool isValidContentType = FileConstants.AllowedAvatarMimeTypes.Contains(contentType) ||
                                   string.IsNullOrEmpty(contentType) ||
                                   contentType == "application/octet-stream" ||
                                   contentType == "multipart/form-data";

        if (!isValidContentType)
        {
            throw CoreErrors.InvalidFileType(contentType, string.Join(", ", FileConstants.AllowedAvatarMimeTypes));
        }
    }
}
