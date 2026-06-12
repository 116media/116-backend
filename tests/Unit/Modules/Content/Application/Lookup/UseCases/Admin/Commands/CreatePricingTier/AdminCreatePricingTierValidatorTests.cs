using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly PricingTierErrorMessage _i18n = LocalizerFactory.CreateMessage<PricingTierErrorMessage>();
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
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: TestConstants.Content.PricingTier.ValidDescription
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
            Name: new string('a', TestConstants.Content.PricingTier.NameMaxLength),
            Description: TestConstants.Content.PricingTier.ValidDescription
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
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: new string('a', TestConstants.Content.PricingTier.DescriptionMaxLength)
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
            Description: TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: null!,
            Description: TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name) && e.ErrorMessage == _i18n.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: new string('a', TestConstants.Content.PricingTier.NameMaxLength + 1),
            Description: TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name)
                && e.ErrorMessage == _i18n.NameTooLong(TestConstants.Content.PricingTier.NameMaxLength)
            );
    }

    #endregion

    #region Description Validation Tests

    [Fact]
    public async Task Validate_WithNullDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: null!
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithEmptyDescription_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
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
                && e.ErrorMessage == _i18n.DescriptionRequired()
            );
    }

    [Fact]
    public async Task Validate_WithDescriptionExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePricingTierCommand(
            Name: TestConstants.Content.PricingTier.ValidName,
            Description: new string('a', TestConstants.Content.PricingTier.DescriptionMaxLength + 1)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Description)
                && e.ErrorMessage == _i18n.DescriptionTooLong(TestConstants.Content.PricingTier.DescriptionMaxLength)
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
        var validator = new AdminCreatePricingTierValidator(i18n);
        var command = new AdminCreatePricingTierCommand(
            Name: "",
            Description: TestConstants.Content.PricingTier.ValidDescription
        );

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePricingTierCommand.Name) && e.ErrorMessage == i18n.NameRequired()
            );
    }

    #endregion
}
