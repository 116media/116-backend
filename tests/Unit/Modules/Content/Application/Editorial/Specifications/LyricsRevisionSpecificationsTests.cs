using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for lyrics-revision and lyrics-revision-vote specification classes.
/// </summary>
public class LyricsRevisionSpecificationsTests
{
    #region LyricsRevisionByIdSpecification

    [Fact]
    public void LyricsRevisionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        var spec = new LyricsRevisionByIdSpecification(revision.Id);

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsRevisionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        LyricsRevisionEntity revision = LyricsRevisionFactory.Create(Guid.NewGuid());
        var spec = new LyricsRevisionByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsRevisionVoteByRevisionAndUserSpecification

    [Fact]
    public void LyricsRevisionVoteByRevisionAndUserSpecification_WithMatchingRevisionAndUser_ShouldReturnTrue()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        LyricsRevisionVoteEntity vote = LyricsRevisionVoteFactory.CreateApprove(revisionId, userId);
        var spec = new LyricsRevisionVoteByRevisionAndUserSpecification(revisionId, userId);

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsRevisionVoteByRevisionAndUserSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        LyricsRevisionVoteEntity vote = LyricsRevisionVoteFactory.CreateApprove(revisionId, Guid.NewGuid());
        var spec = new LyricsRevisionVoteByRevisionAndUserSpecification(revisionId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsRevisionVoteByRevisionIdSpecification

    [Fact]
    public void LyricsRevisionVoteByRevisionIdSpecification_WithMatchingRevisionId_ShouldReturnTrue()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        LyricsRevisionVoteEntity vote = LyricsRevisionVoteFactory.CreateReject(revisionId);
        var spec = new LyricsRevisionVoteByRevisionIdSpecification(revisionId);

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsRevisionVoteByRevisionIdSpecification_WithDifferentRevisionId_ShouldReturnFalse()
    {
        // Arrange
        LyricsRevisionVoteEntity vote = LyricsRevisionVoteFactory.CreateReject(Guid.NewGuid());
        var spec = new LyricsRevisionVoteByRevisionIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
