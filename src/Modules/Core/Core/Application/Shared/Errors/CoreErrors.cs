using _116.Core.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Core.Application.Shared.Errors;

/// <summary>
/// Core domain error factory providing simple, readable exception creation.
/// Usage: CoreErrors.FileUploadFailed(fileName, reason) or CoreErrors.FileNotFound(fileId)
/// </summary>
public static class CoreErrors
{
    /// <summary>
    /// Throws when file upload fails.
    /// </summary>
    public static ConflictException FileUploadFailed(string fileName, string reason) =>
        new(ConflictErrorMessage.FileUploadFailed(fileName, reason));

    /// <summary>
    /// Throws when the file type is not supported.
    /// </summary>
    public static BadRequestException UnsupportedFileType(string fileType, string[] allowedTypes) =>
        new(ValidationErrorMessage.UnsupportedFileType(fileType, allowedTypes));

    /// <summary>
    /// Throws when file size exceeds the limit.
    /// </summary>
    public static BadRequestException FileTooLarge(long fileSize, long maxSize) =>
        new(ValidationErrorMessage.FileTooLarge(fileSize, maxSize));

    /// <summary>
    /// Throws when the file is corrupted.
    /// </summary>
    public static BadRequestException CorruptedFile(string fileName) =>
        new(ValidationErrorMessage.CorruptedFile(fileName));

    /// <summary>
    /// Throws when a file is not found.
    /// </summary>
    public static NotFoundException FileNotFound(int fileId) => new("File", fileId);

    /// <summary>
    /// Throws when a file is not found using name.
    /// </summary>
    public static NotFoundException FileNotFoundByName(string fileName) => new("File", "name", fileName);

    /// <summary>
    /// Throws when configuration is invalid.
    /// </summary>
    public static BadRequestException InvalidConfiguration(string configKey) =>
        new(ValidationErrorMessage.InvalidConfiguration(configKey));

    /// <summary>
    /// Throws when external service is unavailable.
    /// </summary>
    public static InternalServerException ServiceUnavailable(string serviceName) =>
        new(InternalServerErrorMessage.ServiceUnavailable(serviceName));

    /// <summary>
    /// Throws when database connection fails.
    /// </summary>
    public static InternalServerException DatabaseConnectionFailed() =>
        new(InternalServerErrorMessage.DatabaseConnectionFailed());

    /// <summary>
    /// Throws when file name is required.
    /// </summary>
    public static BadRequestException FileNameRequired() => new(ValidationErrorMessage.FileNameRequired());

    /// <summary>
    /// Throws when original file name is required.
    /// </summary>
    public static BadRequestException OriginalFileNameRequired() =>
        new(ValidationErrorMessage.OriginalFileNameRequired());

    /// <summary>
    /// Throws when MIME type is required.
    /// </summary>
    public static BadRequestException MimeTypeRequired() => new(ValidationErrorMessage.MimeTypeRequired());

    /// <summary>
    /// Throws when storage URL is required.
    /// </summary>
    public static BadRequestException StorageUrlRequired() => new(ValidationErrorMessage.StorageUrlRequired());

    /// <summary>
    /// Throws when file size must be greater than zero.
    /// </summary>
    public static BadRequestException FileSizeMustBeGreaterThanZero() =>
        new(ValidationErrorMessage.FileSizeMustBeGreaterThanZero());

    /// <summary>
    /// Throws when a file download fails from external URL.
    /// </summary>
    public static InternalServerException FileDownloadFailed(string fileUrl, string reason) =>
        new(InternalServerErrorMessage.FileDownloadFailed(fileUrl, reason));

    /// <summary>
    /// Throws when the file URL format is invalid.
    /// </summary>
    public static BadRequestException InvalidFileUrl(string fileUrl) =>
        new(ValidationErrorMessage.InvalidFileUrl(fileUrl));

    /// <summary>
    /// Throws when file storage operation fails.
    /// </summary>
    public static InternalServerException FileStorageFailed(string reason) =>
        new(InternalServerErrorMessage.FileStorageFailed(reason));

    /// <summary>
    /// Throws when file URL is required.
    /// </summary>
    public static BadRequestException FileUrlRequired() => new(ValidationErrorMessage.FileUrlRequired());

    /// <summary>
    /// Throws a generic bad request exception with the custom message.
    /// </summary>
    public static BadRequestException BadRequest(string message) => new(message);

    /// <summary>
    /// Error for when no file is provided in the upload request.
    /// </summary>
    public static BadRequestException FileRequired() => new("File.Required", "No file was provided for upload");

    /// <summary>
    /// Error for when the uploaded file exceeds size limits (overload with detailed info).
    /// </summary>
    public static BadRequestException FileTooLarge(long actualSize, long maxSize, long maxSizeMB) =>
        new(
            "File.TooLarge",
            $"File size ({actualSize} bytes) exceeds maximum allowed size of {maxSizeMB}MB ({maxSize} bytes)"
        );

    /// <summary>
    /// Error for Invalid filetype.
    /// </summary>
    public static BadRequestException InvalidFileType(string providedType, string allowedTypes) =>
        new("File.InvalidType", $"File type '{providedType}' is not allowed. Allowed types: {allowedTypes}");

    /// <summary>
    /// Error for invalid file extension.
    /// </summary>
    public static BadRequestException InvalidFileExtension(string providedExtension, string allowedExtensions) =>
        new(
            "File.InvalidExtension",
            $"File extension '{providedExtension}' is not allowed. Allowed extensions: {allowedExtensions}"
        );

    /// <summary>
    /// Error for file upload failures (overload with just reason).
    /// </summary>
    public static BadGatewayException FileUploadFailed(string reason) =>
        new("File.UploadFailed", $"File upload failed: {reason}");
}
