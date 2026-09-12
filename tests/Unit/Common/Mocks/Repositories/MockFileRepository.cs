using System.Runtime.CompilerServices;
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
    /// Files registered through <see cref="SetupGetById(Mock{IFileRepository}, FileEntity)" />,
    /// per mock instance, so the batch lookups serve the same arrangement as the single ones.
    /// </summary>
    private static readonly ConditionalWeakTable<Mock<IFileRepository>, Dictionary<Guid, FileEntity>> KnownFiles =
        new();

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
    /// Verifies the upload was claimed once the referencing row committed. An unclaimed upload
    /// is reaped, so a handler that forgets the claim loses the file a day later.
    /// </summary>
    /// <param name="mock">The mock instance.</param>
    /// <param name="fileId">The file expected to have been claimed.</param>
    public static void VerifyClaimed(this Mock<IFileRepository> mock, Guid fileId)
    {
        mock.Verify(x => x.ClaimAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);
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
        KnownFiles.GetOrCreateValue(mock)[file.Id] = file;
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

    /// <summary>
    /// Installs defaults for write, void and aggregate members only. Identity lookups are left
    /// unconfigured so that a miss has to be arranged by the test, naming the identifier it is a
    /// miss for, rather than being asserted for every identifier before the test says anything.
    /// </summary>
    /// <param name="mock">The repository mock to configure.</param>
    private static void SetupDefaults(Mock<IFileRepository> mock)
    {
        Dictionary<Guid, FileEntity> known = KnownFiles.GetOrCreateValue(mock);

        mock.Setup(x => x.ClaimAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        mock.Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                    (IReadOnlyDictionary<Guid, FileEntity>)
                        ids.Distinct().Where(known.ContainsKey).ToDictionary(id => id, id => known[id])
            );

        mock.Setup(x =>
                x.GetStorageUrlsByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(
                (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                    (IReadOnlyDictionary<Guid, string>)
                        ids.Distinct().Where(known.ContainsKey).ToDictionary(id => id, id => known[id].StorageUrl)
            );

        mock.Setup(x => x.AddAsync(It.IsAny<FileEntity>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }
}
