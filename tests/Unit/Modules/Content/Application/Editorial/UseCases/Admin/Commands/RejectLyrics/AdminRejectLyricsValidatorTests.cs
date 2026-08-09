using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectLyrics;

/// <summary>
/// Unit tests for <see cref="AdminRejectLyricsValidator"/>.
/// </summary>
public class AdminRejectLyricsValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminRejectLyricsValidator _validator;

    public AdminRejectLyricsValidatorTests()
    {
        _validator = new AdminRejectLyricsValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminRejectLyricsCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: TestConstants.Lyrics.ValidRejectionReason
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
        var command = new AdminRejectLyricsCommand(Id: string.Empty, Reason: TestConstants.Lyrics.ValidRejectionReason);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Lyrics.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectLyricsCommand(Id: "not-a-guid", Reason: TestConstants.Lyrics.ValidRejectionReason);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsCommand.Id)
                && e.ErrorMessage == _i18n.Lyrics.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectLyricsCommand(Id: Guid.NewGuid().ToString(), Reason: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsCommand.Reason)
                && e.ErrorMessage == _i18n.Lyrics.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectLyricsCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: new string('a', TestConstants.Lyrics.RejectionReasonMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectLyricsCommand.Reason)
                && e.ErrorMessage
                    == _i18n.Lyrics.Msg.RejectionReasonTooLong(TestConstants.Lyrics.RejectionReasonMaxLength)
            );
    }

    #endregion
}
