using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
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

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Unit tests for <see cref="PublicVoteOnTranslationRevisionHandler"/>.
/// </summary>
public class PublicVoteOnTranslationRevisionHandlerTests
{
    private readonly Mock<ITranslationRevisionRepository> _revisionRepositoryMock;
    private readonly Mock<ITranslationVoteRepository> _voteRepositoryMock;
    private readonly Mock<ITranslationRepository> _translationRepositoryMock;
    private readonly Mock<IContentUnitOfWork> _unitOfWorkMock;
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly PublicVoteOnTranslationRevisionHandler _handler;

    public PublicVoteOnTranslationRevisionHandlerTests()
    {
        _revisionRepositoryMock = MockTranslationRevisionRepository.Create();
        _voteRepositoryMock = MockTranslationVoteRepository.Create();
        _translationRepositoryMock = MockTranslationRepository.Create();
        _unitOfWorkMock = MockContentUnitOfWork.Create();
        _handler = new PublicVoteOnTranslationRevisionHandler(
            _revisionRepositoryMock.Object,
            _voteRepositoryMock.Object,
            _translationRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _i18n
        );
    }

    #region Success Cases

    [Fact]
    public async Task Handle_WhenNetApprovalsBelowThreshold_ShouldStayPending()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        // GetNetApprovalsAsync returns the tally BEFORE this vote is cast — the handler adds
        // this vote's own +1/-1 contribution itself. threshold - 2 existing approvals + this
        // Approve vote = threshold - 1, staying below the threshold.
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, TranslationConstants.AutoAcceptThreshold - 2);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Pending);
        _translationRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsTranslationEntity>()), Times.Never);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNetApprovalsReachThreshold_ShouldAutoAcceptAndApplyText()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
            translation.Id,
            Guid.NewGuid(),
            "Newly accepted translation text."
        );
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        // GetNetApprovalsAsync returns the tally BEFORE this vote — threshold - 1 existing
        // approvals + this Approve vote = exactly the threshold, triggering auto-accept.
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, TranslationConstants.AutoAcceptThreshold - 1);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().BeNull();
        translation.Text.Should().Be("Newly accepted translation text.");
        translation.Source.Should().Be(EnumTranslationSource.Community);
        _revisionRepositoryMock.VerifyUpdateCalled();
        _translationRepositoryMock.VerifyUpdateCalled();
        _unitOfWorkMock.VerifyCommitCalled();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserAlreadyVoted_ShouldThrowConflictException()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        var userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, hasVoted: true);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _voteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    #endregion
}
