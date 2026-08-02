using Microsoft.Extensions.Localization;

namespace _116.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Provides validation-related error messages for the <c>Core</c> domain.
/// These messages describe failures due to invalid input or format requirements.
/// </summary>
public class ValidationErrorMessage(IStringLocalizer<ValidationErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Error message indicating that file name is required.
    /// </summary>
    public string FileNameRequired()
    {
        return localizer["FileNameRequired"];
    }

    /// <summary>
    /// Error message indicating that the original file name is required.
    /// </summary>
    public string OriginalFileNameRequired()
    {
        return localizer["OriginalFileNameRequired"];
    }

    /// <summary>
    /// Error message indicating that MIME type is required.
    /// </summary>
    public string MimeTypeRequired()
    {
        return localizer["MimeTypeRequired"];
    }

    /// <summary>
    /// Error message indicating that storage URL is required.
    /// </summary>
    public string StorageUrlRequired()
    {
        return localizer["StorageUrlRequired"];
    }

    /// <summary>
    /// Error message indicating that storage URL cannot be empty.
    /// </summary>
    public string StorageUrlCannotBeEmpty()
    {
        return localizer["StorageUrlCannotBeEmpty"];
    }

    /// <summary>
    /// Error message indicating that file size must be greater than zero.
    /// </summary>
    public string FileSizeMustBeGreaterThanZero()
    {
        return localizer["FileSizeMustBeGreaterThanZero"];
    }

    /// <summary>
    /// Gets the error message for invalid file URL format.
    /// </summary>
    /// <param name="fileUrl">The invalid file URL.</param>
    /// <returns>A formatted error message indicating the URL format is invalid.</returns>
    public string InvalidFileUrl(string fileUrl)
    {
        return string.Format(localizer["InvalidFileUrl"], fileUrl);
    }

    /// <summary>
    /// Error message indicating that file URL is required.
    /// </summary>
    public string FileUrlRequired()
    {
        return localizer["FileUrlRequired"];
    }

    /// <summary>
    /// Error message indicating that no file was provided for upload.
    /// </summary>
    public string FileRequired()
    {
        return localizer["FileRequired"];
    }

    /// <summary>
    /// Error message indicating that file size exceeds the limit (with MB display).
    /// </summary>
    /// <param name="maxSizeMB">The maximum allowed size in megabytes.</param>
    public string FileTooLargeWithLimit(long maxSizeMB)
    {
        return string.Format(localizer["FileTooLargeWithLimit"], maxSizeMB);
    }

    /// <summary>
    /// Error message indicating that the file type is not allowed.
    /// </summary>
    /// <param name="providedType">The provided MIME type.</param>
    /// <param name="allowedTypes">Comma-separated list of allowed MIME types.</param>
    public string InvalidFileType(string providedType, string allowedTypes)
    {
        return string.Format(localizer["InvalidFileType"], providedType, allowedTypes);
    }

    /// <summary>
    /// Error message indicating that the file extension is not allowed.
    /// </summary>
    /// <param name="providedExtension">The provided file extension.</param>
    /// <param name="allowedExtensions">Comma-separated list of allowed extensions.</param>
    public string InvalidFileExtension(string providedExtension, string allowedExtensions)
    {
        return string.Format(localizer["InvalidFileExtension"], providedExtension, allowedExtensions);
    }

    /// <summary>
    /// Error message indicating that a file upload failed.
    /// </summary>
    /// <param name="reason">The reason the upload failed.</param>
    public string FileUploadFailed(string reason)
    {
        return string.Format(localizer["FileUploadFailed"], reason);
    }
}
