using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminActivatePromotionLevelValidator"/>.
/// </summary>
public class AdminActivatePromotionLevelValidatorTests
{
    private readonly PromotionLevelErrorMessage _i18n = LocalizerFactory.CreateMessage<PromotionLevelErrorMessage>();
    private readonly AdminActivatePromotionLevelValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivatePromotionLevelValidatorTests"/>.
    /// </summary>
    public AdminActivatePromotionLevelValidatorTests()
    {
        _validator = new AdminActivatePromotionLevelValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminActivatePromotionLevelCommand(Id: Guid.NewGuid().ToString());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyGuid_ShouldHaveError()
    {
        // Arrange
        var command = new AdminActivatePromotionLevelCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivatePromotionLevelCommand.Id)
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
        var i18n = LocalizerFactory.CreateMessage<PromotionLevelErrorMessage>(culture);
        var validator = new AdminActivatePromotionLevelValidator(i18n);
        var command = new AdminActivatePromotionLevelCommand(Id: "");

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminActivatePromotionLevelCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
