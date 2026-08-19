using _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Shared.Domain.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Unit tests for <see cref="AdminPublishVideoHandler"/>.
/// </summary>
public class AdminPublishVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminPublishVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminPublishVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminPublishVideoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenVideoIsApproved_ShouldTransitionToPublished()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApprovedWithYoutubeUrl(CategoryId);
        var command = new AdminPublishVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.Published);
        video.PublishedAt.Should().NotBeNull();
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoIsApproved_ShouldRaiseVideoPublishedEvent()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApprovedWithYoutubeUrl(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminPublishVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video
            .DomainEvents.OfType<VideoPublishedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoPublishedEvent(VideoId: video.Id));
        video
            .DomainEvents.OfType<CommissionedContentPublishedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new CommissionedContentPublishedEvent(
                    ContentId: video.Id,
                    ContentType: EnumCoreContentType.Video,
                    CustomerId: video.CustomerId,
                    Title: video.Title,
                    Slug: video.Slug
                )
            );
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminPublishVideoCommand(Id: nonExistentId.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoAlreadyPublished_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminPublishVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        video.Status.Should().Be(EnumContentStatus.Published);
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoInWrongStatus_ShouldThrowDomainRuleException()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminPublishVideoCommand(Id: video.Id.ToString());
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainRuleException>();
        video.Status.Should().Be(EnumContentStatus.Draft);
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
