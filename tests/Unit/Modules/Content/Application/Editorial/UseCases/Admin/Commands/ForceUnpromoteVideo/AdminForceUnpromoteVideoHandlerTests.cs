using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Services;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteVideo;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteVideoHandler"/>.
/// </summary>
public class AdminForceUnpromoteVideoHandlerTests
{
    private readonly Mock<IVideoRepository> _videoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminForceUnpromoteVideoHandler _handler;

    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly string ActorUserId = Guid.NewGuid().ToString();

    public AdminForceUnpromoteVideoHandlerTests()
    {
        _videoRepositoryMock = MockVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();

        ICurrentActor currentActor = Mock.Of<ICurrentActor>(a =>
            a.UserId == ActorUserId && a.IsAuthenticated == true && a.HasHttpContext == true
        );

        _handler = new AdminForceUnpromoteVideoHandler(
            _videoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            currentActor,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenVideoIsPromoted_ShouldUnpromoteAndReturnResult()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);
        var command = new AdminForceUnpromoteVideoCommand(
            Slug: video.Slug,
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetBySlug(video.Slug, video);

        // Act
        AdminForceUnpromoteVideoResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        video.IsPromoted.Should().BeFalse();
        video.PromotedUntil.Should().BeNull();
        video.PromotionLevelId.Should().BeNull();
        video.UnpromotedAt.Should().NotBeNull();
        video.UnpromotedBy.Should().Be(ActorUserId);
        video.UnpromotedReason.Should().Be(command.Reason);
        result.VideoId.Should().Be(video.Id);
        result.UnpromotedAt.Should().Be(video.UnpromotedAt!.Value);
        _videoRepositoryMock.VerifyUpdateCalled(video);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenVideoIsPromoted_ShouldRaiseContentPromotionRemovedEvent()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);
        video.ClearDomainEvents();
        var command = new AdminForceUnpromoteVideoCommand(
            Slug: video.Slug,
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetBySlug(video.Slug, video);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        video
            .DomainEvents.OfType<ContentPromotionRemovedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new ContentPromotionRemovedEvent(
                    ContentId: video.Id,
                    ContentType: EnumCoreContentType.Video,
                    CustomerId: video.CustomerId,
                    Title: video.Title,
                    Reason: command.Reason
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new AdminForceUnpromoteVideoCommand(
            Slug: "non-existent-slug",
            Reason: TestConstants.Video.ValidRejectionReason
        );
        _videoRepositoryMock.SetupGetBySlug("non-existent-slug", null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
