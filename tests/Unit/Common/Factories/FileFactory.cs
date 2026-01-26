using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Factories;

/// <summary>
/// Factory for quickly creating <see cref="FileEntity"/> instances in tests.
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
    /// Creates a list of files with the specified count.
    /// </summary>
    /// <param name="count">The number of files to create.</param>
    /// <returns>A list of FileEntity instances.</returns>
    public static List<FileEntity> CreateMany(int count) => Enumerable.Range(0, count).Select(_ => Create()).ToList();

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
}
