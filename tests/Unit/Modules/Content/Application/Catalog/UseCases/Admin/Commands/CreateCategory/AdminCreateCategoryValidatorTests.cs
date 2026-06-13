using System.Globalization;
using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

/// <summary>
/// Unit tests for <see cref="AdminCreateCategoryValidator"/>.
/// </summary>
public class AdminCreateCategoryValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateCategoryValidator _validator;

    public AdminCreateCategoryValidatorTests()
    {
        _validator = new AdminCreateCategoryValidator(_i18n);
    }

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
            IsFree: false,
            IsGossip: false
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
            IsFree: true,
            IsGossip: false
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.ContentTypeId)
                && e.ErrorMessage == _i18n.ContentType.Msg.Localizer["IdRequired"].Value
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.Category.Msg.NameRequired()
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.Category.Msg.NameRequired()
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.Category.Msg.NameTooLong(TestConstants.Content.Category.NameMaxLength)
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == _i18n.Category.Msg.SlugRequired()
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == _i18n.Category.Msg.SlugTooLong(TestConstants.Content.Category.SlugMaxLength)
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Slug)
                && e.ErrorMessage == _i18n.Category.Msg.SlugInvalidFormat()
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Description)
                && e.ErrorMessage
                    == _i18n.Category.Msg.DescriptionTooLong(TestConstants.Content.Category.DescriptionMaxLength)
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
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateCategoryCommand.Description));
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminCreateCategoryValidator(_i18n);
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Slug: TestConstants.Content.Category.ValidSlug,
            Description: TestConstants.Content.Category.ValidDescription,
            IsFree: false,
            IsGossip: false
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.Category.Msg.NameRequired()
            );
    }

    #endregion
}
