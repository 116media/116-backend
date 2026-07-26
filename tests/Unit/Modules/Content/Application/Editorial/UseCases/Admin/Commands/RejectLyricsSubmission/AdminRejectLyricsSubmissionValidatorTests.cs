using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyricsSubmission;

/// <summary>
/// Unit tests for <see cref="AdminRejectLyricsSubmissionValidator"/>.
/// </summary>
public class AdminRejectLyricsSubmissionValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminRejectLyricsSubmissionValidator _validator;

    public AdminRejectLyricsSubmissionValidatorTests()
    {
        _validator = new AdminRejectLyricsSubmissionValidator(_i18n);
    }

    private static AdminRejectLyricsSubmissionCommand BuildValidCommand(string? note = null) =>
        new(
            Id: Guid.NewGuid(),
            Note: note ?? TestConstants.Content.Editorial.Lyrics.ValidRejectionReason,
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

    #region Note Validation Tests

    [Fact]
    public async Task Validate_WithEmptyNote_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(note: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsSubmissionCommand.Note)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNoteExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(
            note: new string('a', TestConstants.Content.Editorial.Lyrics.RejectionReasonMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsSubmissionCommand.Note)
                && e.ErrorMessage
                    == _i18n.Lyrics.Msg.RejectionReasonTooLong(
                        TestConstants.Content.Editorial.Lyrics.RejectionReasonMaxLength
                    )
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
        var validator = new AdminRejectLyricsSubmissionValidator(_i18n);
        var command = BuildValidCommand(note: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsSubmissionCommand.Note)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    #endregion
}
