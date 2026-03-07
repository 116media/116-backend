using _116.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.ActivateCategory;

/// <summary>
/// Unit tests for <see cref="ActivateCategoryValidator"/>.
/// </summary>
public class ActivateCategoryValidatorTests
{
    private readonly ActivateCategoryValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new ActivateCategoryCommand(Id: Guid.NewGuid());

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
        var command = new ActivateCategoryCommand(Id: Guid.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(ActivateCategoryCommand.Id) && e.ErrorMessage == "Category ID is required."
            );
    }

    #endregion
}
