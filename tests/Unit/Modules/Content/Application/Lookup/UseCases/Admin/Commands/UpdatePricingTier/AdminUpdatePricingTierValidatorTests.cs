using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePricingTierValidator"/>.
/// </summary>
public class AdminUpdatePricingTierValidatorTests
{
    private readonly AdminUpdatePricingTierValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidIdAndName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidIdNameAndDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: TestConstants.Content.PricingTier.ValidDescription
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
        var command = new AdminUpdatePricingTierCommand(
            Id: "",
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Id)
                && e.ErrorMessage == "Pricing tier ID is required."
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name)
                && e.ErrorMessage == "Pricing tier name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: new string('a', TestConstants.Content.PricingTier.NameMaxLength + 1),
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name)
                && e.ErrorMessage == "Pricing tier name must not exceed 40 characters."
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithEmptyIdAndName_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(Id: "", Name: string.Empty, Description: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePricingTierCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name));
    }

    #endregion
}
