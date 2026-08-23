using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCategoryPricingValidator"/>.
/// </summary>
public class AdminUpdateCategoryPricingValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUpdateCategoryPricingValidator _validator;

    public AdminUpdateCategoryPricingValidatorTests()
    {
        _validator = new AdminUpdateCategoryPricingValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString(),
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithZeroPriceUsd_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString(),
            PriceUsd: TestConstants.CategoryPricing.ZeroPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: "",
            PricingTierId: Guid.NewGuid().ToString(),
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == _i18n.Category.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region PricingTierId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyPricingTierId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: "",
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryPricingCommand.PricingTierId)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region PriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePriceUsd_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString(),
            PriceUsd: -5m
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryPricingCommand.PriceUsd)
                && e.ErrorMessage == _i18n.Category.Msg.PriceMustBeNonNegative()
            );
    }

    #endregion
}
