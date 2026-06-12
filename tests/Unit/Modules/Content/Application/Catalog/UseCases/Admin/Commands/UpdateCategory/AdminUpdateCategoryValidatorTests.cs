using _116.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.UpdateCategory;

/// <summary>
/// Unit tests for <see cref="AdminUpdateCategoryValidator"/>.
/// </summary>
public class AdminUpdateCategoryValidatorTests
{
    private readonly CategoryErrorMessage _i18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>();
    private readonly AdminUpdateCategoryValidator _validator;

    public AdminUpdateCategoryValidatorTests()
    {
        _validator = new AdminUpdateCategoryValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
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
        var command = new AdminUpdateCategoryCommand(
            Id: "",
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: new string('a', TestConstants.Content.Category.NameMaxLength + 1),
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.NameTooLong(TestConstants.Content.Category.NameMaxLength)
            );
    }

    #endregion

    #region Slug Validation Tests

    [Fact]
    public async Task Validate_WithInvalidSlugFormat_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateCategoryCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.Category.ValidName,
            Slug: "Invalid Slug",
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryCommand.Slug) && e.ErrorMessage == _i18n.SlugInvalidFormat()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>(culture);
        var validator = new AdminUpdateCategoryValidator(i18n);
        var command = new AdminUpdateCategoryCommand(
            Id: "",
            Name: TestConstants.Content.Category.ValidName,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsGossip: false
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateCategoryCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
