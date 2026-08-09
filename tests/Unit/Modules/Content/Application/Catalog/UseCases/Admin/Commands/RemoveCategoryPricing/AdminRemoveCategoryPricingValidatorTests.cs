using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminRemoveCategoryPricingValidator"/>.
/// </summary>
public class AdminRemoveCategoryPricingValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminRemoveCategoryPricingValidator _validator;

    public AdminRemoveCategoryPricingValidatorTests()
    {
        _validator = new AdminRemoveCategoryPricingValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid().ToString()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region CategoryId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyCategoryId_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(CategoryId: "", PricingTierId: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == _i18n.Category.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidCategoryIdFormat_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: "not-a-guid",
            PricingTierId: Guid.NewGuid().ToString()
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == _i18n.Category.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion

    #region PricingTierId Validation Tests

    [Fact]
    public async Task Validate_WithNullPricingTierId_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: null!
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.PricingTierId)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithEmptyPricingTierId_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: string.Empty
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.PricingTierId)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    [Fact]
    public async Task Validate_WithInvalidPricingTierIdFormat_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: "not-a-guid"
        );
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.PricingTierId)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdInvalid"].Value
            );
    }

    #endregion
}
