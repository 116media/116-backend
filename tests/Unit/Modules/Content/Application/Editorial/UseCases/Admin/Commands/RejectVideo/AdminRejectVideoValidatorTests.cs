using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Unit tests for <see cref="AdminRejectVideoValidator"/>.
/// </summary>
public class AdminRejectVideoValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminRejectVideoValidator _validator;

    public AdminRejectVideoValidatorTests()
    {
        _validator = new AdminRejectVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: TestConstants.Video.ValidRejectionReason
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
        var command = new AdminRejectVideoCommand(Id: string.Empty, Reason: TestConstants.Video.ValidRejectionReason);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Id)
                && e.ErrorMessage == _i18n.Article.Msg.Localizer["VideoIdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(Id: "not-a-guid", Reason: TestConstants.Video.ValidRejectionReason);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Id)
                && e.ErrorMessage == _i18n.Article.Msg.Localizer["VideoIdInvalid"].Value
            );
    }

    #endregion

    #region Reason Validation Tests

    [Fact]
    public async Task Validate_WithEmptyReason_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(Id: Guid.NewGuid().ToString(), Reason: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Reason)
                && e.ErrorMessage == _i18n.Article.Msg.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: new string('a', TestConstants.Video.RejectionReasonMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Reason)
                && e.ErrorMessage
                    == _i18n.Article.Msg.RejectionReasonTooLong(TestConstants.Video.RejectionReasonMaxLength)
            );
    }

    #endregion
}
