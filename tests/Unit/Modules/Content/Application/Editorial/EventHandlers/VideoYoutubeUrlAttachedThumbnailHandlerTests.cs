using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Unit tests for <see cref="VideoYoutubeUrlAttachedThumbnailHandler"/>.
/// </summary>
public class VideoYoutubeUrlAttachedThumbnailHandlerTests
{
    private const string ValidYoutubeUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IYoutubeThumbnailService> _youtubeThumbnailMock;
    private readonly FileEntity _uploadedFile;
    private readonly VideoYoutubeUrlAttachedThumbnailHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public VideoYoutubeUrlAttachedThumbnailHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _youtubeThumbnailMock = MockYoutubeThumbnailService.Create();

        _uploadedFile = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(_uploadedFile);

        _handler = new VideoYoutubeUrlAttachedThumbnailHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            _youtubeThumbnailMock.Object,
            Mock.Of<ILogger<VideoYoutubeUrlAttachedThumbnailHandler>>()
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
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        video.ThumbnailFileId.Should().Be(_uploadedFile.Id);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
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
        _fileRepositoryMock.Verify(
            x =>
                x.ReplaceImageFileAsync(
                    previousThumbnailFileId,
                    It.IsAny<IFormFile>(),
                    video.Id.ToString(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        video.ThumbnailFileId.Should().Be(_uploadedFile.Id);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
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
        _fileRepositoryMock.VerifyReplaceImageFileNotCalled();
        _unitOfWorkMock.VerifyCommitNotCalled();
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
