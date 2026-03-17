using _116.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Commerce.UseCases.Admin.Commands.CreateOrder;

/// <summary>
/// Unit tests for <see cref="AdminCreateOrderValidator"/>.
/// </summary>
public class AdminCreateOrderValidatorTests
{
    private readonly AdminCreateOrderValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: Guid.NewGuid().ToString(), PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region CustomerId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: string.Empty, PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateOrderCommand.CustomerId)
                && e.ErrorMessage == "Customer ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidCustomerId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateOrderCommand(CustomerId: "not-a-guid", PackageId: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateOrderCommand.CustomerId)
                && e.ErrorMessage == "Customer ID is invalid."
            );
    }

    #endregion
}
