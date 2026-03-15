using _116.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Queries.GetPackageById;

/// <summary>
/// Unit tests for <see cref="AdminGetPackageByIdValidator"/>.
/// </summary>
public class AdminGetPackageByIdValidatorTests
{
    private readonly AdminGetPackageByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        var query = new AdminGetPackageByIdQuery(Id: Guid.NewGuid().ToString());
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        var query = new AdminGetPackageByIdQuery(Id: "");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPackageByIdQuery.Id) && e.ErrorMessage == "Package ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidFormat_ShouldHaveError()
    {
        var query = new AdminGetPackageByIdQuery(Id: "not-a-guid");
        ValidationResult result = await _validator.ValidateAsync(query);
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminGetPackageByIdQuery.Id) && e.ErrorMessage == "Package ID is invalid."
            );
    }

    #endregion
}
