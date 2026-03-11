using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Unit tests for <see cref="UpdateCategoryValidator"/>.
/// </summary>
public class UpdateCategoryValidatorTests
{
    private readonly UpdateCategoryValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription
        );

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
        var command = new UpdateCategoryCommand(
            Id: "",
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateCategoryCommand.Id) && e.ErrorMessage == "Category ID is required."
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateCategoryCommand.Name) && e.ErrorMessage == "Category name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: new string('a', TestConstants.Content.Category.NameMaxLength + 1),
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateCategoryCommand.Name)
                && e.ErrorMessage == "Category name must not exceed 60 characters."
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithInvalidSlugFormat_ShouldHaveError()
    {
        // Arrange
        var command = new UpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: "Invalid Slug",
            Description: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(UpdateCategoryCommand.Slug)
                && e.ErrorMessage == "Category slug must be lowercase and contain only letters, numbers, and hyphens."
            );
    }

    #endregion
}
