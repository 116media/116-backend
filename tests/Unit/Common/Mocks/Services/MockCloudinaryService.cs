using _116.Core.Application.Shared.Services;
using _116.Tests.Fixtures.Constants;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Provides mock setup helpers for <see cref="ICloudinaryService"/>.
/// </summary>
public static class MockCloudinaryService
{
    /// <summary>
    /// Creates a new mock instance of ICloudinaryService with safe default setups.
    /// </summary>
    public static Mock<ICloudinaryService> Create()
    {
        Mock<ICloudinaryService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    public static Mock<ICloudinaryService> SetupUploadImage(
        this Mock<ICloudinaryService> mock,
        CloudinaryUploadResult? result = null
    )
    {
        CloudinaryUploadResult uploadResult = result ?? DefaultUploadResult();
        mock.Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploadResult);
        return mock;
    }

    public static void VerifyUploadCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(
            x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    public static Mock<ICloudinaryService> SetupUploadVideo(
        this Mock<ICloudinaryService> mock,
        CloudinaryUploadResult? result = null
    )
    {
        CloudinaryUploadResult uploadResult = result ?? DefaultVideoUploadResult();
        mock.Setup(x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploadResult);
        return mock;
    }

    public static void VerifyVideoUploadCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(
            x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    public static void VerifyVideoUploadNotCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(
            x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    public static CloudinaryUploadResult DefaultVideoUploadResult() =>
        new(
            PublicId: TestConstants.Cloudinary.ValidPublicId,
            SecureUrl: TestConstants.Cloudinary.ValidSecureUrl,
            Format: "mp4",
            Width: 1080,
            Height: 1920,
            Bytes: 5_000_000,
            ResourceType: "video"
        );

    public static void VerifyDeleteImageCalled(this Mock<ICloudinaryService> mock, string storageKey)
    {
        mock.Verify(x => x.DeleteImageAsync(storageKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    public static void VerifyDeleteImageNotCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static void VerifyDeleteImagesCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(
            x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    public static void VerifyDeleteImagesNotCalled(this Mock<ICloudinaryService> mock)
    {
        mock.Verify(
            x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    public static CloudinaryUploadResult DefaultUploadResult() =>
        new(
            PublicId: TestConstants.Cloudinary.ValidPublicId,
            SecureUrl: TestConstants.Cloudinary.ValidSecureUrl,
            Format: TestConstants.Cloudinary.ValidFormat,
            Width: TestConstants.Cloudinary.ValidWidth,
            Height: TestConstants.Cloudinary.ValidHeight,
            Bytes: TestConstants.Cloudinary.ValidBytes,
            ResourceType: TestConstants.Cloudinary.ValidResourceType
        );

    private static void SetupDefaults(Mock<ICloudinaryService> mock)
    {
        mock.Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(DefaultUploadResult());
        mock.Setup(x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(DefaultVideoUploadResult());
        mock.Setup(x => x.DeleteImageAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mock.Setup(x => x.DeleteImagesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }
}
