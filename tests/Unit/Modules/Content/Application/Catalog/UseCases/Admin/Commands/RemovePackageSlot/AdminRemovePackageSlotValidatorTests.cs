using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemovePackageSlot;

/// <summary>
/// Unit tests for <see cref="AdminRemovePackageSlotValidator"/>.
/// </summary>
public class AdminRemovePackageSlotValidatorTests
{
    private readonly AdminRemovePackageSlotValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        var command = new AdminRemovePackageSlotCommand(
            PackageId: Guid.NewGuid().ToString(),
            SlotId: Guid.NewGuid().ToString()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region PackageId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyPackageId_ShouldHaveError()
    {
        var command = new AdminRemovePackageSlotCommand(PackageId: "", SlotId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemovePackageSlotCommand.PackageId)
                && e.ErrorMessage == "Package ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidPackageIdFormat_ShouldHaveError()
    {
        var command = new AdminRemovePackageSlotCommand(PackageId: "not-a-guid", SlotId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemovePackageSlotCommand.PackageId)
                && e.ErrorMessage == "Package ID is invalid."
            );
    }

    #endregion

    #region SlotId Validation Tests

    [Fact]
    public async Task Validate_WithNullSlotId_ShouldHaveError()
    {
        var command = new AdminRemovePackageSlotCommand(PackageId: Guid.NewGuid().ToString(), SlotId: null!);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemovePackageSlotCommand.SlotId)
                && e.ErrorMessage == "Slot ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithEmptySlotId_ShouldHaveError()
    {
        var command = new AdminRemovePackageSlotCommand(PackageId: Guid.NewGuid().ToString(), SlotId: string.Empty);
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemovePackageSlotCommand.SlotId)
                && e.ErrorMessage == "Slot ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidSlotIdFormat_ShouldHaveError()
    {
        var command = new AdminRemovePackageSlotCommand(PackageId: Guid.NewGuid().ToString(), SlotId: "not-a-guid");
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemovePackageSlotCommand.SlotId)
                && e.ErrorMessage == "Slot ID is invalid."
            );
    }

    #endregion
}
