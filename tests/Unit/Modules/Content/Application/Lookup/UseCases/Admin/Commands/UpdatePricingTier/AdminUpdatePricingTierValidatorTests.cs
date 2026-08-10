using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePricingTierValidator"/>.
/// </summary>
public class AdminUpdatePricingTierValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUpdatePricingTierValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdatePricingTierValidatorTests"/>.
    /// </summary>
    public AdminUpdatePricingTierValidatorTests()
    {
        _validator = new AdminUpdatePricingTierValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidIdAndName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PricingTier.ValidName,
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidIdNameAndDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PricingTier.ValidName,
            Description: TestConstants.PricingTier.ValidDescription
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
        var command = new AdminUpdatePricingTierCommand(
            Id: "",
            Name: TestConstants.PricingTier.ValidName,
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Id)
                && e.ErrorMessage == _i18n.PricingTier.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: string.Empty,
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: new string('a', TestConstants.PricingTier.NameMaxLength + 1),
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameTooLong(TestConstants.PricingTier.NameMaxLength)
            );
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithEmptyIdAndName_ShouldHaveMultipleErrors()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: "",
            Name: string.Empty,
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePricingTierCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdatePricingTierCommand.Name));
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PricingTier.ValidName,
            Description: null!
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.PricingTier.Msg.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PricingTier.ValidName,
            Description: string.Empty
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.PricingTier.Msg.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePricingTierCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PricingTier.ValidName,
            Description: new string('a', TestConstants.PricingTier.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminUpdatePricingTierCommand.Description)
                && e.ErrorMessage
                    == _i18n.PricingTier.Msg.DescriptionTooLong(TestConstants.PricingTier.DescriptionMaxLength)
            );
    }

    #endregion
}
