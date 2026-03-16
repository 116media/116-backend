using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePricingTierValidator"/>.
/// </summary>
public class AdminDeactivatePricingTierValidatorTests
{
    private readonly AdminDeactivatePricingTierValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivatePricingTierCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminDeactivatePricingTierCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminDeactivatePricingTierCommand.Id)
                && e.ErrorMessage == "Pricing tier ID is required."
            );
    }

    #endregion
}
