using _116.Core.Contracts.Application.DTOs;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Core;

namespace _116.Tests.Fixtures.Factories.Core;

/// <summary>
/// Builds <see cref="FileReferenceDto" /> fixtures, the cross-module shape consuming modules see.
/// </summary>
public static class FileReferenceDtoFactory
{
    /// <summary>
    /// Creates a reference with default values.
    /// </summary>
    /// <returns>The reference.</returns>
    public static FileReferenceDto Create() => FileFactory.Create().ToFileReferenceDto();

    /// <summary>
    /// Creates a reference with the given identifier.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateWithId(Guid id) => FileFactory.CreateWithId(id).ToFileReferenceDto();

    /// <summary>
    /// Creates a JPEG reference.
    /// </summary>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateJpeg() => FileFactory.CreateJpeg().ToFileReferenceDto();

    /// <summary>
    /// Creates a PNG reference.
    /// </summary>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreatePng() => FileFactory.CreatePng().ToFileReferenceDto();

    /// <summary>
    /// Creates an image reference.
    /// </summary>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateImage() => FileFactory.CreateImage().ToFileReferenceDto();

    /// <summary>
    /// Creates a video reference.
    /// </summary>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateVideo() => FileFactory.CreateVideo().ToFileReferenceDto();

    /// <summary>
    /// Creates a reference served from the given URL.
    /// </summary>
    /// <param name="storageUrl">The storage URL.</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateWithStorageUrl(string storageUrl) =>
        FileFactory.CreateWithStorageUrl(storageUrl).ToFileReferenceDto();

    /// <summary>
    /// Creates a reference with the given storage key.
    /// </summary>
    /// <param name="storageKey">The provider handle.</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateWithStorageKey(string storageKey) =>
        FileFactory.CreateWithStorageKey(storageKey).ToFileReferenceDto();

    /// <summary>
    /// Creates a reference carrying the given colours.
    /// </summary>
    /// <param name="dominantColorHex">The dominant colour.</param>
    /// <param name="foregroundColorHex">The foreground colour.</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto CreateWithColors(string? dominantColorHex, string? foregroundColorHex) =>
        FileFactory.CreateWithColors(dominantColorHex, foregroundColorHex).ToFileReferenceDto();

    /// <summary>
    /// Projects a file fixture to its cross-module reference.
    /// </summary>
    /// <param name="file">The file fixture.</param>
    /// <returns>The reference.</returns>
    public static FileReferenceDto ToFileReferenceDto(this FileEntity file)
    {
        return new FileReferenceDto(
            Id: file.Id,
            FileName: file.FileName,
            OriginalFileName: file.OriginalFileName,
            MimeType: file.MimeType,
            StorageUrl: file.StorageUrl,
            SizeInBytes: file.SizeInBytes,
            StorageKey: file.StorageKey,
            DominantColorHex: file.DominantColorHex,
            ForegroundColorHex: file.ForegroundColorHex
        );
    }
}
