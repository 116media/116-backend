using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for lyrics specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
/// </summary>
public class LyricsSpecificationsTests
{
    #region LyricsByIdSpecification

    [Fact]
    public void LyricsByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        var spec = new LyricsByIdSpecification(lyrics.Id);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create();
        var spec = new LyricsByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsSearchSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void LyricsSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new LyricsSearchSpecification("fally");

        // Act
        Func<LyricsEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region LyricsBySongAndArtistSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void LyricsBySongAndArtistSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new LyricsBySongAndArtistSpecification("Eloko Oyo", "Fally Ipupa");

        // Act
        Func<LyricsEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion
}
