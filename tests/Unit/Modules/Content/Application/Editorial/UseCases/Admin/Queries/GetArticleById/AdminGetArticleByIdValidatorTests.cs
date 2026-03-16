using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetArticleById;

/// <summary>
/// Unit tests for <see cref="AdminGetArticleByIdValidator"/>.
/// </summary>
public class AdminGetArticleByIdValidatorTests
{
    private readonly AdminGetArticleByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new AdminGetArticleByIdQuery(Id: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var query = new AdminGetArticleByIdQuery(Id: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminGetArticleByIdQuery.Id) && e.ErrorMessage == "Article ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var query = new AdminGetArticleByIdQuery(Id: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminGetArticleByIdQuery.Id) && e.ErrorMessage == "Article ID is invalid."
            );
    }

    #endregion
}
