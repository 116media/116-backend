using _116.Core.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Core.Application.Shared.Errors;

/// <summary>
/// File domain error factory providing simple, readable exception creation.
/// Usage: fileErrors.FileUploadFailed(fileName, reason) or fileErrors.FileNotFound(fileId)
/// </summary>
public class FileErrors(
    ConflictErrorMessage conflict,
    ValidationErrorMessage validation,
    InternalServerErrorMessage internalServer
)
{
    /// <summary>
    /// Throws when file upload fails.
    /// </summary>
    public ConflictException FileUploadFailed(string fileName, string reason) =>
        new(conflict.FileUploadFailed(fileName, reason));

    /// <summary>
    /// Throws when the file type is not supported.
    /// </summary>
    public BadRequestException UnsupportedFileType(string fileType, string[] allowedTypes) =>
        new(validation.UnsupportedFileType(fileType, allowedTypes));

    /// <summary>
    /// Throws when file size exceeds the limit.
    /// </summary>
    public BadRequestException FileTooLarge(long fileSize, long maxSize) =>
        new(validation.FileTooLarge(fileSize, maxSize));

    /// <summary>
    /// Throws when the file is corrupted.
    /// </summary>
    public BadRequestException CorruptedFile(string fileName) => new(validation.CorruptedFile(fileName));

    /// <summary>
    /// Throws when a file is not found.
    /// </summary>
    public NotFoundException FileNotFound(int fileId) => new("File", fileId);

    /// <summary>
    /// Throws when a file is not found using name.
    /// </summary>
    public NotFoundException FileNotFoundByName(string fileName) => new("File", "name", fileName);

    /// <summary>
    /// Throws when configuration is invalid.
    /// </summary>
    public BadRequestException InvalidConfiguration(string configKey) =>
        new(validation.InvalidConfiguration(configKey));

    /// <summary>
    /// Throws when external service is unavailable.
    /// </summary>
    public InternalServerException ServiceUnavailable(string serviceName) =>
        new(internalServer.ServiceUnavailable(serviceName));

    /// <summary>
    /// Throws when database connection fails.
    /// </summary>
    public InternalServerException DatabaseConnectionFailed() => new(internalServer.DatabaseConnectionFailed());

    /// <summary>
    /// Throws when file name is required.
    /// </summary>
    public BadRequestException FileNameRequired() => new(validation.FileNameRequired());

    /// <summary>
    /// Throws when original file name is required.
    /// </summary>
    public BadRequestException OriginalFileNameRequired() => new(validation.OriginalFileNameRequired());

    /// <summary>
    /// Throws when MIME type is required.
    /// </summary>
    public BadRequestException MimeTypeRequired() => new(validation.MimeTypeRequired());

    /// <summary>
    /// Throws when storage URL is required.
    /// </summary>
    public BadRequestException StorageUrlRequired() => new(validation.StorageUrlRequired());

    /// <summary>
    /// Throws when file size must be greater than zero.
    /// </summary>
    public BadRequestException FileSizeMustBeGreaterThanZero() => new(validation.FileSizeMustBeGreaterThanZero());

    /// <summary>
    /// Throws when a file download fails from external URL.
    /// </summary>
    public InternalServerException FileDownloadFailed(string fileUrl, string reason) =>
        new(internalServer.FileDownloadFailed(fileUrl, reason));

    /// <summary>
    /// Throws when the file URL format is invalid.
    /// </summary>
    public BadRequestException InvalidFileUrl(string fileUrl) => new(validation.InvalidFileUrl(fileUrl));

    /// <summary>
    /// Throws when file storage operation fails.
    /// </summary>
    public InternalServerException FileStorageFailed(string reason) => new(internalServer.FileStorageFailed(reason));

    /// <summary>
    /// Throws when file URL is required.
    /// </summary>
    public BadRequestException FileUrlRequired() => new(validation.FileUrlRequired());

    /// <summary>
    /// Throws when no file is provided in the upload request.
    /// </summary>
    public BadRequestException FileRequired() => new(validation.FileRequired());

    /// <summary>
    /// Throws when the uploaded file exceeds size limits (overload with MB display).
    /// </summary>
    public BadRequestException FileTooLarge(long actualSize, long maxSize, long maxSizeMB) =>
        new(validation.FileTooLargeWithLimit(maxSizeMB));

    /// <summary>
    /// Throws when the file type is not allowed.
    /// </summary>
    public BadRequestException InvalidFileType(string providedType, string allowedTypes) =>
        new(validation.InvalidFileType(providedType, allowedTypes));

    /// <summary>
    /// Throws when the file extension is not allowed.
    /// </summary>
    public BadRequestException InvalidFileExtension(string providedExtension, string allowedExtensions) =>
        new(validation.InvalidFileExtension(providedExtension, allowedExtensions));

    /// <summary>
    /// Throws when a file upload fails (overload with just reason).
    /// </summary>
    public BadGatewayException FileUploadFailed(string reason) => new(validation.FileUploadFailed(reason));
}
