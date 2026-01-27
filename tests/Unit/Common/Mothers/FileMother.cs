using _116.Core.Domain.Entities;
using _116.Unit.Tests.Common.Builders.Entities;
using _116.Unit.Tests.Common.Constants;

namespace _116.Unit.Tests.Common.Mothers;

/// <summary>
/// Provides predefined <see cref="FileEntity"/> scenarios for testing.
/// </summary>
public static class FileMother
{
    /// <summary>
    /// A JPEG profile avatar image.
    /// </summary>
    public static FileEntity ProfileAvatar() =>
        new FileBuilder()
            .AsJpegImage()
            .WithOriginalFileName("profile-avatar.jpg")
            .WithSizeInBytes(50 * 1024) // 50 KB
            .Build();

    /// <summary>
    /// A JPEG profile avatar image with a specific ID.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    public static FileEntity ProfileAvatar(Guid id) =>
        new FileBuilder()
            .WithId(id)
            .AsJpegImage()
            .WithOriginalFileName("profile-avatar.jpg")
            .WithSizeInBytes(50 * 1024) // 50 KB
            .Build();

    /// <summary>
    /// A PNG image file.
    /// </summary>
    public static FileEntity PngImage() =>
        new FileBuilder()
            .AsPngImage()
            .WithOriginalFileName("screenshot.png")
            .WithSizeInBytes(100 * 1024) // 100 KB
            .Build();

    /// <summary>
    /// A PDF document.
    /// </summary>
    public static FileEntity PdfDocument() =>
        new FileBuilder()
            .AsPdfDocument()
            .WithOriginalFileName("report.pdf")
            .WithSizeInBytes(500 * 1024) // 500 KB
            .Build();

    /// <summary>
    /// A large file (5 MB).
    /// </summary>
    public static FileEntity LargeFile() =>
        new FileBuilder()
            .AsJpegImage()
            .WithOriginalFileName("large-image.jpg")
            .WithSizeInBytes(5 * 1024 * 1024) // 5 MB
            .Build();

    /// <summary>
    /// A small file (1 KB).
    /// </summary>
    public static FileEntity SmallFile() =>
        new FileBuilder()
            .WithOriginalFileName("tiny.txt")
            .WithMimeType("text/plain")
            .WithSizeInBytes(1024) // 1 KB
            .Build();

    /// <summary>
    /// A deleted file.
    /// </summary>
    public static FileEntity Deleted() =>
        new FileBuilder().AsJpegImage().WithOriginalFileName("deleted-file.jpg").AsDeleted().Build();

    /// <summary>
    /// A deleted file with a specific ID.
    /// </summary>
    /// <param name="id">The file identifier.</param>
    public static FileEntity Deleted(Guid id) =>
        new FileBuilder().WithId(id).AsJpegImage().WithOriginalFileName("deleted-file.jpg").AsDeleted().Build();

    /// <summary>
    /// A file with valid test constants.
    /// </summary>
    public static FileEntity Valid() =>
        new FileBuilder()
            .WithFileName(TestConstants.File.ValidFileName)
            .WithOriginalFileName(TestConstants.File.ValidOriginalFileName)
            .WithMimeType(TestConstants.File.ValidMimeType)
            .WithStorageUrl(TestConstants.File.ValidStorageUrl)
            .WithSizeInBytes(TestConstants.File.ValidSizeInBytes)
            .Build();

    /// <summary>
    /// A random file for general testing.
    /// </summary>
    public static FileEntity Random() => new FileBuilder().Build();

    /// <summary>
    /// Common image MIME types.
    /// </summary>
    public static class MimeTypes
    {
        public const string Jpeg = "image/jpeg";
        public const string Png = "image/png";
        public const string Gif = "image/gif";
        public const string Pdf = "application/pdf";
        public const string Text = "text/plain";
    }
}
