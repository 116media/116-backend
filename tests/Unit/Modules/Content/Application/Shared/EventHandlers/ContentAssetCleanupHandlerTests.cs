using _116.Content.Application.Shared.EventHandlers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Shared.EventHandlers;

/// <summary>
/// Unit tests for <see cref="ContentAssetCleanupHandler"/>.
/// </summary>
public class ContentAssetCleanupHandlerTests
{
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentAssetCleanupHandler _handler;

    public ContentAssetCleanupHandlerTests()
    {
        _cloudinaryMock = MockCloudinaryService.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _articleRepositoryMock = MockArticleRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock.SetupSoftDeleteById();

        _handler = new ContentAssetCleanupHandler(
            _cloudinaryMock.Object,
            _fileRepositoryMock.Object,
            _articleRepositoryMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region ArticleDeletedEvent

    [Fact]
    public async Task Handle_ArticleDeleted_ShouldSoftDeleteCoverAndDeleteBodyImages()
    {
        // Arrange
        Guid coverFileId = Guid.NewGuid();
        List<string> storageKeys = ["content/articles/image-0", "content/articles/image-1"];
        var domainEvent = new ArticleDeletedEvent(
            ArticleId: Guid.NewGuid(),
            CoverFileId: coverFileId,
            BodyImageStorageKeys: storageKeys
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdCalled(coverFileId);
        _cloudinaryMock.Verify(
            x =>
                x.DeleteImagesAsync(
                    It.Is<IEnumerable<string>>(keys => keys.SequenceEqual(storageKeys)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ArticleDeletedWithoutCoverOrImages_ShouldTouchNothing()
    {
        // Arrange
        var domainEvent = new ArticleDeletedEvent(
            ArticleId: Guid.NewGuid(),
            CoverFileId: null,
            BodyImageStorageKeys: []
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdNotCalled();
        _cloudinaryMock.VerifyDeleteImagesNotCalled();
    }

    #endregion

    #region VideoDeletedEvent

    [Fact]
    public async Task Handle_VideoDeletedWithThumbnail_ShouldSoftDeleteThumbnailRow()
    {
        // Arrange
        Guid thumbnailFileId = Guid.NewGuid();
        var domainEvent = new VideoDeletedEvent(VideoId: Guid.NewGuid(), ThumbnailFileId: thumbnailFileId);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdCalled(thumbnailFileId);
    }

    [Fact]
    public async Task Handle_VideoDeletedWithoutThumbnail_ShouldTouchNothing()
    {
        // Act
        await _handler.Handle(
            new VideoDeletedEvent(VideoId: Guid.NewGuid(), ThumbnailFileId: null),
            CancellationToken.None
        );

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdNotCalled();
    }

    #endregion

    #region ShortVideoDeletedEvent

    [Fact]
    public async Task Handle_ShortVideoDeleted_ShouldSoftDeleteBothFileRows()
    {
        // Arrange
        Guid videoFileId = Guid.NewGuid();
        Guid thumbnailFileId = Guid.NewGuid();
        var domainEvent = new ShortVideoDeletedEvent(
            ShortVideoId: Guid.NewGuid(),
            VideoFileId: videoFileId,
            ThumbnailFileId: thumbnailFileId
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdCalled(videoFileId);
        _fileRepositoryMock.VerifySoftDeleteByIdCalled(thumbnailFileId);
    }

    [Fact]
    public async Task Handle_ShortVideoDeletedWithoutFiles_ShouldTouchNothing()
    {
        // Act
        await _handler.Handle(
            new ShortVideoDeletedEvent(ShortVideoId: Guid.NewGuid(), VideoFileId: null, ThumbnailFileId: null),
            CancellationToken.None
        );

        // Assert
        _fileRepositoryMock.VerifySoftDeleteByIdNotCalled();
    }

    #endregion

    #region ArticleBodyImagesOrphanedEvent

    [Fact]
    public async Task Handle_BodyImagesOrphaned_ShouldRemoveMatchingRowsThenDeleteAssets()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        List<ArticleImageEntity> images = ArticleImageFactory.CreateMany(articleId, 3);
        List<string> orphanedKeys = images.Take(2).Select(img => img.StorageKey).ToList();

        _articleRepositoryMock.SetupGetImagesByArticleId(articleId, images);

        var domainEvent = new ArticleBodyImagesOrphanedEvent(ArticleId: articleId, StorageKeys: orphanedKeys);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert — only the rows matching the captured keys are removed, then the assets purged.
        _articleRepositoryMock.Verify(
            x =>
                x.RemoveImages(
                    It.Is<IEnumerable<ArticleImageEntity>>(rows =>
                        rows.Select(img => img.StorageKey).SequenceEqual(orphanedKeys)
                    )
                ),
            Times.Once
        );
        _unitOfWorkMock.VerifyCommitCalled();
        _cloudinaryMock.Verify(
            x =>
                x.DeleteImagesAsync(
                    It.Is<IEnumerable<string>>(keys => keys.SequenceEqual(orphanedKeys)),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_BodyImagesOrphanedButRowsAlreadyGone_ShouldStillDeleteAssets()
    {
        // Arrange
        Guid articleId = Guid.NewGuid();
        List<string> orphanedKeys = ["content/articles/image-0"];

        _articleRepositoryMock.SetupGetImagesByArticleId(articleId, new List<ArticleImageEntity>());

        // Act
        await _handler.Handle(
            new ArticleBodyImagesOrphanedEvent(ArticleId: articleId, StorageKeys: orphanedKeys),
            CancellationToken.None
        );

        // Assert
        _articleRepositoryMock.Verify(x => x.RemoveImages(It.IsAny<IEnumerable<ArticleImageEntity>>()), Times.Never);
        _cloudinaryMock.VerifyDeleteImagesCalled();
    }

    #endregion
}
