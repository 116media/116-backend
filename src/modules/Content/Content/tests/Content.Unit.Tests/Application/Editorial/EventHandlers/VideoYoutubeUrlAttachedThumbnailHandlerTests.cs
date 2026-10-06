using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Editorial.Ports;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using AwesomeAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoYoutubeUrlAttachedThumbnailHandler"/>.
/// </summary>
public class VideoYoutubeUrlAttachedThumbnailHandlerTests
{
    private const string ValidYoutubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IYoutubeThumbnailService> _youtubeThumbnailMock;
    private readonly FileReferenceDto _uploadedFile;
    private readonly VideoYoutubeUrlAttachedThumbnailHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public VideoYoutubeUrlAttachedThumbnailHandlerTests()
    {
        _fileStorageMock = MockFileStorageService.Create();
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _youtubeThumbnailMock = MockYoutubeThumbnailService.Create();

        _uploadedFile = FileReferenceDtoFactory.CreateImage();
        _fileStorageMock.SetupUpload(StoredFileFactory.From(_uploadedFile));

        _handler = new VideoYoutubeUrlAttachedThumbnailHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileStorageMock.Object,
            _youtubeThumbnailMock.Object,
            NullLogger<VideoYoutubeUrlAttachedThumbnailHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WithValidUrl_ShouldDownloadReplaceAttachAndCommit()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(
            new VideoYoutubeUrlAttachedEvent(VideoId: video.Id, YoutubeVideoUrl: ValidYoutubeUrl),
            CancellationToken.None
        );

        // Assert
        _youtubeThumbnailMock.Verify(
            x => x.DownloadThumbnailAsync("dQw4w9WgXcQ", It.IsAny<CancellationToken>()),
            Times.Once
        );

        video.ThumbnailFileId.Should().Be(_uploadedFile.Id);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldReplaceItByFileId()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithThumbnail(CategoryId);
        Guid? previousThumbnailFileId = video.ThumbnailFileId;
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(
            new VideoYoutubeUrlAttachedEvent(VideoId: video.Id, YoutubeVideoUrl: ValidYoutubeUrl),
            CancellationToken.None
        );

        // Assert

        video.ThumbnailFileId.Should().Be(_uploadedFile.Id);
        _unitOfWorkMock.VerifyExecutedInTransaction();
    }

    [Fact]
    public async Task Handle_WhenUrlContainsNoYoutubeId_ShouldSkipWithoutTouchingAnything()
    {
        // Act
        await _handler.Handle(
            new VideoYoutubeUrlAttachedEvent(
                VideoId: Guid.NewGuid(),
                YoutubeVideoUrl: "https://www.vimeo.com/watch?v=dQw4w9WgXcQ"
            ),
            CancellationToken.None
        );

        // Assert
        _youtubeThumbnailMock.Verify(
            x => x.DownloadThumbnailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );

        _unitOfWorkMock.VerifyExecutedInTransaction(0);
    }

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public async Task Handle_WithSupportedYoutubeUrlFormat_ShouldExtractIdAndDownload(
        string youtubeUrl,
        string expectedId
    )
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(
            new VideoYoutubeUrlAttachedEvent(VideoId: video.Id, YoutubeVideoUrl: youtubeUrl),
            CancellationToken.None
        );

        // Assert
        _youtubeThumbnailMock.Verify(
            x => x.DownloadThumbnailAsync(expectedId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        video.ThumbnailFileId.Should().Be(_uploadedFile.Id);
    }
}
