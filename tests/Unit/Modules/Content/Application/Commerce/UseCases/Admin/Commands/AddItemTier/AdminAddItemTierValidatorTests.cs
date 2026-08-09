using _116.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.AddItemTier;

/// <summary>
/// Unit tests for <see cref="AdminAddItemTierValidator"/>.
/// </summary>
public class AdminAddItemTierValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminAddItemTierValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminAddItemTierValidatorTests"/>.
    /// </summary>
    public AdminAddItemTierValidatorTests()
    {
        _validator = new AdminAddItemTierValidator(_i18n);
    }

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
                e.PropertyName == nameof(AdminAddItemTierCommand.OrderId)
                && e.ErrorMessage == _i18n.ContentOrder.Msg.Localizer["IdInvalid"].Value
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
                && e.ErrorMessage == _i18n.ContentOrder.Msg.Localizer["OrderItemIdInvalid"].Value
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
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
