using _116.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Queries.GetVideoById;

/// <summary>
/// Unit tests for <see cref="AdminGetVideoByIdValidator"/>.
/// </summary>
public class AdminGetVideoByIdValidatorTests
{
    private readonly AdminGetVideoByIdValidator _validator = new();

    #region Valid Query Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var query = new AdminGetVideoByIdQuery(Id: Guid.NewGuid().ToString());

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
        var query = new AdminGetVideoByIdQuery(Id: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminGetVideoByIdQuery.Id) && e.ErrorMessage == "Video ID is required."
            );
    }

    [Fact]
    public async Task Validate_WithInvalidGuidId_ShouldHaveError()
    {
        // Arrange
        var query = new AdminGetVideoByIdQuery(Id: "not-a-guid");

        // Act
        ValidationResult result = await _validator.ValidateAsync(query);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminGetVideoByIdQuery.Id) && e.ErrorMessage == "Video ID is invalid."
            );
    }

    #endregion
}
