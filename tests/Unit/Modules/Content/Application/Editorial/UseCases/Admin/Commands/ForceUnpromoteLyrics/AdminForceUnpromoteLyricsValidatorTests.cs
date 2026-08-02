using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ForceUnpromoteLyrics;

/// <summary>
/// Unit tests for <see cref="AdminForceUnpromoteLyricsValidator"/>.
/// </summary>
public class AdminForceUnpromoteLyricsValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminForceUnpromoteLyricsValidator _validator;

    public AdminForceUnpromoteLyricsValidatorTests()
    {
        _validator = new AdminForceUnpromoteLyricsValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteLyricsCommand(Guid.NewGuid(), "Government takedown request.");

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
        var command = new AdminForceUnpromoteLyricsCommand(Guid.Empty, "Government takedown request.");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminForceUnpromoteLyricsCommand.Id));
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteLyricsCommand(Guid.NewGuid(), string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteLyricsCommand.Reason)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminForceUnpromoteLyricsCommand(Guid.NewGuid(), new string('a', 501));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteLyricsCommand.Reason)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonTooLong(500)
            );
    }

    [Fact]
    public async Task Validate_WithReasonAtMaxLength_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminForceUnpromoteLyricsCommand(Guid.NewGuid(), new string('a', 500));

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminForceUnpromoteLyricsValidator(_i18n);
        var command = new AdminForceUnpromoteLyricsCommand(Guid.NewGuid(), string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminForceUnpromoteLyricsCommand.Reason)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    #endregion
}
