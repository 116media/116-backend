using _116.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.RemoveCategoryPricing;

/// <summary>
/// Unit tests for <see cref="AdminRemoveCategoryPricingValidator"/>.
/// </summary>
public class AdminRemoveCategoryPricingValidatorTests
{
    private readonly AdminRemoveCategoryPricingValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        var command = new AdminRemoveCategoryPricingCommand(
            CategoryId: Guid.NewGuid().ToString(),
            PricingTierId: Guid.NewGuid()
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
        var command = new AdminRemoveCategoryPricingCommand(CategoryId: "", PricingTierId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == "Category ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidCategoryIdFormat_ShouldHaveError()
    {
        var command = new AdminRemoveCategoryPricingCommand(CategoryId: "not-a-guid", PricingTierId: Guid.NewGuid());
        ValidationResult result = await _validator.ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminRemoveCategoryPricingCommand.CategoryId)
                && e.ErrorMessage == "Category ID is invalid."
            );
    }

    #endregion
}
