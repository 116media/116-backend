using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Commands.VoteOnLyricsRevision;

/// <summary>
/// Unit tests for <see cref="PublicLyricsRevisionVoteService"/>: the double-vote gate, the staged
/// vote and the tally including it.
/// </summary>
public class PublicLyricsRevisionVoteServiceTests
{
    private readonly Mock<ILyricsRevisionRepository> _revisionRepositoryMock = MockLyricsRevisionRepository.Create();
    private readonly Mock<ILyricsRevisionVoteRepository> _voteRepositoryMock =
        MockLyricsRevisionVoteRepository.Create();
    private readonly PublicLyricsRevisionVoteService _service;

    public PublicLyricsRevisionVoteServiceTests()
    {
        _service = new PublicLyricsRevisionVoteService(
            _revisionRepositoryMock.Object,
            _voteRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task CastVoteAsync_ShouldStageTheVoteAndCountItInTheTally()
    {
        // Arrange
        var revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, false);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, 1);

        // Act
        LyricsRevisionVoteData tally = await _service.CastVoteAsync(
            revision.Id,
            userId,
            EnumVote.Approve,
            null,
            CancellationToken.None
        );

        // Assert
        tally.Revision.Should().BeSameAs(revision);
        tally.NetApprovals.Should().Be(2);
        _voteRepositoryMock.VerifyAddCalled();
    }

    [Fact]
    public async Task CastVoteAsync_WithARejection_ShouldLowerTheTally()
    {
        // Arrange
        var revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, false);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, 1);

        // Act
        LyricsRevisionVoteData tally = await _service.CastVoteAsync(
            revision.Id,
            userId,
            EnumVote.Reject,
            null,
            CancellationToken.None
        );

        // Assert
        tally.NetApprovals.Should().Be(0);
    }

    [Fact]
    public async Task CastVoteAsync_WhenTheUserAlreadyVoted_ShouldThrowConflictExceptionWithoutStaging()
    {
        // Arrange
        var revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, true);

        // Act
        Func<Task> act = async () =>
            await _service.CastVoteAsync(revision.Id, userId, EnumVote.Approve, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _voteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsRevisionVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
