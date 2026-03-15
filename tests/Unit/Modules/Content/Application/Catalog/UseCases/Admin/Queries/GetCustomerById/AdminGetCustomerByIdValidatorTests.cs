using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetCustomerById;

/// <summary>
/// Unit tests for <see cref="AdminGetCustomerByIdValidator"/>.
/// </summary>
public class AdminGetCustomerByIdValidatorTests
{
    private readonly AdminGetCustomerByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        var query = new AdminGetCustomerByIdQuery(Id: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        var query = new AdminGetCustomerByIdQuery(Id: "");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetCustomerByIdQuery.Id) && e.ErrorMessage == "Customer ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        var query = new AdminGetCustomerByIdQuery(Id: "not-a-guid");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetCustomerByIdQuery.Id) && e.ErrorMessage == "Customer ID is invalid."
            );
    }

    #endregion
}
