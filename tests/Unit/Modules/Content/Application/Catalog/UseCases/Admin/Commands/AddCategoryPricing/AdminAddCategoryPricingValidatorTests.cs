using _116.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.AddCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminAddCategoryPricingValidator"/>.
/// </summary>
public class AdminAddCategoryPricingValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminAddCategoryPricingValidator _validator;

    public AdminAddCategoryPricingValidatorTests()
    {
        _validator = new AdminAddCategoryPricingValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminAddCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid(),
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
        var command = new AdminAddCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid(),
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
        var command = new AdminAddCategoryPricingCommand(
            CategoryId: "",
            PricingTierId: Guid.NewGuid(),
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == _i18n.Category.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region PricingTierId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyPricingTierId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.Empty,
            PriceUsd: TestConstants.CategoryPricing.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddCategoryPricingCommand.PricingTierId)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameRequired()
            );
    }

    #endregion

    #region PriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePriceUsd_ShouldHaveError()
    {
        // Arrange
        var command = new AdminAddCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid(),
            PriceUsd: -1m
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminAddCategoryPricingCommand.PriceUsd)
                && e.ErrorMessage == _i18n.Category.Msg.PriceMustBeNonNegative()
            );
    }

    #endregion
}
