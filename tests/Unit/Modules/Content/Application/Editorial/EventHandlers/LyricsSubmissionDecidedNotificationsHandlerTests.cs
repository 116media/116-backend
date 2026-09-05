using _116.Content.Application.Editorial.EventHandlers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
/// Unit tests for <see cref="LyricsSubmissionDecidedNotificationsHandler"/>.
/// </summary>
public class LyricsSubmissionDecidedNotificationsHandlerTests
{
    private readonly Mock<IUserLookupService> _userLookupServiceMock = new();
    private readonly Mock<ILyricsSubmissionRepository> _submissionRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IMailer> _mailerMock = new();
    private readonly Mock<INotifier> _notifierMock = new();
    private readonly LyricsSubmissionDecidedNotificationsHandler _handler;
    private readonly LyricsSubmissionEntity _submission;

    public LyricsSubmissionDecidedNotificationsHandlerTests()
    {
        _submissionRepositoryMock = MockLyricsSubmissionRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();

        _submission = LyricsSubmissionFactory.Create();
        _submissionRepositoryMock
            .Setup(x => x.GetByIdAsync(_submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_submission);

        _handler = new LyricsSubmissionDecidedNotificationsHandler(
            _userLookupServiceMock.Object,
            _submissionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _mailerMock.Object,
            _notifierMock.Object,
            NullLogger<LyricsSubmissionDecidedNotificationsHandler>.Instance
        );
    }

    [Fact]
    public async Task Handle_WhenRejected_ShouldEnqueueTheSubmissionDecidedEmailCarryingTheModeratorNote()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        SetupUser(submitterId, "submitter@test.com");

        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                _submission.Id,
                submitterId,
                EnumSubmissionStatus.Rejected,
                "Duplicate of an existing song.",
                null
            ),
            CancellationToken.None
        );

        // Assert
        _mailerMock.Verify(
            x =>
                x.EnqueueAsync(
                    EnumEmailTemplate.SubmissionDecided,
                    It.Is<EmailRecipient>(r => r.Address == "submitter@test.com"),
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["userName"] == "Fally"
                        && t["songTitle"] == _submission.SongTitle
                        && t["outcome"] == "rejected"
                        && t["reviewNote"] == "Duplicate of an existing song."
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenApproved_ShouldWriteTheNotificationLinkedToThePublishedLyricsPage()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        SetupUser(submitterId, "submitter@test.com");

        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        _lyricsRepositoryMock.SetupGetByIdAsync(lyrics.Id, lyrics);

        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                _submission.Id,
                submitterId,
                EnumSubmissionStatus.Approved,
                null,
                lyrics.Id
            ),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    submitterId,
                    EnumNotificationType.SubmissionDecided,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["songTitle"] == _submission.SongTitle
                        && t["outcome"] == "approved"
                        && t["linkPath"] == $"/lyrics/{lyrics.Slug}"
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenRevisionRequested_ShouldWriteTheNotificationWithoutALink()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        SetupUser(submitterId, "submitter@test.com");

        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                _submission.Id,
                submitterId,
                EnumSubmissionStatus.NeedsRevision,
                "Please fix the formatting.",
                null
            ),
            CancellationToken.None
        );

        // Assert
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    submitterId,
                    EnumNotificationType.SubmissionDecided,
                    It.Is<IReadOnlyDictionary<string, string>>(t =>
                        t["outcome"] == "returned for revision" && !t.ContainsKey("linkPath")
                    ),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenSubmitterHasNoEmail_ShouldSkipTheEmailButStillNotify()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        SetupUser(submitterId, email: null);

        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                _submission.Id,
                submitterId,
                EnumSubmissionStatus.Rejected,
                "Not a good fit.",
                null
            ),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.Verify(
            x =>
                x.NotifyAsync(
                    submitterId,
                    EnumNotificationType.SubmissionDecided,
                    It.IsAny<IReadOnlyDictionary<string, string>>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenSubmitterNotFound_ShouldSkipBothChannels()
    {
        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                _submission.Id,
                Guid.NewGuid(),
                EnumSubmissionStatus.Rejected,
                "Not a good fit.",
                null
            ),
            CancellationToken.None
        );

        // Assert
        _mailerMock.VerifyNoOtherCalls();
        _notifierMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_WhenSubmissionNotFound_ShouldSkipBothChannels()
    {
        // Arrange
        var submitterId = Guid.NewGuid();
        SetupUser(submitterId, "submitter@test.com");

        // Act
        await _handler.Handle(
            new LyricsSubmissionDecidedEvent(
                Guid.NewGuid(),
                submitterId,
                EnumSubmissionStatus.Rejected,
                "Not a good fit.",
                null
            ),
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
