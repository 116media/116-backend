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
    /// Maximum allowed length for cloud storage keys (e.g., Cloudinary public IDs).
    /// </summary>
    public const int MaxStorageKeyLength = 100;

    /// <summary>
    /// Length of a stored color hex string in the canonical <c>#RRGGBB</c> form.
    /// </summary>
    public const int ColorHexLength = 7;

    /// <summary>
    /// Default deletion status for new files.
    /// </summary>
    public const bool DefaultIsDeleted = false;

    /// <summary>
    /// Maximum file size for avatar uploads (2MB).
    /// </summary>
    public const long MaxAvatarFileSizeBytes = 2 * 1024 * 1024;

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

    /// <summary>
    /// Maximum file size for video uploads (350 MB).
    /// </summary>
    public const long MaxVideoFileSizeBytes = 350L * 1024 * 1024;

    /// <summary>
    /// Allowed MIME types for video uploads.
    /// </summary>
    public static readonly string[] AllowedVideoMimeTypes =
    [
        "video/mp4",
        "video/quicktime",
        "video/webm",
        "video/x-msvideo",
        "video/x-matroska",
        "video/3gpp",
    ];

    /// <summary>
    /// Allowed file extensions for video uploads.
    /// </summary>
    public static readonly string[] AllowedVideoExtensions = [".mp4", ".mov", ".webm", ".avi", ".mkv", ".3gp"];
}
