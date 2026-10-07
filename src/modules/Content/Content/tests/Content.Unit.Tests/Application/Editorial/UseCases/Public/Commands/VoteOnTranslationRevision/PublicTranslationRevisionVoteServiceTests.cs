using _116.BuildingBlocks.Application.Exceptions;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;
using _116.Content.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision.Contracts;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Mocks.Repositories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.UseCases.Public.Commands.VoteOnTranslationRevision;

/// <summary>
/// Unit tests for <see cref="PublicTranslationRevisionVoteService"/>: the double-vote gate, the staged
/// vote and the tally including it.
/// </summary>
public class PublicTranslationRevisionVoteServiceTests
{
    private readonly Mock<ITranslationRevisionRepository> _revisionRepositoryMock =
        MockTranslationRevisionRepository.Create();
    private readonly Mock<ITranslationVoteRepository> _voteRepositoryMock = MockTranslationVoteRepository.Create();
    private readonly PublicTranslationRevisionVoteService _service;

    public PublicTranslationRevisionVoteServiceTests()
    {
        _service = new PublicTranslationRevisionVoteService(
            _revisionRepositoryMock.Object,
            _voteRepositoryMock.Object,
            TestErrorsFactory.CreateContentI18n()
        );
    }

    [Fact]
    public async Task CastVoteAsync_ShouldStageTheVoteAndCountItInTheTally()
    {
        // Arrange
        var revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, false);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, 1);

        // Act
        TranslationRevisionVoteData tally = await _service.CastVoteAsync(
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
        var revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, false);
        _voteRepositoryMock.SetupGetNetApprovals(revision.Id, 1);

        // Act
        TranslationRevisionVoteData tally = await _service.CastVoteAsync(
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
        var revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        Guid userId = Guid.NewGuid();
        _revisionRepositoryMock.SetupGetByIdOrThrow(revision);
        _voteRepositoryMock.SetupHasVoted(revision.Id, userId, true);

        // Act
        Func<Task> act = async () =>
            await _service.CastVoteAsync(revision.Id, userId, EnumVote.Approve, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
        _voteRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<LyricsTranslationVoteEntity>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
}
