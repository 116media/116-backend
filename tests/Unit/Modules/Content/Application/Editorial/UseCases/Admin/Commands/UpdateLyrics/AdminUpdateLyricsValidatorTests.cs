using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsValidator"/>.
/// </summary>
public class AdminUpdateLyricsValidatorTests
{
    private readonly LyricsErrorMessage _i18n = LocalizerFactory.CreateMessage<LyricsErrorMessage>();

    private readonly AdminUpdateLyricsValidator _validator;

    public AdminUpdateLyricsValidatorTests()
    {
        _validator = new AdminUpdateLyricsValidator(_i18n);
    }

    private static AdminUpdateLyricsCommand ValidCommand() =>
        new(
            Id: Guid.NewGuid().ToString(),
            SongTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            ArtistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            Language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            VideoId: null
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Act
        ValidationResult result = await _validator.ValidateAsync(ValidCommand());

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Id = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            Id = "not-a-guid",
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region LyricsText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLyricsText_ShouldHaveError()
    {
        // Arrange
        var command = ValidCommand() with
        {
            LyricsText = string.Empty,
        };

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.LyricsText)
                && e.ErrorMessage == _i18n.LyricsTextRequired()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<LyricsErrorMessage>(culture);
        var validator = new AdminUpdateLyricsValidator(i18n);
        var command = ValidCommand() with { LyricsText = string.Empty };

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.LyricsText)
                && e.ErrorMessage == i18n.LyricsTextRequired()
            );
    }

    #endregion
}
