using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
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
    private readonly AdminAttachYoutubeVideoUrlHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminAttachYoutubeVideoUrlHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _fileRepositoryMock = MockFileRepository.Create();

        _handler = new AdminAttachYoutubeVideoUrlHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _fileRepositoryMock.Object,
            Mapper,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    /// <summary>
    /// Builds a video carrying the Category navigation EF Core would populate, so the mapper can
    /// read Category.Name.
    /// </summary>
    private static VideoEntity CreateVideoWithCategory(DateTimeOffset? shootingScheduledAt = null)
    {
        var builder = new VideoBuilder(CategoryId).WithCategory(CategoryFactory.Create(CategoryId));

        if (shootingScheduledAt.HasValue)
        {
            builder.WithShootingScheduledAt(shootingScheduledAt.Value);
        }

        return builder.Build();
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
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        AdminAttachYoutubeVideoUrlResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Video.Should().NotBeNull();
        video.YoutubeVideoUrl.Should().Be(command.YoutubeVideoUrl);
        _videoRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WithValidUrl_ShouldRaiseAttachmentEventAndSkipInlineThumbnailWork()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory();
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
        );

        _videoRepositoryMock.SetupGetByIdOrThrow(video);
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — the thumbnail is acquired by the post-commit consumer, not inline.
        video
            .DomainEvents.OfType<VideoYoutubeUrlAttachedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoYoutubeUrlAttachedEvent(video.Id, command.YoutubeVideoUrl));
        _fileRepositoryMock.VerifyReplaceImageFileNotCalled();
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
    }

    [Fact]
    public async Task Handle_WhenShootIsScheduledInTheFuture_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = CreateVideoWithCategory(DateTimeOffset.UtcNow.AddDays(30));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
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
        VideoEntity video = CreateVideoWithCategory(DateTimeOffset.UtcNow.AddDays(-7));
        var command = new AdminAttachYoutubeVideoUrlCommand(
            VideoId: video.Id.ToString(),
            YoutubeVideoUrl: TestConstants.Video.ValidYoutubeVideoUrl
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
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion
}
