using _116.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.RemoveOrderItem;

/// <summary>
/// Unit tests for <see cref="AdminRemoveOrderItemValidator"/>.
/// </summary>
public class AdminRemoveOrderItemValidatorTests
{
    private readonly AdminRemoveOrderItemValidator _validator = new();

    #region Validate

    [Fact]
    public void Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminRemoveOrderItemCommand(
            OrderId: Guid.NewGuid().ToString(),
            ItemId: Guid.NewGuid().ToString()
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
        var command = new AdminRemoveOrderItemCommand(OrderId: "not-a-guid", ItemId: Guid.NewGuid().ToString());

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
        var command = new AdminRemoveOrderItemCommand(OrderId: Guid.NewGuid().ToString(), ItemId: "not-a-guid");

        // Act
        ValidationResult result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ItemId");
    }

    #endregion
}
