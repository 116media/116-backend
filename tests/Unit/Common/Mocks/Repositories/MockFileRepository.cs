using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Repositories;

/// <summary>
/// Provides mock setup helpers for <see cref="IFileRepository"/>.
/// </summary>
public static class MockFileRepository
{
    /// <summary>
    /// Creates a new mock instance of IFileRepository.
    /// </summary>
    /// <returns>A configured Mock of IFileRepository.</returns>
    public static Mock<IFileRepository> Create()
    {
        Mock<IFileRepository> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Sets up GetByIdAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupGetById(this Mock<IFileRepository> mock, FileEntity file)
    {
        mock.Setup(x => x.GetByIdAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);
        return mock;
    }

    /// <summary>
    /// Sets up GetByIdAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="fileId">The file ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupGetByIdReturnsNull(this Mock<IFileRepository> mock, Guid fileId)
    {
        mock.Setup(x => x.GetByIdAsync(fileId, It.IsAny<CancellationToken>())).ReturnsAsync((FileEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up GetAvatarFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="avatarFileId">The avatar file ID.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupGetAvatarFile(
        this Mock<IFileRepository> mock,
        Guid? avatarFileId,
        FileEntity file
    )
    {
        mock.Setup(x => x.GetAvatarFileAsync(avatarFileId, It.IsAny<CancellationToken>())).ReturnsAsync(file);
        return mock;
    }

    /// <summary>
    /// Sets up GetAvatarFileAsync to return null.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="avatarFileId">The avatar file ID.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupGetAvatarFileReturnsNull(
        this Mock<IFileRepository> mock,
        Guid? avatarFileId
    )
    {
        mock.Setup(x => x.GetAvatarFileAsync(avatarFileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileEntity?)null);
        return mock;
    }

    /// <summary>
    /// Sets up UploadAndStoreAvatarAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUploadAndStoreAvatar(this Mock<IFileRepository> mock, FileEntity file)
    {
        mock.Setup(x =>
                x.UploadAndStoreAvatarAsync(
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
    /// Sets up DownloadAndStoreAvatarFromUrlAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupDownloadAndStoreAvatarFromUrl(
        this Mock<IFileRepository> mock,
        FileEntity file
    )
    {
        mock.Setup(x =>
                x.DownloadAndStoreAvatarFromUrlAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);
        return mock;
    }

    /// <summary>
    /// Sets up UpdateAvatarFromUrlAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUpdateAvatarFromUrl(this Mock<IFileRepository> mock, FileEntity? file)
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
    /// Sets up UpdateAvatarFromFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUpdateAvatarFromFile(this Mock<IFileRepository> mock, FileEntity file)
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
    /// Sets up UpdateAvatarUrlFromSourceAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return (or null).</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUpdateAvatarUrlFromSource(
        this Mock<IFileRepository> mock,
        FileEntity? file
    )
    {
        mock.Setup(x =>
                x.UpdateAvatarUrlFromSourceAsync(
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(file);
        return mock;
    }

    /// <summary>
    /// Sets up UploadAndStoreImageFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUploadAndStoreImageFile(this Mock<IFileRepository> mock, FileEntity file)
    {
        mock.Setup(x =>
                x.UploadAndStoreImageFileAsync(
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
    /// Sets up UploadAndStoreVideoFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUploadAndStoreVideoFile(this Mock<IFileRepository> mock, FileEntity file)
    {
        mock.Setup(x =>
                x.UploadAndStoreVideoFileAsync(
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
    /// Sets up ReplaceImageFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupReplaceImageFile(this Mock<IFileRepository> mock, FileEntity file)
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
    /// Sets up ReplaceVideoFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupReplaceVideoFile(this Mock<IFileRepository> mock, FileEntity file)
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
    /// Sets up SoftDeleteByIdAsync to return the specified result.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="result">The result to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupSoftDeleteById(this Mock<IFileRepository> mock, bool result = true)
    {
        mock.Setup(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        return mock;
    }

    /// <summary>
    /// Sets up UploadAndStoreRawFileAsync to return the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupUploadAndStoreRawFile(this Mock<IFileRepository> mock, FileEntity file)
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
    /// Verifies that UploadAndStoreImageFileAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadAndStoreImageFileCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(
            x =>
                x.UploadAndStoreImageFileAsync(
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
    /// Verifies that UploadAndStoreVideoFileAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadAndStoreVideoFileCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(
            x =>
                x.UploadAndStoreVideoFileAsync(
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
    /// Verifies that UploadAndStoreVideoFileAsync was not called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyUploadAndStoreVideoFileNotCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(
            x =>
                x.UploadAndStoreVideoFileAsync(
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
    /// Verifies that ReplaceImageFileAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceImageFileCalled(this Mock<IFileRepository> mock)
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
    /// Verifies that ReplaceImageFileAsync was not called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceImageFileNotCalled(this Mock<IFileRepository> mock)
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
    /// Verifies that ReplaceVideoFileAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyReplaceVideoFileCalled(this Mock<IFileRepository> mock)
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
    /// Verifies that SoftDeleteByIdAsync was called with a specific file ID.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="fileId">The expected file ID.</param>
    public static void VerifySoftDeleteByIdCalled(this Mock<IFileRepository> mock, Guid fileId)
    {
        mock.Verify(x => x.SoftDeleteByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that SoftDeleteByIdAsync was called at least once.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifySoftDeleteByIdCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that SoftDeleteByIdAsync was not called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifySoftDeleteByIdNotCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifies that AddAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyFile">Optional predicate to verify the file.</param>
    public static void VerifyAddCalled(this Mock<IFileRepository> mock, Func<FileEntity, bool>? verifyFile = null)
    {
        if (verifyFile is not null)
        {
            mock.Verify(
                x => x.AddAsync(It.Is<FileEntity>(f => verifyFile(f)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.AddAsync(It.IsAny<FileEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that UpdateAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="verifyFile">Optional predicate to verify the file.</param>
    public static void VerifyUpdateCalled(this Mock<IFileRepository> mock, Func<FileEntity, bool>? verifyFile = null)
    {
        if (verifyFile is not null)
        {
            mock.Verify(
                x => x.UpdateAsync(It.Is<FileEntity>(f => verifyFile(f)), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
        else
        {
            mock.Verify(x => x.UpdateAsync(It.IsAny<FileEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    /// <summary>
    /// Verifies that Remove was called with the specified file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="file">The file that should have been removed.</param>
    public static void VerifyRemoveCalled(this Mock<IFileRepository> mock, FileEntity file)
    {
        mock.Verify(x => x.Remove(file), Times.Once);
    }

    /// <summary>
    /// Verifies that Remove was called with any file.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyRemoveCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(x => x.Remove(It.IsAny<FileEntity>()), Times.Once);
    }

    /// <summary>
    /// Verifies that SaveChangesAsync was called.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifySaveChangesCalled(this Mock<IFileRepository> mock)
    {
        mock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Sets up default behaviors for the mock.
    /// </summary>
    /// <summary>
    /// Sets up GetByIdsAsync to return the specified file map.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="files">The file map keyed by id to return.</param>
    /// <returns>The mock instance for chaining.</returns>
    public static Mock<IFileRepository> SetupGetByIds(
        this Mock<IFileRepository> mock,
        IReadOnlyDictionary<Guid, FileEntity> files
    )
    {
        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);
        return mock;
    }

    /// <summary>
    /// Verifies GetByIdsAsync was invoked exactly once (asserts batching, not per-item N+1).
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    public static void VerifyGetByIdsCalledOnce(this Mock<IFileRepository> mock)
    {
        mock.Verify(
            x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    private static void SetupDefaults(Mock<IFileRepository> mock)
    {
        mock.Setup(x => x.AddAsync(It.IsAny<FileEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mock.Setup(x => x.UpdateAsync(It.IsAny<FileEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, FileEntity>());
    }
}
