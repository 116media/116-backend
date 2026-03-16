using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyrics;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsValidator"/>.
/// </summary>
public class AdminUpdateLyricsValidatorTests
{
    private readonly AdminUpdateLyricsValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateLyricsCommand(
            Id: Guid.NewGuid().ToString(),
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

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
        var command = new AdminUpdateLyricsCommand(
            Id: string.Empty,
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id) && e.ErrorMessage == "Lyrics ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsCommand(
            Id: "not-a-guid",
            LyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.Id) && e.ErrorMessage == "Lyrics ID is invalid."
            );
    }

    #endregion

    #region LyricsText Validation Tests

    [Fact]
    public async Task Validate_WithEmptyLyricsText_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsCommand(Id: Guid.NewGuid().ToString(), LyricsText: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateLyricsCommand.LyricsText)
                && e.ErrorMessage == "Lyrics text is required."
            );
    }

    #endregion
}
