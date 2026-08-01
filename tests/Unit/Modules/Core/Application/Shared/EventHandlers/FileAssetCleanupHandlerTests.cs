using _116.Core.Application.Shared.EventHandlers;
using _116.Core.Application.Shared.Services;
using _116.Core.Domain.Events;
using _116.Unit.Tests.Common.Mocks.Services;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="FileAssetCleanupHandler"/>.
/// </summary>
public class FileAssetCleanupHandlerTests
{
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly FileAssetCleanupHandler _handler;

    public FileAssetCleanupHandlerTests()
    {
        _fileServiceMock = MockFileService.Create();
        _handler = new FileAssetCleanupHandler(_fileServiceMock.Object);
    }

    [Fact]
    public async Task Handle_FileReplacedWithStorageKey_ShouldDeleteOldRemoteAsset()
    {
        // Act
        await _handler.Handle(
            new FileReplacedEvent(FileId: Guid.NewGuid(), OldStorageKey: "avatars/user-1"),
            CancellationToken.None
        );

        // Assert
        _fileServiceMock.Verify(x => x.DeleteFileAsync("avatars/user-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FileReplacedWithoutStorageKey_ShouldSkipRemoteDeletion()
    {
        // Act
        await _handler.Handle(
            new FileReplacedEvent(FileId: Guid.NewGuid(), OldStorageKey: null),
            CancellationToken.None
        );

        // Assert
        _fileServiceMock.VerifyDeleteFileNotCalled();
    }

    [Fact]
    public async Task Handle_FileSoftDeletedWithStorageKey_ShouldDeleteRemoteAsset()
    {
        // Act
        await _handler.Handle(
            new FileSoftDeletedEvent(FileId: Guid.NewGuid(), StorageKey: "content/covers/cover-1"),
            CancellationToken.None
        );

        // Assert
        _fileServiceMock.Verify(
            x => x.DeleteFileAsync("content/covers/cover-1", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_FileSoftDeletedWithoutStorageKey_ShouldSkipRemoteDeletion()
    {
        // Act
        await _handler.Handle(
            new FileSoftDeletedEvent(FileId: Guid.NewGuid(), StorageKey: null),
            CancellationToken.None
        );

        // Assert
        _fileServiceMock.VerifyDeleteFileNotCalled();
    }
}
