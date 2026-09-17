namespace _116.Core.Contracts.Application.DTOs;

/// <summary>
/// An opaque reference to a stored file: everything a consuming module may know about it.
/// Flat and immutable, so it is safe to cache.
/// </summary>
/// <param name="Id">The file's identifier.</param>
/// <param name="FileName">The stored filename.</param>
/// <param name="OriginalFileName">The filename as submitted.</param>
/// <param name="MimeType">The file's MIME type.</param>
/// <param name="StorageUrl">The URL the asset is served from.</param>
/// <param name="SizeInBytes">The stored size in bytes.</param>
/// <param name="StorageKey">The provider handle the asset is removed by.</param>
/// <param name="DominantColorHex">The image's dominant colour, when one was extracted.</param>
/// <param name="ForegroundColorHex">The image's foreground colour, when one was extracted.</param>
public record FileReferenceDto(
    Guid Id,
    string FileName,
    string OriginalFileName,
    string MimeType,
    string StorageUrl,
    long SizeInBytes,
    string? StorageKey = null,
    string? DominantColorHex = null,
    string? ForegroundColorHex = null
);
