using _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly ArticleErrorMessage _i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>();
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
            Reason: TestConstants.Content.Editorial.Video.ValidRejectionReason
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
        var command = new AdminRejectVideoCommand(
            Id: string.Empty,
            Reason: TestConstants.Content.Editorial.Video.ValidRejectionReason
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["VideoIdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(
            Id: "not-a-guid",
            Reason: TestConstants.Content.Editorial.Video.ValidRejectionReason
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["VideoIdInvalid"].Value
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
                && e.ErrorMessage == _i18n.RejectionReasonRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReasonExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRejectVideoCommand(
            Id: Guid.NewGuid().ToString(),
            Reason: new string('a', TestConstants.Content.Editorial.Video.RejectionReasonMaxLength + 1)
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
                    == _i18n.RejectionReasonTooLong(TestConstants.Content.Editorial.Video.RejectionReasonMaxLength)
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
        var i18n = LocalizerFactory.CreateMessage<ArticleErrorMessage>(culture);
        var validator = new AdminRejectVideoValidator(i18n);
        var command = new AdminRejectVideoCommand(Id: string.Empty, Reason: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Id)
                && e.ErrorMessage == i18n.Localizer["VideoIdRequired"].Value
            );
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminRejectVideoCommand.Reason)
                && e.ErrorMessage == i18n.RejectionReasonRequired()
            );
    }

    #endregion
}
