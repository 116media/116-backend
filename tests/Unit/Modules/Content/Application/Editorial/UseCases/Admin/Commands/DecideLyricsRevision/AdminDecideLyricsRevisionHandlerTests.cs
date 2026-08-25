using _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DecideLyricsRevision;

/// <summary>
/// Unit tests for <see cref="AdminDecideLyricsRevisionHandler"/>.
/// </summary>
public class AdminDecideLyricsRevisionHandlerTests
{
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly AdminDecideLyricsRevisionHandler _handler;

    public AdminDecideLyricsRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new AdminDecideLyricsRevisionHandler(
            _revisionRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object
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
        _revisionRepositoryMock.VerifyUpdateCalled(revision);
        _lyricsRepositoryMock.VerifyUpdateCalled(lyrics);
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
        _revisionRepositoryMock.VerifyUpdateCalled(revision);
        _lyricsRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Never);
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
