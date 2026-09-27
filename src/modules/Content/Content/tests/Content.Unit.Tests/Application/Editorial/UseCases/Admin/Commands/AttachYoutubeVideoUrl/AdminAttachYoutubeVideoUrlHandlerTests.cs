using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.Factories;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeVideoUrl;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.TestData;
using _116.Content.TestData.Builders.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.Application.Shared.Repositories;
using _116.Storage.Application.Shared.Services;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
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
            new VideoDtoFactory(
                Mapper,
                _fileStorageMock.Object,
                _videoRepositoryMock.Object,
                CreateContentLookupFactory()
            ),
            TimeProvider.System
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
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

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
        _videoRepositoryMock
            .Setup(x => x.GetByIdOrThrowAsync(video.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(video);

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
