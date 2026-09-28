using _116.Storage.Application.Shared.Cache;
using _116.Storage.Application.Shared.EventHandlers;
using _116.Storage.Contracts.Domain.Enums;
using _116.Storage.Domain.Events;
using _116.Tests.TestData.Mocks;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using Xunit;

namespace _116.Storage.Unit.Tests.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="FileCacheHandler"/>. A file whose row is gone must stop resolving
/// from cache, or consumers keep serving the URL of a deleted asset until the entry expires.
/// </summary>
public class FileCacheHandlerTests
{
    private readonly Mock<HybridCache> _cacheMock = MockHybridCache.Create();
    private readonly FileCacheHandler _handler;

    public FileCacheHandlerTests()
    {
        _handler = new FileCacheHandler(_cacheMock.Object);
    }

    [Fact]
    public async Task Handle_FileSoftDeleted_ShouldEvictTheFileProjections()
    {
        var domainEvent = new FileSoftDeletedEvent(
            FileId: Guid.NewGuid(),
            StorageKey: "core/files/deleted",
            Kind: EnumStoredFileKind.Image
        );

        await _handler.Handle(domainEvent, CancellationToken.None);

        _cacheMock.VerifyRemovedByTag(StorageCacheTags.Files);
    }

    [Fact]
    public async Task Handle_FileReplaced_ShouldEvictTheFileProjections()
    {
        var domainEvent = new FileReplacedEvent(
            FileId: Guid.NewGuid(),
            OldStorageKey: "core/files/superseded",
            Kind: EnumStoredFileKind.Video
        );

        await _handler.Handle(domainEvent, CancellationToken.None);

        _cacheMock.VerifyRemovedByTag(StorageCacheTags.Files);
    }

    [Fact]
    public async Task Handle_FileSoftDeletedWithoutAStorageKey_ShouldStillEvict()
    {
        // An external-URL row has nothing to purge remotely, but its projection is still cached.
        var domainEvent = new FileSoftDeletedEvent(
            StorageKey: null,
            FileId: Guid.NewGuid(),
            Kind: EnumStoredFileKind.Image
        );

        await _handler.Handle(domainEvent, CancellationToken.None);

        _cacheMock.VerifyRemovedByTag(StorageCacheTags.Files);
    }
}
