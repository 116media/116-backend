using _116.Core.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Core.Application.Shared.Errors;

/// <summary>
/// File domain error factory providing simple, readable exception creation.
/// Usage: fileErrors.FileUploadFailed(reason) or fileErrors.FileNameRequired()
/// </summary>
public class FileErrors(ValidationErrorMessage validation, InternalServerErrorMessage internalServer)
{
    /// <summary>
    /// Throws when a file download fails from external URL.
    /// </summary>
    public InternalServerException FileDownloadFailed(string fileUrl, string reason) =>
        new(internalServer.FileDownloadFailed(fileUrl, reason));

    /// <summary>
    /// Throws a generic download failure that reveals neither the URL nor the underlying reason.
    /// Used when the target is rejected for safety or the provider response cannot be trusted.
    /// </summary>
    public InternalServerException FileDownloadFailed() => new(internalServer.FileDownloadFailedGeneric());

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
    /// Throws when the uploaded file exceeds the size limit, shown in megabytes.
    /// </summary>
    public BadRequestException FileTooLarge(long maxSizeMB) => new(validation.FileTooLargeWithLimit(maxSizeMB));

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
