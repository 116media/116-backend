using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Unit tests for <see cref="AdminCreatePricingTierValidator"/>.
/// </summary>
public class AdminCreatePricingTierValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreatePricingTierValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePricingTierValidatorTests"/>.
    /// </summary>
    public AdminCreatePricingTierValidatorTests()
    {
        _validator = new AdminCreatePricingTierValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidNameAndDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
    public async Task Validate_WithMaxLengthName_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: new string('a', TestConstants.PricingTier.NameMaxLength),
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithMaxLengthDescription_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.PricingTier.ValidName,
            Description: new string('a', TestConstants.PricingTier.DescriptionMaxLength)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: null!,
            Description: TestConstants.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.PricingTier.Msg.NameTooLong(TestConstants.PricingTier.NameMaxLength)
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(Name: TestConstants.PricingTier.ValidName, Description: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.PricingTier.Msg.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.PricingTier.Msg.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
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
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage
                    == _i18n.PricingTier.Msg.DescriptionTooLong(TestConstants.PricingTier.DescriptionMaxLength)
            );
    }

    #endregion
}
