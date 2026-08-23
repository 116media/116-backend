using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePromotionLevelValidator"/>.
/// </summary>
public class AdminDeactivatePromotionLevelValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminDeactivatePromotionLevelValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivatePromotionLevelValidatorTests"/>.
    /// </summary>
    public AdminDeactivatePromotionLevelValidatorTests()
    {
        _validator = new AdminDeactivatePromotionLevelValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivatePromotionLevelCommand(Id: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldHaveError()
    {
        // Arrange
        var command = new AdminDeactivatePromotionLevelCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminDeactivatePromotionLevelCommand.Id)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
