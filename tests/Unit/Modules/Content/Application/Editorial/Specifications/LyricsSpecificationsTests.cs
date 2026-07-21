using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
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
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region LyricsByIdSpecification

    [Fact]
    public void LyricsByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
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
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsBySlugSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void LyricsBySlugSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new LyricsBySlugSpecification("fally-ipupa-eloko-oyo-lyrics");

        // Act
        Func<LyricsEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region LyricsByStatusSpecification

    [Fact]
    public void LyricsByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        var spec = new LyricsByStatusSpecification(EnumContentStatus.Published);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByStatusSpecification(EnumContentStatus.Published);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsByCategorySpecification

    [Fact]
    public void LyricsByCategorySpecification_WithMatchingCategory_ShouldReturnTrue()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByCategorySpecification(CategoryId);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByCategorySpecification_WithDifferentCategory_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByCategorySpecification(Guid.NewGuid());

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

    #region LyricsByVideoIdSpecification

    [Fact]
    public void LyricsByVideoIdSpecification_WithMatchingVideoId_ShouldReturnTrue()
    {
        // Arrange
        Guid videoId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, videoId);
        var spec = new LyricsByVideoIdSpecification(videoId);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByVideoIdSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForVideo(CategoryId, Guid.NewGuid());
        var spec = new LyricsByVideoIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LyricsByVideoIdSpecification_WithStandaloneLyrics_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByVideoIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsByLanguageSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void LyricsByLanguageSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new LyricsByLanguageSpecification("fr");

        // Act
        Func<LyricsEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion
}
