using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Builds a mock <see cref="IFileUploadService" /> and the setup and verification helpers the
/// upload handlers need.
/// </summary>
public static class MockFileUploadService
{
    /// <summary>
    /// Creates a mock upload service that records whatever asset it is handed.
    /// </summary>
    /// <returns>The mock instance.</returns>
    public static Mock<IFileUploadService> Create()
    {
        Mock<IFileUploadService> mock = new();
        SetupDefaultRecord(mock);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadImageAsync" /> to return the asset.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadImage(this Mock<IFileUploadService> mock, FileEntity asset)
    {
        mock.Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(asset);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadVideoAsync" /> to return the asset.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadVideo(this Mock<IFileUploadService> mock, FileEntity asset)
    {
        mock.Setup(x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(asset);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadRawAsync" /> to return the asset.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadRaw(this Mock<IFileUploadService> mock, FileEntity asset)
    {
        mock.Setup(x =>
                x.UploadRawAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(asset);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadAvatarAsync" /> to return the asset.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadAvatar(this Mock<IFileUploadService> mock, FileEntity asset)
    {
        mock.Setup(x =>
                x.UploadAvatarAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(asset);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadAvatarFromUrlAsync" /> to return the asset, or
    /// null when the avatar needed no update.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset to return, or null.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadAvatarFromUrl(
        this Mock<IFileUploadService> mock,
        FileEntity? asset
    )
    {
        mock.Setup(x =>
                x.UploadAvatarFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(asset);

        return mock;
    }

    /// <summary>
    /// Asserts an image upload was requested exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadImageCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Asserts no image upload was requested.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadImageNotCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// Asserts a video upload was requested exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadVideoCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Asserts no video upload was requested.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadVideoNotCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.UploadVideoAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// Asserts the asset was recorded once, superseding the expected file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="asset">The asset expected to have been recorded.</param>
    /// <param name="supersededFileId">The file it was expected to supersede, or null when none.</param>
    public static void VerifyRecorded(
        this Mock<IFileUploadService> mock,
        FileEntity asset,
        Guid? supersededFileId = null
    )
    {
        mock.Verify(x => x.RecordAsync(asset, supersededFileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Asserts nothing was recorded.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyNothingRecorded(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x => x.RecordAsync(It.IsAny<FileEntity>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    /// <summary>
    /// Records whatever asset it is handed, so handlers get back the file they just uploaded.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaultRecord(Mock<IFileUploadService> mock)
    {
        mock.Setup(x => x.RecordAsync(It.IsAny<FileEntity>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileEntity file, Guid? _, CancellationToken _) => file);
    }
}
