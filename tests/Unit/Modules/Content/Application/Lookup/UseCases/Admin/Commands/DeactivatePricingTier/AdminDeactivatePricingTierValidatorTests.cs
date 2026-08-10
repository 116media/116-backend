using _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;
using _116.Content.Application.Shared.Errors.Facade;
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
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
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
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
