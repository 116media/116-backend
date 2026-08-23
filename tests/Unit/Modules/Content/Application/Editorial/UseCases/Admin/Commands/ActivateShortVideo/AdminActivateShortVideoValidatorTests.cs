using _116.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.ActivateShortVideo;

/// <summary>
/// Unit tests for <see cref="AdminActivateShortVideoValidator"/>.
/// </summary>
public class AdminActivateShortVideoValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminActivateShortVideoValidator _validator;

    public AdminActivateShortVideoValidatorTests()
    {
        _validator = new AdminActivateShortVideoValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminActivateShortVideoCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminActivateShortVideoCommand(Id: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminActivateShortVideoCommand.Id)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminActivateShortVideoCommand(Id: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminActivateShortVideoCommand.Id)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
