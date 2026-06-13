using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveItemTier;

/// <summary>
/// Unit tests for <see cref="AdminRemoveItemTierValidator"/>.
/// </summary>
public class AdminRemoveItemTierValidatorTests
{
    private readonly AdminRemoveItemTierValidator _validator = new(TestErrorsFactory.CreateContentI18n());

    #region Validate

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminRemoveItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithInvalidGuidOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRemoveItemTierCommand(
            OrderId: "not-a-guid",
            ItemId: Guid.NewGuid().ToString(),
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OrderId");
    }

    [Fact]
    public void Validate_WithInvalidGuidItemId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRemoveItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: "not-a-guid",
            TierId: Guid.NewGuid().ToString()
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ItemId");
    }

    [Fact]
    public void Validate_WithInvalidGuidTierId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminRemoveItemTierCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: Guid.NewGuid().ToString(),
            TierId: "not-a-guid"
        );

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TierId");
    }

    #endregion
}
