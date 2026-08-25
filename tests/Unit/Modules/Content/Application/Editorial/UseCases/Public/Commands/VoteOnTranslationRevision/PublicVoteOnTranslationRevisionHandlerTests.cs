using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
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
        revision.ClearDomainEvents();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, TranslationConstants.AutoAcceptThreshold - 2);
        var userId = Guid.NewGuid();
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, userId);

        LyricsTranslationVoteEntity? addedVote = null;
        _voteRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()))
            .Callback<LyricsTranslationVoteEntity, CancellationToken>((vote, _) => addedVote = vote)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Pending);
        revision.DecidedByUserId.Should().BeNull();
        revision.DomainEvents.Should().BeEmpty();
        addedVote.Should().NotBeNull();
        addedVote!.RevisionId.Should().Be(revision.Id);
        addedVote.UserId.Should().Be(userId);
        addedVote.Vote.Should().Be(EnumVote.Approve);
        _revisionRepositoryMock.Verify(x => x.Update(It.IsAny<LyricsTranslationRevisionEntity>()), Times.Never);
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
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, TranslationConstants.AutoAcceptThreshold - 1);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision.Status.Should().Be(EnumRevisionStatus.Accepted);
        revision.DecidedByUserId.Should().BeNull();
        translation.Text.Should().Be("Newly accepted translation text.");
        translation.Source.Should().Be(EnumTranslationSource.Community);
        _voteRepositoryMock.VerifyAddCalled();
        _revisionRepositoryMock.VerifyUpdateCalled(revision);
        _translationRepositoryMock.VerifyUpdateCalled(translation);
        _unitOfWorkMock.VerifyCommitCalled();
    }

    [Fact]
    public async Task Handle_WhenNetApprovalsReachThreshold_ShouldRaiseTranslationRevisionDecidedEvent()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(
            translation.Id,
            Guid.NewGuid(),
            "Newly accepted translation text."
        );
        revision.ClearDomainEvents();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _translationRepositoryMock.SetupGetByIdOrThrow(translation);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, TranslationConstants.AutoAcceptThreshold - 1);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        revision
            .DomainEvents.OfType<TranslationRevisionDecidedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(
                new TranslationRevisionDecidedEvent(
                    RevisionId: revision.Id,
                    TranslationId: translation.Id,
                    ProposedByUserId: revision.ProposedByUserId,
                    Accepted: true,
                    ByModerator: false
                )
            );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task Handle_WhenUserAlreadyVoted_ShouldThrowConflictException()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        revision.ClearDomainEvents();
        var userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, hasVoted: true);
        var command = new PublicVoteOnTranslationRevisionCommand(revision.Id, EnumVote.Approve, null, userId);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        revision.Status.Should().Be(EnumRevisionStatus.Pending);
        revision.DomainEvents.Should().BeEmpty();
        _voteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _unitOfWorkMock.VerifyCommitNotCalled();
    }

    #endregion
}
