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
    /// Gets the error message for the unsupported file type.
    /// </summary>
    /// <param name="fileType">The unsupported file type.</param>
    /// <param name="allowedTypes">Array of allowed file types.</param>
    /// <returns>A formatted error message indicating the file type is not supported.</returns>
    public string UnsupportedFileType(string fileType, string[] allowedTypes)
    {
        return string.Format(localizer["UnsupportedFileType"], fileType, string.Join(", ", allowedTypes));
    }

    /// <summary>
    /// Gets the error message for the file too large.
    /// </summary>
    /// <param name="fileSize">The actual file size in bytes.</param>
    /// <param name="maxSize">The maximum allowed file size in bytes.</param>
    /// <returns>A formatted error message indicating the file size exceeds the limit.</returns>
    public string FileTooLarge(long fileSize, long maxSize)
    {
        return string.Format(localizer["FileTooLarge"], fileSize, maxSize);
    }

    /// <summary>
    /// Gets the error message for the corrupted file.
    /// </summary>
    /// <param name="fileName">The name of the corrupted file.</param>
    /// <returns>A formatted error message indicating the file is corrupted.</returns>
    public string CorruptedFile(string fileName)
    {
        return string.Format(localizer["CorruptedFile"], fileName);
    }

    /// <summary>
    /// Gets the error message for invalid configuration.
    /// </summary>
    /// <param name="configKey">The configuration key that is invalid.</param>
    /// <returns>A formatted error message indicating the configuration is invalid.</returns>
    public string InvalidConfiguration(string configKey)
    {
        return string.Format(localizer["InvalidConfiguration"], configKey);
    }

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
}
