using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminCreatePromotionLevelValidator"/>.
/// </summary>
public class AdminCreatePromotionLevelValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreatePromotionLevelValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminCreatePromotionLevelValidatorTests"/>.
    /// </summary>
    public AdminCreatePromotionLevelValidatorTests()
    {
        _validator = new AdminCreatePromotionLevelValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithZeroPrice_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ZeroPriceUsd,
            SpotPriority: 1
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
        var command = new AdminCreatePromotionLevelCommand(
            Name: string.Empty,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.Name)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: new string('a', TestConstants.PromotionLevel.NameMaxLength + 1),
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.Name)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.NameTooLong(TestConstants.PromotionLevel.NameMaxLength)
            );
    }

    #endregion

    #region DurationDays Validation Tests

    [Fact]
    public async Task Validate_WithZeroDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: 0,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.DurationMustBePositive()
            );
    }

    [Fact]
    public async Task Validate_WithNegativeDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: -1,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.DurationMustBePositive()
            );
    }

    #endregion

    #region PriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: -0.01m,
            SpotPriority: 1
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.PriceUsd)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.PriceMustBeNonNegative()
            );
    }

    #endregion

    #region SpotPriority Validation Tests

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public async Task Validate_WithInvalidSpotPriority_ShouldHaveError(int spotPriority)
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: spotPriority
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreatePromotionLevelCommand.SpotPriority)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.InvalidSpotPriority()
            );
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task Validate_WithValidSpotPriority_ShouldNotHaveError(int spotPriority)
    {
        // Arrange
        var command = new AdminCreatePromotionLevelCommand(
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ValidPriceUsd,
            SpotPriority: spotPriority
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result
            .Errors.Should()
            .NotContain(e => e.PropertyName == nameof(AdminCreatePromotionLevelCommand.SpotPriority));
    }

    #endregion
}
