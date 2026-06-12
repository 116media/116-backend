using _116.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Catalog.UseCases.Admin.Commands.DeactivateCategory;

/// <summary>
/// Unit tests for <see cref="AdminDeactivateCategoryValidator"/>.
/// </summary>
public class AdminDeactivateCategoryValidatorTests
{
    private readonly CategoryErrorMessage _i18n = LocalizerFactory.CreateMessage<CategoryErrorMessage>();
    private readonly AdminDeactivateCategoryValidator _validator;

    public AdminDeactivateCategoryValidatorTests()
    {
        _validator = new AdminDeactivateCategoryValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidId_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivateCategoryCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminDeactivateCategoryCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminDeactivateCategoryCommand.Id)
                && e.ErrorMessage == _i18n.Localizer["IdRequired"].Value
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
        var validator = new AdminDeactivateCategoryValidator(i18n);
        var command = new AdminDeactivateCategoryCommand(Id: "");

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminDeactivateCategoryCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
