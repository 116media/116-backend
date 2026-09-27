namespace _116.Storage.Contracts.Domain.Constants;

/// <summary>
/// Upload limits other modules validate against before handing a file to Storage: the avatar rules
/// Identity enforces and the video rules Content enforces.
/// </summary>
public static class FileUploadLimits
{
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
    /// Maximum file size for video uploads (350 MB).
    /// </summary>
    public const long MaxVideoFileSizeBytes = 350L * 1024 * 1024;

    /// <summary>
    /// Allowed file extensions for video uploads.
    /// </summary>
    public static readonly string[] AllowedVideoExtensions = [".mp4", ".mov", ".webm", ".avi", ".mkv", ".3gp"];
}
