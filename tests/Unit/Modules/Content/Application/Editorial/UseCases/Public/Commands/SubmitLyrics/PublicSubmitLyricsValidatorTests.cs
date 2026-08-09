using _116.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Public.Commands.SubmitLyrics;

/// <summary>
/// Unit tests for <see cref="PublicSubmitLyricsValidator"/>.
/// </summary>
public class PublicSubmitLyricsValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly PublicSubmitLyricsValidator _validator;

    public PublicSubmitLyricsValidatorTests()
    {
        _validator = new PublicSubmitLyricsValidator(_i18n);
    }

    private static PublicSubmitLyricsCommand BuildValidCommand(
        string? songTitle = null,
        string? artistName = null,
        string? lyricsText = null,
        string? language = null,
        string? slug = null
    ) =>
        new(
            SongTitle: songTitle ?? TestConstants.Lyrics.ValidSongTitle,
            ArtistName: artistName,
            LyricsText: lyricsText ?? TestConstants.Lyrics.ValidLyricsText,
            Language: language ?? TestConstants.Lyrics.ValidLanguage,
            Slug: slug,
            UserId: Guid.NewGuid()
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAllOptionalFieldsProvided_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(
            artistName: TestConstants.Lyrics.ValidArtistName,
            slug: TestConstants.Lyrics.ValidSlug
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
        var command = BuildValidCommand(songTitle: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.SongTitle)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SongTitleRequired()
            );
    }

    [Fact]
    public async Task Validate_WithSongTitleExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(songTitle: new string('a', TestConstants.Lyrics.SongTitleMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.SongTitle)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SongTitleTooLong(TestConstants.Lyrics.SongTitleMaxLength)
            );
    }

    #endregion

    #region LyricsText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLyricsText_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(lyricsText: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.LyricsText)
                && e.ErrorMessage == _i18n.Lyrics.Msg.LyricsTextRequired()
            );
    }

    #endregion

    #region Language Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLanguage_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(language: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.Language)
                && e.ErrorMessage == _i18n.Lyrics.Msg.LanguageRequired()
            );
    }

    [Fact]
    public async Task Validate_WithLanguageExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(language: new string('a', TestConstants.Lyrics.LanguageMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.Language)
                && e.ErrorMessage == _i18n.Lyrics.Msg.LanguageTooLong(TestConstants.Lyrics.LanguageMaxLength)
            );
    }

    #endregion

    #region Optional Slug Validation Tests

    [Fact]
    public async Task Validate_WithNullSlug_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(slug: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithWhitespaceSlug_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(slug: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(slug: new string('a', TestConstants.Lyrics.SlugMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugTooLong(TestConstants.Lyrics.SlugMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(slug: "Invalid-Slug");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugInvalidFormat()
            );
    }

    #endregion

    #region Optional ArtistName Validation Tests

    [Fact]
    public async Task Validate_WithNullArtistName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(artistName: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithWhitespaceArtistName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(artistName: "   ");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithArtistNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(artistName: new string('a', TestConstants.Lyrics.ArtistNameMaxLength + 1));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(PublicSubmitLyricsCommand.ArtistName)
                && e.ErrorMessage == _i18n.Lyrics.Msg.ArtistNameTooLong(TestConstants.Lyrics.ArtistNameMaxLength)
            );
    }

    #endregion
}
