namespace _116.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Provides validation-related error messages for the <c>Core</c> domain.
/// These messages describe failures due to invalid input or format requirements.
/// </summary>
public static class ValidationErrorMessage
{
    /// <summary>
    /// Gets the error message for the unsupported file type.
    /// </summary>
    /// <param name="fileType">The unsupported file type.</param>
    /// <param name="allowedTypes">Array of allowed file types.</param>
    /// <returns>A formatted error message indicating the file type is not supported.</returns>
    public static string UnsupportedFileType(string fileType, string[] allowedTypes)
    {
        return $"File type '{fileType}' is not supported. Allowed types: {string.Join(", ", allowedTypes)}";
    }

    /// <summary>
    /// Gets the error message for the file too large.
    /// </summary>
    /// <param name="fileSize">The actual file size in bytes.</param>
    /// <param name="maxSize">The maximum allowed file size in bytes.</param>
    /// <returns>A formatted error message indicating the file size exceeds the limit.</returns>
    public static string FileTooLarge(long fileSize, long maxSize)
    {
        return $"File size {fileSize} bytes exceeds maximum allowed size of {maxSize} bytes";
    }

    /// <summary>
    /// Gets the error message for the corrupted file.
    /// </summary>
    /// <param name="fileName">The name of the corrupted file.</param>
    /// <returns>A formatted error message indicating the file is corrupted.</returns>
    public static string CorruptedFile(string fileName)
    {
        return $"File '{fileName}' appears to be corrupted or invalid";
    }

    /// <summary>
    /// Gets the error message for invalid configuration.
    /// </summary>
    /// <param name="configKey">The configuration key that is invalid.</param>
    /// <returns>A formatted error message indicating the configuration is invalid.</returns>
    public static string InvalidConfiguration(string configKey)
    {
        return $"Configuration '{configKey}' is missing or invalid";
    }

    /// <summary>
    /// Error message indicating that file name is required.
    /// </summary>
    public static string FileNameRequired()
    {
        return "File name is required";
    }

    /// <summary>
    /// Error message indicating that the original file name is required.
    /// </summary>
    public static string OriginalFileNameRequired()
    {
        return "Original file name is required";
    }

    /// <summary>
    /// Error message indicating that MIME type is required.
    /// </summary>
    public static string MimeTypeRequired()
    {
        return "MIME type is required";
    }

    /// <summary>
    /// Error message indicating that storage URL is required.
    /// </summary>
    public static string StorageUrlRequired()
    {
        return "Storage URL is required";
    }

    /// <summary>
    /// Error message indicating that storage URL cannot be empty.
    /// </summary>
    public static string StorageUrlCannotBeEmpty()
    {
        return "Storage URL cannot be empty";
    }

    /// <summary>
    /// Error message indicating that file size must be greater than zero.
    /// </summary>
    public static string FileSizeMustBeGreaterThanZero()
    {
        return "File size must be greater than zero";
    }

    /// <summary>
    /// Gets the error message for invalid file URL format.
    /// </summary>
    /// <param name="fileUrl">The invalid file URL.</param>
    /// <returns>A formatted error message indicating the URL format is invalid.</returns>
    public static string InvalidFileUrl(string fileUrl)
    {
        return $"File URL '{fileUrl}' has an invalid format. Must be a valid HTTP or HTTPS URL";
    }

    /// <summary>
    /// Error message indicating that file URL is required.
    /// </summary>
    public static string FileUrlRequired()
    {
        return "File URL is required";
    }
}
