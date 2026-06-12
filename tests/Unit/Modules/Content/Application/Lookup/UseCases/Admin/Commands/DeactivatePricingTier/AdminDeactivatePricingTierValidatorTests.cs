using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;
using _116.Content.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminDeactivatePricingTierValidator"/>.
/// </summary>
public class AdminDeactivatePricingTierValidatorTests
{
    private readonly PricingTierErrorMessage _i18n = LocalizerFactory.CreateMessage<PricingTierErrorMessage>();
    private readonly AdminDeactivatePricingTierValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminDeactivatePricingTierValidatorTests"/>.
    /// </summary>
    public AdminDeactivatePricingTierValidatorTests()
    {
        _validator = new AdminDeactivatePricingTierValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminDeactivatePricingTierCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminDeactivatePricingTierCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminDeactivatePricingTierCommand.Id)
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
        var i18n = LocalizerFactory.CreateMessage<PricingTierErrorMessage>(culture);
        var validator = new AdminDeactivatePricingTierValidator(i18n);
        var command = new AdminDeactivatePricingTierCommand(Id: "");

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminDeactivatePricingTierCommand.Id)
                && e.ErrorMessage == i18n.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
