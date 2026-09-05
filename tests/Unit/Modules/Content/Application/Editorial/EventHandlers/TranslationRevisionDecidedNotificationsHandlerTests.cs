using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application;
using _116.Mailer.Contracts.Application;
using _116.Mailer.Contracts.Domain;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.EventHandlers;

/// <summary>
/// Unit tests for <see cref="TranslationRevisionDecidedNotificationsHandler"/>.
/// </summary>
public class TranslationRevisionDecidedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly TranslationRevisionDecidedNotificationsHandler _handler;
    private readonly LyricsEntity _lyrics;
    private readonly LyricsTranslationEntity _translation;

    public TranslationRevisionDecidedNotificationsHandlerTests()
    {
        _translationRepositoryMock = MockTranslationRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();

        _lyrics = LyricsFactory.Create(Guid.NewGuid());
        _translation = LyricsTranslationFactory.Create(_lyrics.Id);

        _translationRepositoryMock
            .Setup(x => x.GetByIdAsync(_translation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_translation);
        _lyricsRepositoryMock.SetupGetByIdAsync(_lyrics.Id, _lyrics);

        _handler = new TranslationRevisionDecidedNotificationsHandler(
            _userLookupServiceMock.Object,
            _translationRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _mailerMock.Object,
            _notifierMock.Object,
            NullLogger<TranslationRevisionDecidedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenAccepted_ShouldEnqueueTheSharedRevisionDecidedEmail()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new TranslationRevisionDecidedEvent(Guid.NewGuid(), _translation.Id, proposerId, true, false),
            CancellationToken.None
        );

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.RevisionDecided,
                    It.Is<EmailRecipient>(r => r.Address == "proposer@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally" && t["songTitle"] == _lyrics.SongTitle && t["decision"] == "accepted"
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRejected_ShouldWriteTheRevisionDecidedNotificationLinkedToTheLyricsPage()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new TranslationRevisionDecidedEvent(Guid.NewGuid(), _translation.Id, proposerId, false, true),
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
                    It.IsAny<string>(),
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
            new TranslationRevisionDecidedEvent(Guid.NewGuid(), _translation.Id, proposerId, true, true),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    proposerId,
                    EnumNotificationType.RevisionDecided,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenTranslationNotFound_ShouldSkipBothChannels()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        SetupUser(proposerId, "proposer@test.com");

        // Act
        await _handler.Handle(
            new TranslationRevisionDecidedEvent(Guid.NewGuid(), Guid.NewGuid(), proposerId, true, true),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenProposerNotFound_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(
            new TranslationRevisionDecidedEvent(Guid.NewGuid(), _translation.Id, Guid.NewGuid(), true, true),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    private void SetupUser(Guid userId, string? email)
    {
        _userLookupServiceMock
            .Setup(x => x.GetAuthorInfoByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthorInfo("Fally", email, null, "Visitor"));
    }
}
