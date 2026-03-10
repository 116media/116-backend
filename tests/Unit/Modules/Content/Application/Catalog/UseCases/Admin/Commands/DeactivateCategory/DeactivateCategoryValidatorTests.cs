using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Unit tests for <see cref="DeactivateCategoryValidator"/>.
/// </summary>
public class DeactivateCategoryValidatorTests
{
    private readonly DeactivateCategoryValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new DeactivateCategoryCommand(Id: Guid.NewGuid());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

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
        var command = new DeactivateCategoryCommand(Id: Guid.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(DeactivateCategoryCommand.Id) && e.ErrorMessage == "Category ID is required."
            );
    }

    #endregion
}
