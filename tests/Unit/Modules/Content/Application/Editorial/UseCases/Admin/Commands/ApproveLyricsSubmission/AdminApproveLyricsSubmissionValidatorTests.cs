using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Unit tests for <see cref="AdminApproveLyricsSubmissionValidator"/>.
/// </summary>
public class AdminApproveLyricsSubmissionValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminApproveLyricsSubmissionValidator _validator;

    public AdminApproveLyricsSubmissionValidatorTests()
    {
        _validator = new AdminApproveLyricsSubmissionValidator(_i18n);
    }

    private static AdminApproveLyricsSubmissionCommand BuildValidCommand(string? slug = null) =>
        new(
            Id: Guid.NewGuid(),
            Slug: slug ?? TestConstants.Content.Editorial.Lyrics.ValidSlug,
            ReviewerId: Guid.NewGuid()
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

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(slug: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminApproveLyricsSubmissionCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(
            slug: new string('a', TestConstants.Content.Editorial.Lyrics.SlugMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminApproveLyricsSubmissionCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugTooLong(TestConstants.Content.Editorial.Lyrics.SlugMaxLength)
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
                e.PropertyName == nameof(AdminApproveLyricsSubmissionCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugInvalidFormat()
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
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminApproveLyricsSubmissionValidator(_i18n);
        var command = BuildValidCommand(slug: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminApproveLyricsSubmissionCommand.Slug)
                && e.ErrorMessage == _i18n.Lyrics.Msg.SlugRequired()
            );
    }

    #endregion
}
