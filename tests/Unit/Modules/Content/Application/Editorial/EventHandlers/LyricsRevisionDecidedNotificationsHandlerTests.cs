using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Shared.Messages;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application.DTOs;
using _116.Identity.Contracts.Application.Services;
using _116.Mailer.Contracts.Application.Messages;
using _116.Mailer.Contracts.Application.Services;
using _116.Mailer.Contracts.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Unit tests for <see cref="LyricsRevisionDecidedNotificationsHandler"/>.
/// </summary>
public class LyricsRevisionDecidedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IMessageDispatcher> _dispatcherMock = new();
    private readonly Mock<INotificationService> _notifierMock = new();
    private readonly LyricsRevisionDecidedNotificationsHandler _handler;
    private readonly LyricsEntity _lyrics;

    public LyricsRevisionDecidedNotificationsHandlerTests()
    {
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(_lyrics.Id, _lyrics);

        _handler = new LyricsRevisionDecidedNotificationsHandler(
            _userLookupServiceMock.Object,
            _lyricsRepositoryMock.Object,
            _dispatcherMock.Object,
            _notifierMock.Object,
            NullLogger<LyricsRevisionDecidedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenAccepted_ShouldEnqueueTheRevisionDecidedEmailWithAcceptedDecision()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new LyricsRevisionDecidedEvent(Guid.NewGuid(), _lyrics.Id, proposerId, true, true),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.Verify(
            x =>
                x.DispatchAsync(
                    It.Is<Message>(m =>
                        m.TemplateName == ContentMessageTemplates.RevisionDecided
                        && m.Recipients[0].Address == "proposer@test.com"
                        && m.Tokens["userName"] == "Fally"
                        && m.Tokens["songTitle"] == _lyrics.SongTitle
                        && m.Tokens["decision"] == "accepted"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRejected_ShouldWriteTheRevisionDecidedNotificationWithRejectedDecision()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new LyricsRevisionDecidedEvent(Guid.NewGuid(), _lyrics.Id, proposerId, false, true),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    proposerId,
                    EnumNotificationType.RevisionDecided,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["songTitle"] == _lyrics.SongTitle
                        && t["decision"] == "rejected"
                        && t["linkPath"] == $"/lyrics/{_lyrics.Slug}"
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenProposerHasNoEmail_ShouldSkipTheEmailButStillNotify()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, email: null);

        // Act
        await _handler.Handle(
            new LyricsRevisionDecidedEvent(Guid.NewGuid(), _lyrics.Id, proposerId, true, false),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    proposerId,
                    EnumNotificationType.RevisionDecided,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenProposerNotFound_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(
            new LyricsRevisionDecidedEvent(Guid.NewGuid(), _lyrics.Id, Guid.NewGuid(), true, true),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenLyricsNotFound_ShouldSkipBothChannels()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new LyricsRevisionDecidedEvent(Guid.NewGuid(), Guid.NewGuid(), proposerId, true, true),
            CancellationToken.None
        );

        // Assert
        _dispatcherMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorDto("Fally", email, null, "Visitor"));
    }
}
