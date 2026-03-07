using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Unit tests for <see cref="AddPackageSlotValidator"/>.
/// </summary>
public class AddPackageSlotValidatorTests
{
    private readonly AddPackageSlotValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AddPackageSlotCommand(
            PackageId: Guid.NewGuid(),
            CategoryId: Guid.NewGuid(),
            IsRequired: true,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNullCategoryId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AddPackageSlotCommand(
            PackageId: Guid.NewGuid(),
            CategoryId: null,
            IsRequired: false,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region PackageId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyPackageId_ShouldHaveError()
    {
        // Arrange
        var command = new AddPackageSlotCommand(
            PackageId: Guid.Empty,
            CategoryId: null,
            IsRequired: false,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AddPackageSlotCommand.PackageId) && e.ErrorMessage == "Package ID is required."
            );
    }

    #endregion

    #region Quantity Validation Tests

    [Fact]
    public async Task Validate_WithZeroQuantity_ShouldHaveError()
    {
        // Arrange
        var command = new AddPackageSlotCommand(
            PackageId: Guid.NewGuid(),
            CategoryId: null,
            IsRequired: false,
            Quantity: 0
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AddPackageSlotCommand.Quantity)
                && e.ErrorMessage == "Slot quantity must be greater than zero."
            );
    }

    [Fact]
    public async Task Validate_WithNegativeQuantity_ShouldHaveError()
    {
        // Arrange
        var command = new AddPackageSlotCommand(
            PackageId: Guid.NewGuid(),
            CategoryId: null,
            IsRequired: false,
            Quantity: -1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AddPackageSlotCommand.Quantity)
                && e.ErrorMessage == "Slot quantity must be greater than zero."
            );
    }

    #endregion
}
