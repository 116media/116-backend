using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
using _116.Tests.TestData.Mocks;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsTranslationVoteEntity"/>.
/// </summary>
public class LyricsTranslationVoteEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithApproveVote_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var revisionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string comment = "Looks correct to me.";

        // Act
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteEntity.Create(
            id,
            revisionId,
            userId,
            EnumVote.Approve,
            comment
        );

        // Assert
        vote.Id.Should().Be(id);
        vote.RevisionId.Should().Be(revisionId);
        vote.UserId.Should().Be(userId);
        vote.Vote.Should().Be(EnumVote.Approve);
        vote.Comment.Should().Be(comment);
    }

    [Fact]
    public void Create_WithRejectVote_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var revisionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string comment = "This translation is inaccurate.";

        // Act
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteEntity.Create(
            id,
            revisionId,
            userId,
            EnumVote.Reject,
            comment
        );

        // Assert
        vote.Vote.Should().Be(EnumVote.Reject);
        vote.Comment.Should().Be(comment);
    }

    [Fact]
    public void Create_WithNullComment_ShouldAllowNull()
    {
        // Act
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            EnumVote.Approve,
            null
        );

        // Assert
        vote.Comment.Should().BeNull();
    }

    #endregion
}
