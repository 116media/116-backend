using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Shared test helpers for file-related tests.
/// For creating FileEntity instances, use FileFactory instead.
/// </summary>
public static class FileTestHelpers
{
    /// <summary>
    /// Creates a mock IFormFile for testing file uploads.
    /// </summary>
    public static IFormFile CreateMockFormFile()
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        return fileMock.Object;
    }

    /// <summary>
    /// Creates a mock IFormFile representing a valid video file for testing.
    /// </summary>
    public static IFormFile CreateMockVideoFile()
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns("clip.mp4");
        fileMock.Setup(f => f.Length).Returns(5_000_000);
        fileMock.Setup(f => f.ContentType).Returns("video/mp4");
        return fileMock.Object;
    }

    /// <summary>
    /// Creates a mock IFormFile with custom parameters for testing file validation.
    /// </summary>
    public static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        return fileMock.Object;
    }

    /// <summary>
    /// Creates a real IFormFile backed by the given bytes so that
    /// <see cref="IFormFile.OpenReadStream"/> returns the actual content — needed
    /// for code paths that read the uploaded stream (e.g. image color extraction).
    /// </summary>
    /// <param name="content">The raw file bytes served by the form file.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The MIME content type.</param>
    /// <returns>A backed <see cref="FormFile"/> instance.</returns>
    public static IFormFile CreateFormFileWithContent(byte[] content, string fileName, string contentType)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}
