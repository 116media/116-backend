using System.Reflection;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Factories.Core;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;

/// <summary>
/// Unit tests for <see cref="AdminAttachYoutubeVideoUrlHandler"/>.
/// </summary>
public class AdminAttachYoutubeVideoUrlHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IYoutubeThumbnailService> _youtubeThumbnailMock;
    private readonly AdminAttachYoutubeVideoUrlHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminAttachYoutubeVideoUrlHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();
        _youtubeThumbnailMock = MockYoutubeThumbnailService.Create();

        FileEntity fileEntity = FileFactory.CreateImage();
        _fileRepositoryMock.SetupReplaceImageFile(fileEntity);

        _handler = new AdminAttachYoutubeVideoUrlHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            _youtubeThumbnailMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    // Sets the Category navigation property via reflection so mapper can access Category.Name.
    private static VideoEntity WithCategory(VideoEntity entity)
    {
        CategoryEntity category = CategoryFactory.Create(CategoryId);
        typeof(VideoEntity)
            .GetProperty("Category", BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(entity, category);
        return entity;
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenVideoHasNoExistingThumbnail_ShouldAttachAndUploadThumbnail()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.Create(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldReplaceViaCentralizedFileEntity()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.CreateWithThumbnail(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: nonExistentId.ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenUrlContainsNoRecognisedYoutubePattern_ShouldThrowArgumentException()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.Create(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: "https://www.vimeo.com/watch?v=dQw4w9WgXcQ"
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Could not extract a YouTube video ID from*");
    }

    [Fact]
    public async Task Handle_WhenShootIsScheduledInTheFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.CreateWithFutureShoot(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("*YouTube URL cannot be added before the shooting date*");
    }

    [Fact]
    public async Task Handle_WhenShootIsScheduledInThePast_ShouldAttachUrlSuccessfully()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.CreateWithPastShoot(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Content.Editorial.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _fileRepositoryMock.VerifyReplaceImageFileCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region ExtractVideoId (via Handle) — URL format coverage

    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
    public async Task Handle_WithSupportedYoutubeUrlFormat_ShouldExtractIdAndSucceed(
        string youtubeUrl,
        string expectedId
    )
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.Create(CategoryId));
        var command = new AdminAttachYoutubeVideoUrlCommand(VideoId: video.Id.ToString(), YoutubeVideoUrl: youtubeUrl);

        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _youtubeThumbnailMock.Verify(
            x => x.DownloadThumbnailAsync(expectedId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion
}
