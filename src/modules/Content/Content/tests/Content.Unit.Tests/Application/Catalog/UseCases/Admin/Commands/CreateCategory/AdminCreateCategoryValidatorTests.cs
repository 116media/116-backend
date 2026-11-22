using _116.Content.Application.Catalog.UseCases.Admin.Commands.CreateCategory;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.TestData.Constants;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Catalog.UseCases.Admin.Commands.CreateCategory;

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
            Name: TestConstants.Category.ValidName,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
    #endregion

    #region ContentTypeId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyContentTypeId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: string.Empty,
            Name: TestConstants.Category.ValidName,
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
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
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
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
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
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
            Name: new string('a', TestConstants.Category.NameMaxLength + 1),
            Description: TestConstants.Category.ValidDescription,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Name)
                && e.ErrorMessage == _i18n.Category.Msg.NameTooLong(TestConstants.Category.NameMaxLength)
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
            Name: TestConstants.Category.ValidName,
            Description: new string('d', TestConstants.Category.DescriptionMaxLength + 1),
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateCategoryCommand.Description)
                && e.ErrorMessage == _i18n.Category.Msg.DescriptionTooLong(TestConstants.Category.DescriptionMaxLength)
            );
    }

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateCategoryCommand(
            ContentTypeId: Guid.NewGuid().ToString(),
            Name: TestConstants.Category.ValidName,
            Description: null!,
            IsFree: false,
            IsGossip: false,
            IsExclusive: false
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateCategoryCommand.Description));
    }

    #endregion
}
