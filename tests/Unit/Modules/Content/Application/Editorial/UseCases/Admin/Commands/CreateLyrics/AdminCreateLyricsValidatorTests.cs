using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminCreateLyricsValidator"/>.
/// </summary>
public class AdminCreateLyricsValidatorTests
{
    private readonly AdminCreateLyricsValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region SongTitle Validation Tests

    [Fact]
    public async Task Validate_WithEmptySongTitle_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: string.Empty,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.SongTitle)
                && e.ErrorMessage == "Song title is required."
            );
    }

    [Fact]
    public async Task Validate_WithSongTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: new string('a', TestConstants.Content.Editorial.Lyrics.SongTitleMaxLength + 1),
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.SongTitle)
                && e.ErrorMessage == "Song title must not exceed 200 characters."
            );
    }

    #endregion

    #region ArtistName Validation Tests

    [Fact]
    public async Task Validate_WithEmptyArtistName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: string.Empty,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.ArtistName)
                && e.ErrorMessage == "Artist name is required."
            );
    }

    [Fact]
    public async Task Validate_WithArtistNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: new string('a', TestConstants.Content.Editorial.Lyrics.ArtistNameMaxLength + 1),
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.ArtistName)
                && e.ErrorMessage == "Artist name must not exceed 100 characters."
            );
    }

    #endregion

    #region LyricsText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLyricsText_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: string.Empty,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.LyricsText)
                && e.ErrorMessage == "Lyrics text is required."
            );
    }

    #endregion

    #region Language Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLanguage_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: string.Empty,
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.Language)
                && e.ErrorMessage == "Lyrics language is required."
            );
    }

    [Fact]
    public async Task Validate_WithLanguageExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateLyricsCommand(
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: new string('a', TestConstants.Content.Editorial.Lyrics.LanguageMaxLength + 1),
            AuthorId: Guid.NewGuid(),
            VideoId: null,
            ArticleId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateLyricsCommand.Language)
                && e.ErrorMessage == "Lyrics language must not exceed 5 characters."
            );
    }

    #endregion
}
