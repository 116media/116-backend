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
        lyrics.SongTitle.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidSongTitle);
        lyrics.ArtistName.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidArtistName);
        lyrics.LyricsText.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidLyricsText);
        lyrics.Language.Should().Be(TestConstants.Content.Editorial.Lyrics.ValidLanguage);
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

    #region Update Tests

    [Fact]
    public void Update_WithValidParams_ShouldUpdateAllFields()
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
        const string newSongTitle = "Eloko Oyo";
        const string newArtistName = "Fally Ipupa";
        const string newLyricsText = "Updated lyrics text here.";
        const string newLanguage = "ln";
        var newVideoId = Guid.NewGuid();

        // Act
        lyrics.Update(newSongTitle, newArtistName, newLyricsText, newLanguage, newVideoId);

        // Assert
        lyrics.SongTitle.Should().Be(newSongTitle);
        lyrics.ArtistName.Should().Be(newArtistName);
        lyrics.LyricsText.Should().Be(newLyricsText);
        lyrics.Language.Should().Be(newLanguage);
        lyrics.VideoId.Should().Be(newVideoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptySongTitle_ShouldThrowBadRequestException(string? invalidSongTitle)
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
        Action act = () =>
            lyrics.Update(
                invalidSongTitle!,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                null
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyArtistName_ShouldThrowBadRequestException(string? invalidArtistName)
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
        Action act = () =>
            lyrics.Update(
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                invalidArtistName!,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                null
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyLyricsText_ShouldThrowBadRequestException(string? invalidText)
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
        Action act = () =>
            lyrics.Update(
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                invalidText!,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                null
            );

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
        lyrics.UpdateSeo("SEO Title", "SEO Description", "{\"@type\":\"MusicComposition\"}");

        // Assert
        lyrics.MetaTitle.Should().Be("SEO Title");
        lyrics.MetaDescription.Should().Be("SEO Description");
        lyrics.StructuredData.Should().Be("{\"@type\":\"MusicComposition\"}");
    }

    #endregion
}
