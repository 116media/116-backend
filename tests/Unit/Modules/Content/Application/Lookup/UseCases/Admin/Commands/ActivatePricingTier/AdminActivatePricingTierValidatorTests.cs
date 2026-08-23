using _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminActivatePricingTierValidator"/>.
/// </summary>
public class AdminActivatePricingTierValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminActivatePricingTierValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminActivatePricingTierValidatorTests"/>.
    /// </summary>
    public AdminActivatePricingTierValidatorTests()
    {
        _validator = new AdminActivatePricingTierValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidGuid_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminActivatePricingTierCommand(Id: Guid.NewGuid().ToString());

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
        var command = new AdminActivatePricingTierCommand(Id: "");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminActivatePricingTierCommand.Id)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion
}
