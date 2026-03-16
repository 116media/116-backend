using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePromotionLevelValidator"/>.
/// </summary>
public class AdminDeactivatePromotionLevelValidatorTests
{
    private readonly AdminDeactivatePromotionLevelValidator _validator = new();

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
                && e.ErrorMessage == "Promotion level ID is required."
            );
    }

    #endregion
}
