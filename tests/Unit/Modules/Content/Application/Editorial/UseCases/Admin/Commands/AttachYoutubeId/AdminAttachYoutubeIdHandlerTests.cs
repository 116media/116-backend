using System.Reflection;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Services;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Unit tests for <see cref="AdminAttachYoutubeIdHandler"/>.
/// </summary>
public class AdminAttachYoutubeIdHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICloudinaryService> _cloudinaryMock;
    private readonly Mock<IYoutubeThumbnailService> _youtubeThumbnailMock;
    private readonly AdminAttachYoutubeIdHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminAttachYoutubeIdHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _cloudinaryMock = MockCloudinaryService.Create();
        _youtubeThumbnailMock = MockYoutubeThumbnailService.Create();
        _handler = new AdminAttachYoutubeIdHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cloudinaryMock.Object,
            _youtubeThumbnailMock.Object,
            Mapper
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
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoId: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        AdminAttachYoutubeIdResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
        _cloudinaryMock.VerifyDeleteImageNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoHasExistingThumbnail_ShouldDeleteOldThumbnailAfterUpload()
    {
        // Arrange
        VideoEntity video = WithCategory(VideoFactory.CreateWithThumbnail(CategoryId));
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoId: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        string oldThumbnailKey = video.ThumbnailStorageKey!;

        // Act
        AdminAttachYoutubeIdResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _cloudinaryMock.VerifyUploadCalled();
        _cloudinaryMock.VerifyDeleteImageCalled(oldThumbnailKey);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminAttachYoutubeIdCommand(
            VideoId: nonExistentId.ToString(),
            YoutubeVideoId: TestConstants.Content.Editorial.Video.ValidYoutubeVideoId
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
