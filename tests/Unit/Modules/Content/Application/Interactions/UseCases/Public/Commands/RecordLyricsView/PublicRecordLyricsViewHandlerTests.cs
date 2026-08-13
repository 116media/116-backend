using _116.Content.Application.Interactions.Constants;
using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Unit tests for <see cref="PublicRecordLyricsViewHandler"/>, in particular the private
/// <c>SatisfiesReadTimeRule</c> read-time view-counting algorithm (spec 05), exercised
/// indirectly through <c>Handle</c>'s observable <c>IsCounted</c> outcome and the persisted
/// raw view event.
/// </summary>
public class PublicRecordLyricsViewHandlerTests
{
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly PublicRecordLyricsViewHandler _handler;

    public PublicRecordLyricsViewHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicRecordLyricsViewHandler(_lyricsRepositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Read-Time Rule — Very Short / Very Long Lyrics

    [Fact]
    public async Task Handle_ForVeryShortLyricsWithDwellAtAbsoluteFloor_ShouldCount()
    {
        // Arrange — a one-word song has a near-zero expected read time, so the absolute
        // MinDwellFloor (not the word-count-derived requirement) is the binding constraint.
        LyricsEntity lyrics = CreateLyricsWithText("Yeah");
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        int dwellMs = (int)LyricsViewCountingConstants.MinDwellFloor.TotalMilliseconds;
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: dwellMs,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ForVeryShortLyricsWithDwellBelowAbsoluteFloor_ShouldNotCount()
    {
        // Arrange
        LyricsEntity lyrics = CreateLyricsWithText("Yeah");
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        int dwellMs = (int)LyricsViewCountingConstants.MinDwellFloor.TotalMilliseconds - 1;
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: dwellMs,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ForVeryLongLyricsWithDwellAtMaxRequiredDwellCap_ShouldCount()
    {
        // Arrange — a very long song's word-count-derived requirement would exceed
        // MaxRequiredDwell, so the cap (not the word count) is the binding constraint.
        string veryLongLyrics = string.Join(' ', Enumerable.Repeat("word", 2000));
        LyricsEntity lyrics = CreateLyricsWithText(veryLongLyrics);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        int dwellMs = (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds;
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: dwellMs,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ForVeryLongLyricsWithDwellBelowMaxRequiredDwellCap_ShouldNotCount()
    {
        // Arrange
        string veryLongLyrics = string.Join(' ', Enumerable.Repeat("word", 2000));
        LyricsEntity lyrics = CreateLyricsWithText(veryLongLyrics);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        int dwellMs = (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds - 1000;
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: dwellMs,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
    }

    #endregion

    #region Read-Time Rule — MinScrollDepthRatio Boundary

    [Fact]
    public async Task Handle_WithScrollDepthAtMinScrollDepthRatio_AndSufficientDwell_ShouldCount()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: LyricsViewCountingConstants.MinScrollDepthRatio
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithScrollDepthBelowMinScrollDepthRatio_ShouldNotCount()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: LyricsViewCountingConstants.MinScrollDepthRatio - 0.01
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
    }

    #endregion

    #region Read-Time Rule — MinReadTimeRatio Effect

    [Fact]
    public async Task Handle_WithDwellJustBelowRequiredReadTimeRatio_ShouldNotCount()
    {
        // Arrange — a mid-length song where the word-count-derived requirement (before the
        // MaxRequiredDwell cap kicks in) is the binding constraint.
        string lyricsText = string.Join(' ', Enumerable.Repeat("word", 100));
        LyricsEntity lyrics = CreateLyricsWithText(lyricsText);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        double expectedReadMs = 100 / LyricsViewCountingConstants.WordsPerMinute * 60_000;
        double requiredMs = Math.Min(
            expectedReadMs * LyricsViewCountingConstants.MinReadTimeRatio,
            LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds
        );

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)requiredMs - 1,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithDwellAtRequiredReadTimeRatio_ShouldCount()
    {
        // Arrange
        string lyricsText = string.Join(' ', Enumerable.Repeat("word", 100));
        LyricsEntity lyrics = CreateLyricsWithText(lyricsText);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        double expectedReadMs = 100 / LyricsViewCountingConstants.WordsPerMinute * 60_000;
        double requiredMs = Math.Min(
            expectedReadMs * LyricsViewCountingConstants.MinReadTimeRatio,
            LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds
        );

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)Math.Ceiling(requiredMs),
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeTrue();
    }

    #endregion

    #region Handler-Level Behaviour For A Failed Read-Time Rule

    [Fact]
    public async Task Handle_WhenReadTimeRuleFails_ShouldStillSucceedAndPersistRawEventUncounted()
    {
        // Arrange — a bounce: negligible dwell and scroll depth.
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        LyricsViewEventEntity? recorded = null;
        _lyricsRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: Guid.NewGuid(),
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: 200,
            ScrollDepthRatio: 0.1
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsCounted.Should().BeFalse();
        recorded.Should().NotBeNull();
        recorded!.IsCounted.Should().BeFalse();
        _lyricsRepositoryMock.VerifyAddViewEventCalled();
        _lyricsRepositoryMock.VerifyUpdateNotCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Dedup Counting

    [Fact]
    public async Task Handle_WhenIdentityAlreadyCountedInWindow_ShouldRecordUncountedEvent()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        _lyricsRepositoryMock.SetupHasCountedViewSinceAsync(true);

        LyricsViewEventEntity? recorded = null;
        _lyricsRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: null,
            DeviceId: "device-abc",
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert — the read-time rule passes but the dedup window already has a counted view.
        result.IsCounted.Should().BeFalse();
        recorded.Should().NotBeNull();
        recorded!.IsCounted.Should().BeFalse();
        _lyricsRepositoryMock.VerifyAddViewEventCalled();
        _lyricsRepositoryMock.VerifyUpdateNotCalled();
    }

    [Fact]
    public async Task Handle_WhenNoIdentitySignals_ShouldUseUnknownKeyAndNeverDeduplicate()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        LyricsViewEventEntity? recorded = null;
        _lyricsRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: null,
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be("unknown");
        result.IsCounted.Should().BeTrue();
        _lyricsRepositoryMock.VerifyHasCountedViewSinceNotCalled();
    }

    [Fact]
    public async Task Handle_WhenOnlyIpAddressIsAvailable_ShouldDeduplicateOnTheIpKey()
    {
        // Arrange — no user id and no device id, so the IP address is the strongest signal left.
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        LyricsViewEventEntity? recorded = null;
        _lyricsRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        const string ipAddress = "196.4.10.22";
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: null,
            DeviceId: null,
            IpAddress: ipAddress,
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be($"ip:{ipAddress}");
        result.IsCounted.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserIsAuthenticated_ShouldPreferTheUserDedupKeyOverDeviceAndIp()
    {
        // Arrange — all three signals present; the user id must win.
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);

        LyricsViewEventEntity? recorded = null;
        _lyricsRepositoryMock.SetupCaptureViewEvent(viewEvent => recorded = viewEvent);

        var userId = Guid.NewGuid();
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: lyrics.Id,
            UserId: userId,
            DeviceId: "device-abc",
            IpAddress: "196.4.10.22",
            UserAgent: null,
            DwellMs: (int)LyricsViewCountingConstants.MaxRequiredDwell.TotalMilliseconds,
            ScrollDepthRatio: 1.0
        );

        // Act
        PublicRecordLyricsViewResult result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        recorded.Should().NotBeNull();
        recorded!.DedupKey.Should().Be($"user:{userId}");
        result.IsCounted.Should().BeTrue();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        Guid id = Guid.NewGuid();
        _lyricsRepositoryMock.SetupGetByIdOrThrowNotFound(id);

        var command = new PublicRecordLyricsViewCommand(
            LyricsId: id,
            UserId: null,
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: 5000,
            ScrollDepthRatio: 1.0
        );

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    /// <summary>
    /// Creates a lyrics page with a specific lyrics text, used to control the word-count-derived
    /// expected reading time the read-time rule recomputes server-side.
    /// </summary>
    private static LyricsEntity CreateLyricsWithText(string lyricsText)
    {
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        lyrics.Update(
            categoryId: lyrics.CategoryId,
            songTitle: lyrics.SongTitle,
            artistName: lyrics.ArtistName,
            slug: lyrics.Slug,
            lyricsText: lyricsText,
            language: lyrics.Language,
            videoId: lyrics.VideoId,
            customerId: lyrics.CustomerId,
            orderItemId: lyrics.OrderItemId,
            errors: TestErrorsFactory.CreateLyricsErrors()
        );
        return lyrics;
    }
}
