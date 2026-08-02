using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

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
