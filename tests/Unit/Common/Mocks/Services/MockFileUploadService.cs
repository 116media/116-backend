using System.Runtime.CompilerServices;
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
    /// Creates a mock upload service with no behaviour configured.
    /// </summary>
    /// <returns>The mock instance.</returns>
    public static Mock<IFileUploadService> Create()
    {
        return new Mock<IFileUploadService>();
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.ReplaceImageFileAsync" /> to return the file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupReplaceImageFile(this Mock<IFileUploadService> mock, FileEntity file)
    {
        mock.Setup(x =>
                x.ReplaceImageFileAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.ReplaceVideoFileAsync" /> to return the file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupReplaceVideoFile(this Mock<IFileUploadService> mock, FileEntity file)
    {
        mock.Setup(x =>
                x.ReplaceVideoFileAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UpdateAvatarFromFileAsync" /> to return the file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUpdateAvatarFromFile(
        this Mock<IFileUploadService> mock,
        FileEntity file
    )
    {
        mock.Setup(x =>
                x.UpdateAvatarFromFileAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UpdateAvatarFromUrlAsync" /> to return the file, or
    /// null when the avatar needed no update.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return, or null.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUpdateAvatarFromUrl(
        this Mock<IFileUploadService> mock,
        FileEntity? file
    )
    {
        mock.Setup(x =>
                x.UpdateAvatarFromUrlAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);

        return mock;
    }

    /// <summary>
    /// Sets up <see cref="IFileUploadService.UploadAndStoreRawFileAsync" /> to return the file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileUploadService> SetupUploadAndStoreRawFile(
        this Mock<IFileUploadService> mock,
        FileEntity file
    )
    {
        mock.Setup(x =>
                x.UploadAndStoreRawFileAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);

        return mock;
    }

    /// <summary>
    /// Asserts an image replacement was requested exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceImageFileCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.ReplaceImageFileAsync(
                    It.IsAny<Guid?>(),
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
    /// Asserts no image replacement was requested.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceImageFileNotCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.ReplaceImageFileAsync(
                    It.IsAny<Guid?>(),
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
    /// Asserts a video replacement was requested exactly once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceVideoFileCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.ReplaceVideoFileAsync(
                    It.IsAny<Guid?>(),
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
    /// Asserts no video replacement was requested.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceVideoFileNotCalled(this Mock<IFileUploadService> mock)
    {
        mock.Verify(
            x =>
                x.ReplaceVideoFileAsync(
                    It.IsAny<Guid?>(),
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
}
