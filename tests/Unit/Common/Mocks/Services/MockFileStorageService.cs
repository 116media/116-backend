using System.Runtime.CompilerServices;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Tests.Fixtures.Factories.Core;
using Microsoft.AspNetCore.Http;
using Moq;

namespace _116.Unit.Tests.Common.Mocks.Services;

/// <summary>
/// Mock for <see cref="IFileStorageService" />, Core's cross-module storage contract.
/// </summary>
public static class MockFileStorageService
{
    /// <summary>
    /// Files registered through <see cref="SetupResolve" />, so batch resolution answers with
    /// everything a test has staged.
    /// </summary>
    private static readonly ConditionalWeakTable<
        Mock<IFileStorageService>,
        Dictionary<Guid, FileReferenceDto>
    > KnownFiles = new();

    /// <summary>
    /// Creates a store mock that resolves nothing and records anything.
    /// </summary>
    /// <returns>The configured mock.</returns>
    public static Mock<IFileStorageService> Create()
    {
        Mock<IFileStorageService> mock = new();
        SetupDefaults(mock);
        return mock;
    }

    /// <summary>
    /// Makes the store resolve this reference by its id.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="file">The reference to resolve.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupResolve(this Mock<IFileStorageService> mock, FileReferenceDto file)
    {
        mock.Setup(x => x.ResolveAsync(file.Id, It.IsAny<CancellationToken>())).ReturnsAsync(file);
        KnownFiles.GetOrCreateValue(mock)[file.Id] = file;
        SetupBatchResolution(mock);
        SetupUrlResolution(mock);
        return mock;
    }

    /// <summary>
    /// Makes the store resolve nothing for this id.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="fileId">The id that resolves to nothing.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupResolveReturnsNull(this Mock<IFileStorageService> mock, Guid fileId)
    {
        mock.Setup(x => x.ResolveAsync(fileId, It.IsAny<CancellationToken>())).ReturnsAsync((FileReferenceDto?)null);
        return mock;
    }

    /// <summary>
    /// Makes an upload return this handle.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="stored">The handle uploads return.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupUpload(this Mock<IFileStorageService> mock, StoredFile stored)
    {
        mock.Setup(x =>
                x.UploadAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<EnumStoredFileKind>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(stored);

        mock.Setup(x => x.RecordAsync(It.IsAny<StoredFile>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored.Reference);

        return mock;
    }

    /// <summary>
    /// Makes a URL-sourced upload return this handle.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="stored">The handle the fetch returns, or null for no update.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupUploadFromUrl(this Mock<IFileStorageService> mock, StoredFile? stored)
    {
        mock.Setup(x => x.UploadFromUrlAsync(It.IsAny<Guid?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        if (stored is not null)
        {
            mock.Setup(x => x.RecordAsync(It.IsAny<StoredFile>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(stored.Reference);
        }

        return mock;
    }

    /// <summary>
    /// Makes batch resolution answer with these references.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="files">The references to resolve.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupResolveMany(
        this Mock<IFileStorageService> mock,
        params FileReferenceDto[] files
    )
    {
        foreach (FileReferenceDto file in files)
        {
            mock.SetupResolve(file);
        }

        return mock;
    }

    /// <summary>
    /// Makes batch resolution answer with exactly this map.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="files">The references to resolve, keyed by file id.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupResolveMany(
        this Mock<IFileStorageService> mock,
        IReadOnlyDictionary<Guid, FileReferenceDto> files
    )
    {
        foreach (FileReferenceDto file in files.Values)
        {
            mock.SetupResolve(file);
        }

        mock.Setup(x => x.ResolveManyAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(files);

        return mock;
    }

    /// <summary>
    /// Makes the store report this file as deleted.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="fileId">The file that deletes successfully.</param>
    /// <returns>The same mock, for chaining.</returns>
    public static Mock<IFileStorageService> SetupDelete(this Mock<IFileStorageService> mock, Guid fileId)
    {
        mock.Setup(x => x.DeleteAsync(fileId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return mock;
    }

    /// <summary>
    /// Asserts a deletion ran.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    public static void VerifyDeleteCalled(this Mock<IFileStorageService> mock)
    {
        mock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    /// <summary>
    /// Asserts a deletion ran the given number of times.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    /// <param name="times">The expected call count.</param>
    public static void VerifyDeleteCalled(this Mock<IFileStorageService> mock, Times times)
    {
        mock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), times);
    }

    /// <summary>
    /// Asserts no deletion ran.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    public static void VerifyDeleteNotCalled(this Mock<IFileStorageService> mock)
    {
        mock.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Asserts batch resolution ran exactly once.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    public static void VerifyResolveManyCalledOnce(this Mock<IFileStorageService> mock)
    {
        mock.Verify(
            x => x.ResolveManyAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    /// <summary>
    /// Sets the resolving, recording and deleting defaults.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    private static void SetupDefaults(Mock<IFileStorageService> mock)
    {
        mock.Setup(x => x.ResolveAsync(It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FileReferenceDto?)null);
        mock.Setup(x => x.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, string>());
        mock.Setup(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mock.Setup(x =>
                x.DeleteAssetsAsync(
                    It.IsAny<IEnumerable<string>>(),
                    It.IsAny<EnumStoredFileKind>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(true);

        SetupBatchResolution(mock);
        SetupUrlResolution(mock);
    }

    /// <summary>
    /// Answers url resolution from everything registered on this mock.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    private static void SetupUrlResolution(Mock<IFileStorageService> mock)
    {
        mock.Setup(x => x.ResolveUrlsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                {
                    Dictionary<Guid, FileReferenceDto> known = KnownFiles.GetOrCreateValue(mock);
                    return ids.Where(known.ContainsKey).ToDictionary(id => id, id => known[id].StorageUrl);
                }
            );
    }

    /// <summary>
    /// Answers batch resolution from everything registered on this mock.
    /// </summary>
    /// <param name="mock">The store mock.</param>
    private static void SetupBatchResolution(Mock<IFileStorageService> mock)
    {
        mock.Setup(x => x.ResolveManyAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                (IReadOnlyCollection<Guid> ids, CancellationToken _) =>
                {
                    Dictionary<Guid, FileReferenceDto> known = KnownFiles.GetOrCreateValue(mock);
                    return ids.Where(known.ContainsKey).ToDictionary(id => id, id => known[id]);
                }
            );
    }
}
