using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCategoryById;

/// <summary>
/// Unit tests for <see cref="AdminGetCategoryByIdValidator"/>.
/// </summary>
public class AdminGetCategoryByIdValidatorTests
{
    private readonly AdminGetCategoryByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        var query = new AdminGetCategoryByIdQuery(Id: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        var query = new AdminGetCategoryByIdQuery(Id: "");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetCategoryByIdQuery.Id) && e.ErrorMessage == "Category ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        var query = new AdminGetCategoryByIdQuery(Id: "not-a-guid");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetCategoryByIdQuery.Id) && e.ErrorMessage == "Category ID is invalid."
            );
    }

    #endregion
}
