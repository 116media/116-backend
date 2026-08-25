using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Unit tests for <see cref="PublicRecordShortVideoViewHandler"/>.
/// </summary>
public class PublicRecordShortVideoViewHandlerTests
{
    private readonly Mock<IShortVideoRepository> _shortVideoRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRecordShortVideoViewHandler _handler;

    public PublicRecordShortVideoViewHandlerTests()
    {
        _shortVideoRepositoryMock = MockShortVideoRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRecordShortVideoViewHandler(_shortVideoRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenShortVideoExists_ShouldRecordCountedViewAndCommit()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();

        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        var command = new PublicRecordShortVideoViewCommand(ShortVideoId: shortVideo.Id);

        // Act
        PublicRecordShortVideoViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyAddViewEventCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Dedup Key Resolution

    [Fact]
    public async Task Handle_WhenUserIdPresent_ShouldDeduplicateOnUserKey()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        Guid userId = Guid.NewGuid();

        // User id wins even when a device id and IP are also present.
        var command = new PublicRecordShortVideoViewCommand(
            ShortVideoId: shortVideo.Id,
            UserId: userId,
            DeviceId: "device-abc",
            IpAddress: "203.0.113.5"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be($"user:{userId}");
        recorded.UserId.Should().Be(userId);
        _shortVideoRepositoryMock.Verify(
            x =>
                x.HasCountedViewSinceAsync(
                    shortVideo.Id,
                    $"user:{userId}",
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenOnlyDeviceIdPresent_ShouldDeduplicateOnDeviceKey()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        // Anonymous: device id wins over IP.
        var command = new PublicRecordShortVideoViewCommand(
            ShortVideoId: shortVideo.Id,
            UserId: null,
            DeviceId: "device-abc",
            IpAddress: "203.0.113.5"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be("device:device-abc");
    }

    [Fact]
    public async Task Handle_WhenOnlyIpAddressPresent_ShouldDeduplicateOnIpKey()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        // A whitespace device id counts as absent, so the IP is used.
        var command = new PublicRecordShortVideoViewCommand(
            ShortVideoId: shortVideo.Id,
            UserId: null,
            DeviceId: "   ",
            IpAddress: "203.0.113.5"
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be("ip:203.0.113.5");
    }

    [Fact]
    public async Task Handle_WhenNoIdentitySignals_ShouldUseUnknownKeyAndAlwaysCount()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordShortVideoViewCommand(
            ShortVideoId: shortVideo.Id,
            UserId: null,
            DeviceId: null,
            IpAddress: null
        );

        // Act
        PublicRecordShortVideoViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be("unknown");
        recorded.IsCounted.Should().BeTrue();
        result.IsCounted.Should().BeTrue();

        // The shared "unknown" bucket is never deduplicated, so the window is not queried.
        _shortVideoRepositoryMock.VerifyHasCountedViewSinceNotCalled();
    }

    #endregion

    #region Dedup Counting

    [Fact]
    public async Task Handle_WhenIdentityAlreadyCountedInWindow_ShouldRecordUncountedEvent()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasCountedViewSinceAsync(true);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordShortVideoViewCommand(ShortVideoId: shortVideo.Id, DeviceId: "device-abc");

        // Act
        PublicRecordShortVideoViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
        recorded.Should().NotBeNull();
        recorded!.IsCounted.Should().BeFalse();
        _shortVideoRepositoryMock.VerifyAddViewEventCalled();
        _shortVideoRepositoryMock.VerifyUpdateNotCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenIdentityNotYetCounted_ShouldRecordCountedEvent()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoFactory.Create();
        _shortVideoRepositoryMock.SetupGetByIdOrThrow(shortVideo);
        _shortVideoRepositoryMock.SetupHasCountedViewSinceAsync(false);

        ShortVideoViewEventEntity? recorded = null;
        _shortVideoRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordShortVideoViewCommand(ShortVideoId: shortVideo.Id, DeviceId: "device-abc");

        // Act
        PublicRecordShortVideoViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
        recorded.Should().NotBeNull();
        recorded!.IsCounted.Should().BeTrue();
        _shortVideoRepositoryMock.VerifyAddViewEventCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _shortVideoRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicRecordShortVideoViewCommand(ShortVideoId: id);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
