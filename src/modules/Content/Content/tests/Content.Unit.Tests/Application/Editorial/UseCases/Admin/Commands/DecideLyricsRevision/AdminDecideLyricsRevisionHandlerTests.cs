using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
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
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Helpers;
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Unit tests for <see cref="AdminDecideLyricsRevisionHandler"/>.
/// </summary>
public class AdminDecideLyricsRevisionHandlerTests
{
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminDecideLyricsRevisionHandler _handler;

    public AdminDecideLyricsRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDecideLyricsRevisionHandler(
            _revisionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Accept Cases

    [Fact]
    public async Task Handle_WhenAcceptTrue_ShouldBypassTallyAndReplaceLyricsTextWithRealModerator()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
            lyrics.Id,
            Guid.NewGuid(),
            "Moderator-approved lyrics text."
        );
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: true, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().Be(moderatorId);
        lyrics.LyricsText.Should().Be("Moderator-approved lyrics text.");
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAcceptTrue_ShouldRaiseLyricsRevisionDecidedEvent()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
            lyrics.Id,
            Guid.NewGuid(),
            "Moderator-approved lyrics text."
        );
        revision.ClearDomainEvents();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        var moderatorId = Guid.NewGuid();
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: true, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision
            .DomainEvents.OfType<LyricsRevisionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsRevisionDecidedEvent(
                    RevisionId: revision.Id,
                    LyricsId: lyrics.Id,
                    ProposedByUserId: revision.ProposedByUserId,
                    Accepted: true,
                    ByModerator: true
                )
            );
    }

    #endregion

    #region Reject Cases

    [Fact]
    public async Task Handle_WhenAcceptFalse_ShouldBypassTallyAndRejectWithoutTouchingLyrics()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        var moderatorId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: false, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Rejected);
        revision.DecidedByUserId.Should().Be(moderatorId);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenAcceptFalse_ShouldRaiseLyricsRevisionDecidedEvent()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        revision.ClearDomainEvents();
        var moderatorId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        var command = new AdminDecideLyricsRevisionCommand(revision.Id, Accept: false, moderatorId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision
            .DomainEvents.OfType<LyricsRevisionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new LyricsRevisionDecidedEvent(
                    RevisionId: revision.Id,
                    LyricsId: revision.LyricsId,
                    ProposedByUserId: revision.ProposedByUserId,
                    Accepted: false,
                    ByModerator: true
                )
            );
    }

    #endregion
}
