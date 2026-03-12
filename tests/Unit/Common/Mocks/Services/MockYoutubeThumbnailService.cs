using _116.Content.Application.Editorial.Services;
using _116.Tests.Fixtures.Constants;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="IYoutubeThumbnailService"/>.
/// </summary>
public static class MockYoutubeThumbnailService
{
    /// <summary>
    /// Creates a new mock instance of IYoutubeThumbnailService with safe default setups.
    /// </summary>
    public static Mock<IYoutubeThumbnailService> Create()
    {
        Mock<IYoutubeThumbnailService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<IYoutubeThumbnailService> SetupDownload(
        this Mock<IYoutubeThumbnailService> mock,
        IFormFile? formFile = null
    )
    {
        IFormFile file = formFile ?? CreateMockFormFile();
        mock.Setup(x => x.DownloadThumbnailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(file);
        return mock;
    }

    public static void VerifyDownloadCalled(this Mock<IYoutubeThumbnailService> mock, string youtubeVideoId)
    {
        mock.Verify(x => x.DownloadThumbnailAsync(youtubeVideoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyDownloadNotCalled(this Mock<IYoutubeThumbnailService> mock)
    {
        mock.Verify(x => x.DownloadThumbnailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static IFormFile CreateMockFormFile()
    {
        Mock<IFormFile> fileMock = new();
        fileMock.Setup(x => x.FileName).Returns("thumbnail.jpg");
        fileMock.Setup(x => x.ContentType).Returns("image/jpeg");
        fileMock.Setup(x => x.Length).Returns(TestConstants.Content.Editorial.Cloudinary.ValidBytes);
        return fileMock.Object;
    }

    private static void SetupDefaults(Mock<IYoutubeThumbnailService> mock)
    {
        mock.Setup(x => x.DownloadThumbnailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateMockFormFile());
    }
}
