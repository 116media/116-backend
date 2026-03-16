using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminCreatePricingTierValidator"/>.
/// </summary>
public class AdminCreatePricingTierValidatorTests
{
    private readonly AdminCreatePricingTierValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidNameAndNoDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
    public async Task Validate_WithValidNameAndDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithMaxLengthName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: new string('a', TestConstants.Content.PricingTier.NameMaxLength),
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: new string('a', TestConstants.Content.PricingTier.DescriptionMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(Name: string.Empty, Description: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == "Pricing tier name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(Name: null!, Description: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == "Pricing tier name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == "Pricing tier name must not exceed 40 characters."
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldBeValid()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: new string('a', TestConstants.Content.PricingTier.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage == "Pricing tier description must not exceed 200 characters."
            );
    }

    #endregion
}
