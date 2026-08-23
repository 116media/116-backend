using _116.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.DeactivateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateShortVideoValidator"/>.
/// </summary>
public class AdminDeactivateShortVideoValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminDeactivateShortVideoValidator _validator;

    public AdminDeactivateShortVideoValidatorTests()
    {
        _validator = new AdminDeactivateShortVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivateShortVideoCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminDeactivateShortVideoCommand(Id: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminDeactivateShortVideoCommand.Id)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminDeactivateShortVideoCommand(Id: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminDeactivateShortVideoCommand.Id)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
