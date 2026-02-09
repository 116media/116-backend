using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Helpers;

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
}
