using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="AdminUpdatePromotionLevelValidator"/>.
/// </summary>
public class AdminUpdatePromotionLevelValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUpdatePromotionLevelValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="AdminUpdatePromotionLevelValidatorTests"/>.
    /// </summary>
    public AdminUpdatePromotionLevelValidatorTests()
    {
        _validator = new AdminUpdatePromotionLevelValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: TestConstants.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.PromotionLevel.ZeroPriceUsd,
            SpotPriority: 2
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Id Validation Tests

    [Fact]
    public async Task Validate_WithEmptyId_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: "",
            Name: TestConstants.PromotionLevel.ValidName,
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.Id)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.Localizer["IdRequired"].Value
            );
    }

    #endregion

    #region Name Validation Tests

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.Name)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.NameRequired()
            );
    }

    #endregion

    #region DurationDays Validation Tests

    [Fact]
    public async Task Validate_WithZeroDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.DurationMustBePositive()
            );
    }

    [Fact]
    public async Task Validate_WithNegativeDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.PromotionLevel.ValidName,
            DurationDays: -5,
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == _i18n.PromotionLevel.Msg.DurationMustBePositive()
            );
    }

    #endregion

    #region PriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.PriceUsd)
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
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
                e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.SpotPriority)
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
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
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
            .NotContain(e => e.PropertyName == nameof(AdminUpdatePromotionLevelCommand.SpotPriority));
    }

    #endregion
}
