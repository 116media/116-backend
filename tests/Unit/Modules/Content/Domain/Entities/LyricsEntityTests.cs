using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsEntity"/>.
/// </summary>
public class LyricsEntityTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();

    #region CreateForVideo Tests

    [Fact]
    public void CreateForVideo_WithValidParams_ShouldSetVideoId()
    {
        // Arrange
        var id = Guid.NewGuid();
        var videoId = Guid.NewGuid();

        // Act
        LyricsEntity lyrics = LyricsEntity.CreateForVideo(
            id,
            videoId,
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );

        // Assert
        lyrics.Id.Should().Be(id);
        lyrics.VideoId.Should().Be(videoId);
        lyrics.ArticleId.Should().BeNull();
        lyrics.SongTitle.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidSongTitle);
        lyrics.ArtistName.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidArtistName);
        lyrics.LyricsText.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidLyricsText);
        lyrics.Language.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidLanguage);
    }

    #endregion

    #region CreateForArticle Tests

    [Fact]
    public void CreateForArticle_WithValidParams_ShouldSetArticleId()
    {
        // Arrange
        var articleId = Guid.NewGuid();

        // Act
        LyricsEntity lyrics = LyricsEntity.CreateForArticle(
            Guid.NewGuid(),
            articleId,
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );

        // Assert
        lyrics.ArticleId.Should().Be(articleId);
        lyrics.VideoId.Should().BeNull();
    }

    #endregion

    #region CreateStandalone Tests

    [Fact]
    public void CreateStandalone_WithValidParams_ShouldHaveNoLinks()
    {
        // Act
        LyricsEntity lyrics = LyricsEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );

        // Assert
        lyrics.VideoId.Should().BeNull();
        lyrics.ArticleId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySongTitle_ShouldThrowBadRequestException(string? invalidSongTitle)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateStandalone(
                Guid.NewGuid(),
                invalidSongTitle!,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyArtistName_ShouldThrowBadRequestException(string? invalidArtistName)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateStandalone(
                Guid.NewGuid(),
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                invalidArtistName!,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyLyricsText_ShouldThrowBadRequestException(string? invalidLyricsText)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateStandalone(
                Guid.NewGuid(),
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                invalidLyricsText!,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region UpdateLyrics Tests

    [Fact]
    public void UpdateLyrics_WithValidText_ShouldUpdate()
    {
        // Arrange
        LyricsEntity lyrics = LyricsEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );
        const string newLyricsText = "Updated lyrics text here.";

        // Act
        lyrics.UpdateLyrics(newLyricsText);

        // Assert
        lyrics.LyricsText.Should().Be(newLyricsText);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateLyrics_WithEmptyText_ShouldThrowBadRequestException(string? invalidText)
    {
        // Arrange
        LyricsEntity lyrics = LyricsEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );

        // Act
        Action act = () => lyrics.UpdateLyrics(invalidText!);

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region UpdateSeo Tests

    [Fact]
    public void UpdateSeo_ShouldSetAllMetaFields()
    {
        // Arrange
        LyricsEntity lyrics = LyricsEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId
        );

        // Act
        lyrics.UpdateSeo("SEO Title", "SEO Description", "keyword1,keyword2", "{\"@type\":\"MusicComposition\"}");

        // Assert
        lyrics.MetaTitle.Should().Be("SEO Title");
        lyrics.MetaDescription.Should().Be("SEO Description");
        lyrics.MetaKeywords.Should().Be("keyword1,keyword2");
        lyrics.StructuredData.Should().Be("{\"@type\":\"MusicComposition\"}");
    }

    #endregion
}
