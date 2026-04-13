using _116.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.EditOrder;

/// <summary>
/// Unit tests for <see cref="AdminEditOrderValidator"/>.
/// </summary>
public class AdminEditOrderValidatorTests
{
    private readonly AdminEditOrderValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidDataAndCustomerId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminEditOrderCommand(
            OrderId: Guid.NewGuid().ToString(),
            CustomerId: Guid.NewGuid().ToString(),
            PackageId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullCustomerId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminEditOrderCommand(OrderId: Guid.NewGuid().ToString(), CustomerId: null, PackageId: null);

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
        var command = new AdminEditOrderCommand(OrderId: "not-a-guid", CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminEditOrderCommand.OrderId) && e.ErrorMessage == "Order ID is invalid."
            );
    }

    [Fact]
    public async Task Validate_WithEmptyOrderId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderCommand(OrderId: "", CustomerId: null, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithInvalidGuidCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminEditOrderCommand(
            OrderId: Guid.NewGuid().ToString(),
            CustomerId: "not-a-guid",
            PackageId: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminEditOrderCommand.CustomerId)
                && e.ErrorMessage == "Customer ID is invalid."
            );
    }

    #endregion
}
