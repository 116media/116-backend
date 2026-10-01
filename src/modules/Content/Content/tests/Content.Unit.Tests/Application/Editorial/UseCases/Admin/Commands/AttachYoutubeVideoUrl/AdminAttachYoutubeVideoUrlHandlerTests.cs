using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.TestData;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Tests.TestData.Constants;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;

/// <summary>
/// Unit tests for <see cref="AdminAttachYoutubeVideoUrlHandler"/>.
/// </summary>
public class AdminAttachYoutubeVideoUrlHandlerTests : BaseContentHandlerTest
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly AdminAttachYoutubeVideoUrlHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminAttachYoutubeVideoUrlHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileStorageMock = MockFileStorageService.Create();

        _handler = new AdminAttachYoutubeVideoUrlHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            new VideoDtoService(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupService()
            ),
            TimeProvider.System
        );
    }

    /// <summary>
    /// Builds a video filed under a category, optionally with a shoot already scheduled.
    /// </summary>
    private static VideoEntity CreateVideoWithCategory(DateTimeOffset? shootingScheduledAt = null)
    {
        CategoryEntity category = CategoryFactory.Create(CategoryId);

        return shootingScheduledAt.HasValue
            ? VideoFactory.CreateWithShootingScheduledAt(category, shootingScheduledAt.Value)
            : VideoFactory.CreateWithCategory(category);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WithValidUrl_ShouldAttachUrlAndCommit()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.YoutubeVideoUrl.Should().Be(command.YoutubeVideoUrl);
        result.Video.Id.Should().Be(video.Id);
        result.Video.YoutubeVideoUrl.Should().Be(command.YoutubeVideoUrl);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidUrl_ShouldRaiseAttachmentEventAndSkipInlineThumbnailWork()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        video.ClearDomainEvents();
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video
            .DomainEvents.OfType<VideoYoutubeUrlAttachedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoYoutubeUrlAttachedEvent(VideoId: video.Id, YoutubeVideoUrl: command.YoutubeVideoUrl));
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
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenShootIsScheduledInTheFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory(DateTimeOffset.UtcNow.AddDays(30));
        video.ClearDomainEvents();
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        (await act.Should().ThrowAsync<ContentRuleException>())
            .Which.Code.Should()
            .Be(ContentRuleCodes.CannotAttachYoutubeUrlBeforeShoot);
        video.YoutubeVideoUrl.Should().BeNull();
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenShootIsScheduledInThePast_ShouldAttachUrlSuccessfully()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory(DateTimeOffset.UtcNow.AddDays(-7));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.YoutubeVideoUrl.Should().Be(command.YoutubeVideoUrl);
        result.Video.Id.Should().Be(video.Id);
        result.Video.YoutubeVideoUrl.Should().Be(command.YoutubeVideoUrl);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
