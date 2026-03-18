using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierValidator"/>.
/// </summary>
public class AdminAddItemTierValidatorTests
{
    private readonly AdminAddItemTierValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region OrderId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddItemTierCommand(
            OrderId: "not-a-guid",
            OrderItemId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddItemTierCommand.OrderId) && e.ErrorMessage == "Order ID is invalid."
            );
    }

    #endregion

    #region OrderItemId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidOrderItemId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: "not-a-guid",
            PricingTierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddItemTierCommand.OrderItemId)
                && e.ErrorMessage == "Order item ID is invalid."
            );
    }

    #endregion

    #region PricingTierId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidPricingTierId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            OrderItemId: Guid.NewGuid().ToString(),
            PricingTierId: "not-a-guid"
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddItemTierCommand.PricingTierId)
                && e.ErrorMessage == "Pricing tier ID is invalid."
            );
    }

    #endregion
}
