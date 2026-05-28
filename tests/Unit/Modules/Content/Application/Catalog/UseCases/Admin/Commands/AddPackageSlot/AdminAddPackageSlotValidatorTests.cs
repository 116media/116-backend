using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddPackageSlot;

/// <summary>
/// Unit tests for <see cref="AdminAddPackageSlotValidator"/>.
/// </summary>
public class AdminAddPackageSlotValidatorTests
{
    private readonly PackageErrorMessage _i18n = LocalizerFactory.CreateMessage<PackageErrorMessage>();
    private readonly AdminAddPackageSlotValidator _validator;

    public AdminAddPackageSlotValidatorTests()
    {
        _validator = new AdminAddPackageSlotValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAddPackageSlotCommand(
            PackageId: Guid.NewGuid().ToString(),
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
        var command = new AdminAddPackageSlotCommand(
            PackageId: Guid.NewGuid().ToString(),
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
        var command = new AdminAddPackageSlotCommand(
            PackageId: "",
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
                e.PropertyName == nameof(AdminAddPackageSlotCommand.PackageId)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region Quantity Validation Tests

    [Fact]
    public async Task Validate_WithZeroQuantity_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddPackageSlotCommand(
            PackageId: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminAddPackageSlotCommand.Quantity)
                && e.ErrorMessage == _i18n.SlotQuantityMustBePositive()
            );
    }

    [Fact]
    public async Task Validate_WithNegativeQuantity_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddPackageSlotCommand(
            PackageId: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminAddPackageSlotCommand.Quantity)
                && e.ErrorMessage == _i18n.SlotQuantityMustBePositive()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<PackageErrorMessage>(culture);
        var validator = new AdminAddPackageSlotValidator(i18n);
        var command = new AdminAddPackageSlotCommand(
            PackageId: "",
            CategoryId: null,
            IsRequired: false,
            Quantity: TestConstants.Content.PackageSlot.ValidQuantity
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddPackageSlotCommand.PackageId)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
