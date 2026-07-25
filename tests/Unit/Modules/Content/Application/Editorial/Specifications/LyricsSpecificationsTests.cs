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

    #region LyricsByArtistSpecification

    [Fact]
    public void LyricsByArtistSpecification_WithMatchingArtistId_ShouldReturnTrue()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForArtist(CategoryId, artistId);
        var spec = new LyricsByArtistSpecification(artistId);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByArtistSpecification_WithDifferentArtistId_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateForArtist(CategoryId, Guid.NewGuid());
        var spec = new LyricsByArtistSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsByAlbumSpecification

    [Fact]
    public void LyricsByAlbumSpecification_WithMatchingAlbumId_ShouldReturnTrue()
    {
        // Arrange
        Guid albumId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateForAlbum(CategoryId, albumId);
        var spec = new LyricsByAlbumSpecification(albumId);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByAlbumSpecification_WithStandaloneLyrics_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsByAlbumSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsByOrderItemIdSpecification

    [Fact]
    public void LyricsByOrderItemIdSpecification_WithMatchingOrderItemId_ShouldReturnTrue()
    {
        // Arrange
        Guid orderItemId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreatePaid(CategoryId, Guid.NewGuid(), orderItemId);
        var spec = new LyricsByOrderItemIdSpecification(orderItemId);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsByOrderItemIdSpecification_WithDifferentOrderItemId_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePaid(CategoryId, Guid.NewGuid(), Guid.NewGuid());
        var spec = new LyricsByOrderItemIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsSimilarByVideoCategorySpecification

    [Fact]
    public void LyricsSimilarByVideoCategorySpecification_WithPublishedLyricsInSameVideoCategory_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.CreatePublishedWithVideoNavigation(CategoryId, video);
        var spec = new LyricsSimilarByVideoCategorySpecification(CategoryId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsSimilarByVideoCategorySpecification_WithExcludedSourceLyrics_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        LyricsEntity lyrics = LyricsFactory.CreatePublishedWithVideoNavigation(CategoryId, video);
        var spec = new LyricsSimilarByVideoCategorySpecification(CategoryId, lyrics.Id);

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void LyricsSimilarByVideoCategorySpecification_WithStandaloneLyrics_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        var spec = new LyricsSimilarByVideoCategorySpecification(CategoryId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsBySharedTagsSpecification

    [Fact]
    public void LyricsBySharedTagsSpecification_WithPublishedLyricsSharingATag_ShouldReturnTrue()
    {
        // Arrange
        Guid tagId = Guid.NewGuid();
        LyricsEntity lyrics = LyricsFactory.CreateWithTags(CategoryId, tagId);
        lyrics.Publish();
        var spec = new LyricsBySharedTagsSpecification([tagId], Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsBySharedTagsSpecification_WithNoSharedTag_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreateWithTags(CategoryId, Guid.NewGuid());
        lyrics.Publish();
        var spec = new LyricsBySharedTagsSpecification([Guid.NewGuid()], Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsStandaloneSpecification

    [Fact]
    public void LyricsStandaloneSpecification_WithPublishedStandaloneLyrics_ShouldReturnTrue()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.CreatePublished(CategoryId);
        var spec = new LyricsStandaloneSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsStandaloneSpecification_WithDraftStandaloneLyrics_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = LyricsFactory.Create(CategoryId);
        var spec = new LyricsStandaloneSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(lyrics);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region LyricsLikeByUserAndLyricsSpecification

    [Fact]
    public void LyricsLikeByUserAndLyricsSpecification_WithMatchingUserAndLyrics_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid lyricsId = Guid.NewGuid();
        LyricsLikeEntity like = LyricsLikeFactory.Create(userId, lyricsId);
        var spec = new LyricsLikeByUserAndLyricsSpecification(userId, lyricsId);

        // Act
        bool result = spec.IsSatisfiedBy(like);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void LyricsLikeByUserAndLyricsSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        LyricsLikeEntity like = LyricsLikeFactory.Create(Guid.NewGuid(), lyricsId);
        var spec = new LyricsLikeByUserAndLyricsSpecification(Guid.NewGuid(), lyricsId);

        // Act
        bool result = spec.IsSatisfiedBy(like);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
