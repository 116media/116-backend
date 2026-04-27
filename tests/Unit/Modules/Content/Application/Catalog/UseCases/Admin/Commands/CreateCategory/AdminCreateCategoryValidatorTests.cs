using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryValidator"/>.
/// </summary>
public class AdminCreateCategoryValidatorTests
{
    private readonly AdminCreateCategoryValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidSlugFormat_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: "artist-profile",
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: true
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region ContentTypeId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyContentTypeId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: string.Empty,
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.ContentTypeId)
                && e.ErrorMessage == "Content type ID is required."
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == "Category name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: null!,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == "Category name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: new string('a', TestConstants.Content.Category.NameMaxLength + 1),
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == "Category name must not exceed 60 characters."
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: string.Empty,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == "Category slug is required."
            );
    }

    [Fact]
    public async Task Validate_WithSlugExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: new string('a', TestConstants.Content.Category.SlugMaxLength + 1),
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == "Category slug must not exceed 80 characters."
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: "Artist Profile",
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == "Category slug must be lowercase and contain only letters, numbers, and hyphens."
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: new string('d', TestConstants.Content.Category.DescriptionMaxLength + 1),
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Description)
                && e.ErrorMessage == "Category description must not exceed 300 characters."
            );
    }

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: null!,
            IsFree: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateCategoryCommand.Description));
    }

    #endregion
}
