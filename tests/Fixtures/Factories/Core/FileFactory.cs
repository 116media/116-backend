using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Core;
using _116.Tests.Fixtures.Constants;

namespace _116.Tests.Fixtures.Factories.Core;

/// <summary>
/// Named aliases for <see cref="FileBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class FileFactory
{
    /// <summary>
    /// Creates a file with default random values.
    /// </summary>
    /// <returns>A new FileEntity with random values.</returns>
    public static FileEntity Create() => new FileBuilder().Build();

    /// <summary>
    /// Creates a file with a specific original filename.
    /// </summary>
    /// <param name="originalFileName">The original filename.</param>
    /// <returns>A new FileEntity with the specified filename.</returns>
    public static FileEntity Create(string originalFileName) =>
        new FileBuilder().WithOriginalFileName(originalFileName).Build();

    /// <summary>
    /// Creates a file with a specific ID.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <returns>A new FileEntity with the specified ID.</returns>
    public static FileEntity CreateWithId(Guid id) => new FileBuilder().WithId(id).Build();

    /// <summary>
    /// Creates a JPEG image file.
    /// </summary>
    /// <returns>A new JPEG FileEntity.</returns>
    public static FileEntity CreateJpeg() => new FileBuilder().AsJpegImage().Build();

    /// <summary>
    /// Creates a PNG image file.
    /// </summary>
    /// <returns>A new PNG FileEntity.</returns>
    public static FileEntity CreatePng() => new FileBuilder().AsPngImage().Build();

    /// <summary>
    /// Creates a PDF document file.
    /// </summary>
    /// <returns>A new PDF FileEntity.</returns>
    public static FileEntity CreatePdf() => new FileBuilder().AsPdfDocument().Build();

    /// <summary>
    /// Creates a deleted file.
    /// </summary>
    /// <returns>A new deleted FileEntity.</returns>
    public static FileEntity CreateDeleted() => new FileBuilder().AsDeleted().Build();

    /// <summary>
    /// Creates a file with a specific size.
    /// </summary>
    /// <param name="sizeInBytes">The file size in bytes.</param>
    /// <returns>A new FileEntity with the specified size.</returns>
    public static FileEntity CreateWithSize(long sizeInBytes) => new FileBuilder().WithSizeInBytes(sizeInBytes).Build();

    /// <summary>
    /// Creates a file with known test values.
    /// </summary>
    /// <returns>A new FileEntity with test constants.</returns>
    public static FileEntity CreateWithTestValues() =>
        new FileBuilder()
            .WithFileName(TestConstants.File.ValidFileName)
            .WithOriginalFileName(TestConstants.File.ValidOriginalFileName)
            .WithMimeType(TestConstants.File.ValidMimeType)
            .WithStorageUrl(TestConstants.File.ValidStorageUrl)
            .WithSizeInBytes(TestConstants.File.ValidSizeInBytes)
            .Build();

    /// <summary>
    /// Creates a file with a specific stored filename.
    /// </summary>
    /// <param name="fileName">The stored filename.</param>
    /// <returns>A new FileEntity with the specified filename.</returns>
    public static FileEntity CreateWithFileName(string fileName) => new FileBuilder().WithFileName(fileName).Build();

    /// <summary>
    /// Creates a file with a specific MIME type.
    /// </summary>
    /// <param name="mimeType">The MIME type.</param>
    /// <returns>A new FileEntity with the specified MIME type.</returns>
    public static FileEntity CreateWithMimeType(string mimeType) => new FileBuilder().WithMimeType(mimeType).Build();

    /// <summary>
    /// Creates a deleted file with a specific ID.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    /// <returns>A new deleted FileEntity with the specified ID.</returns>
    public static FileEntity CreateDeletedWithId(Guid id) => new FileBuilder().WithId(id).AsDeleted().Build();

    /// <summary>
    /// Creates a file with a specific storage URL.
    /// </summary>
    /// <param name="storageUrl">The storage URL.</param>
    /// <returns>A new FileEntity with the specified storage URL.</returns>
    public static FileEntity CreateWithStorageUrl(string storageUrl) =>
        new FileBuilder().WithStorageUrl(storageUrl).Build();

    /// <summary>
    /// Creates a JPEG image file with a storage key.
    /// </summary>
    /// <returns>A new JPEG FileEntity with a storage key.</returns>
    public static FileEntity CreateImage() =>
        new FileBuilder().AsJpegImage().WithStorageKey(TestConstants.File.ValidStorageKey).Build();

    /// <summary>
    /// Creates a video file with a storage key.
    /// </summary>
    /// <returns>A new FileEntity configured as a video with a storage key.</returns>
    public static FileEntity CreateVideo() =>
        new FileBuilder().WithMimeType("video/mp4").WithStorageKey(TestConstants.File.ValidVideoStorageKey).Build();

    /// <summary>
    /// Creates a file with a specific storage key.
    /// </summary>
    /// <param name="storageKey">The storage key.</param>
    /// <returns>A new FileEntity with the specified storage key.</returns>
    public static FileEntity CreateWithStorageKey(string storageKey) =>
        new FileBuilder().WithStorageKey(storageKey).Build();

    /// <summary>
    /// Creates a file with the specified extracted dominant and foreground colors.
    /// </summary>
    /// <param name="dominantColorHex">The dominant color as <c>#RRGGBB</c>, or null.</param>
    /// <param name="foregroundColorHex">The contrasting foreground color as <c>#RRGGBB</c>, or null.</param>
    /// <returns>A new FileEntity carrying the specified colors.</returns>
    public static FileEntity CreateWithColors(string? dominantColorHex, string? foregroundColorHex) =>
        new FileBuilder().WithColors(dominantColorHex, foregroundColorHex).Build();
}
