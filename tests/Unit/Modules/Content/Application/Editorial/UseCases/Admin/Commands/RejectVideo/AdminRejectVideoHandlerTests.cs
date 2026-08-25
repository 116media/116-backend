using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Unit tests for <see cref="AdminRejectVideoHandler"/>.
/// </summary>
public class AdminRejectVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminRejectVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();

    public AdminRejectVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminRejectVideoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task Handle_WhenVideoInPendingReview_ShouldTransitionToRejected()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingReview(CategoryId);
        var command = new AdminRejectVideoCommand(
            Id: video.Id.ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.Status.Should().Be(EnumContentStatus.Rejected);
        video.RejectionReason.Should().Be(TestConstants.Video.ValidRejectionReason);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoInPendingReview_ShouldRaiseCommissionedContentRejectedEvent()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePendingReview(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminRejectVideoCommand(
            Id: video.Id.ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video
            .DomainEvents.OfType<CommissionedContentRejectedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new CommissionedContentRejectedEvent(
                    ContentId: video.Id,
                    ContentType: EnumCoreContentType.Video,
                    CustomerId: video.CustomerId,
                    Title: video.Title,
                    Reason: TestConstants.Video.ValidRejectionReason
                )
            );
        video.DomainEvents.OfType<VideoUnpublishedEvent>().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        var command = new AdminRejectVideoCommand(
            Id: nonExistentId.ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetByIdOrThrowNotFound(nonExistentId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoAlreadyRejected_ShouldThrowConflictException()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateRejected(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminRejectVideoCommand(
            Id: video.Id.ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        video.Status.Should().Be(EnumContentStatus.Rejected);
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoInWrongStatus_ShouldThrowBadRequestException()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminRejectVideoCommand(
            Id: video.Id.ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetByIdOrThrow(video);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>();
        video.Status.Should().Be(EnumContentStatus.Draft);
        video.RejectionReason.Should().BeNull();
        video.DomainEvents.Should().BeEmpty();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }
}
