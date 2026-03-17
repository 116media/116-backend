namespace _116.BuildingBlocks.Constants;

/// <summary>
/// Contains constants related to file entity business rules and constraints.
/// </summary>
public static class FileConstants
{
    /// <summary>
    /// Maximum allowed length for file names.
    /// </summary>
    public const int MaxFileNameLength = 255;

    /// <summary>
    /// Maximum allowed length for original file names.
    /// </summary>
    public const int MaxOriginalFileNameLength = 255;

    /// <summary>
    /// Maximum allowed length for MIME type strings.
    /// </summary>
    public const int MaxMimeTypeLength = 100;

    /// <summary>
    /// Maximum allowed length for storage URLs.
    /// </summary>
    public const int MaxStorageUrlLength = 2048;

    /// <summary>
    /// Default deletion status for new files.
    /// </summary>
    public const bool DefaultIsDeleted = false;

    /// <summary>
    /// Maximum file size for avatar uploads (1MB).
    /// </summary>
    public const long MaxAvatarFileSizeBytes = 1024 * 1024;

    /// <summary>
    /// Allowed image MIME types for avatar uploads.
    /// </summary>
    public static readonly string[] AllowedAvatarMimeTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
    ];

    /// <summary>
    /// Allowed file extensions for avatar uploads.
    /// </summary>
    public static readonly string[] AllowedAvatarExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    /// <summary>
    /// Maximum file size for raw file uploads, e.g. document attachments (5MB).
    /// </summary>
    public const long MaxRawFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Allowed MIME types for raw file uploads (images and PDF).
    /// </summary>
    public static readonly string[] AllowedRawFileMimeTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "application/pdf",
    ];

    /// <summary>
    /// Allowed file extensions for raw file uploads.
    /// </summary>
    public static readonly string[] AllowedRawFileExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"];
}
