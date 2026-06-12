using _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Unit tests for <see cref="AdminSubmitVideoValidator"/>.
/// </summary>
public class AdminSubmitVideoValidatorTests
{
    private readonly VideoErrorMessage _i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>();

    private readonly AdminSubmitVideoValidator _validator;

    public AdminSubmitVideoValidatorTests()
    {
        _validator = new AdminSubmitVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminSubmitVideoCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminSubmitVideoCommand(Id: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminSubmitVideoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminSubmitVideoCommand(Id: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminSubmitVideoCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdInvalid"].Value
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
        var i18n = LocalizerFactory.CreateMessage<VideoErrorMessage>(culture);
        var validator = new AdminSubmitVideoValidator(i18n);
        var command = new AdminSubmitVideoCommand(Id: string.Empty);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminSubmitVideoCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
