using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Content;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Infrastructure;
using _116.Unit.Tests.Common.Mocks.Repositories;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Unit tests for <see cref="PublicVoteOnLyricsRevisionHandler"/>.
/// </summary>
public class PublicVoteOnLyricsRevisionHandlerTests
{
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ILyricsRevisionVoteRepository> _voteRepositoryMock;
    private readonly Mock<ILyricsRepository> _lyricsRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly PublicVoteOnLyricsRevisionHandler _handler;

    public PublicVoteOnLyricsRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
        _voteRepositoryMock = MockLyricsRevisionVoteRepository.Create();
        _lyricsRepositoryMock = MockLyricsRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicVoteOnLyricsRevisionHandler(
            _revisionRepositoryMock.Object,
            _voteRepositoryMock.Object,
            _lyricsRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNetApprovalsBelowThreshold_ShouldStayPending()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        // GetNetApprovalsAsync returns the tally BEFORE this vote is cast — the handler adds
        // this vote's own +1/-1 contribution itself. threshold - 2 existing approvals + this
        // Approve vote = threshold - 1, staying below the threshold.
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, LyricsRevisionConstants.AutoAcceptThreshold - 2);
        var command = new PublicVoteOnLyricsRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Pending);
        _lyricsRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNetApprovalsReachThreshold_ShouldAutoAcceptAndReplaceLyricsText()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(Guid.NewGuid());
        string originalSongTitle = lyrics.SongTitle;
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(
            lyrics.Id,
            Guid.NewGuid(),
            "Corrected, community-accepted lyrics text."
        );
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _lyricsRepositoryMock.SetupGetByIdOrThrow(lyrics);
        // GetNetApprovalsAsync returns the tally BEFORE this vote — threshold - 1 existing
        // approvals + this Approve vote = exactly the threshold, triggering auto-accept.
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, LyricsRevisionConstants.AutoAcceptThreshold - 1);
        var command = new PublicVoteOnLyricsRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().BeNull();
        lyrics.LyricsText.Should().Be("Corrected, community-accepted lyrics text.");
        lyrics.SongTitle.Should().Be(originalSongTitle);
        _revisionRepositoryMock.VerifyUpdateCalled();
        _lyricsRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserAlreadyVoted_ShouldThrowConflictException()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        var userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, hasVoted: true);
        var command = new PublicVoteOnLyricsRevisionCommand(revision.Id, EnumVote.Approve, null, userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _voteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsRevisionVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
