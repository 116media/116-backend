using _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Unit tests for <see cref="CreatePromotionLevelValidator"/>.
/// </summary>
public class CreatePromotionLevelValidatorTests
{
    private readonly CreatePromotionLevelValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreatePromotionLevelCommand(
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd
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
        var command = new CreatePromotionLevelCommand(
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ZeroPriceUsd
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
        var command = new CreatePromotionLevelCommand(
            Name: string.Empty,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreatePromotionLevelCommand.Name)
                && e.ErrorMessage == "Promotion level name is required."
            );
    }

    [Fact]
    public async Task Validate_WithNameExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePromotionLevelCommand(
            Name: new string('a', TestConstants.Content.PromotionLevel.NameMaxLength + 1),
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreatePromotionLevelCommand.Name)
                && e.ErrorMessage == "Promotion level name must not exceed 40 characters."
            );
    }

    #endregion

    #region DurationDays Validation Tests

    [Fact]
    public async Task Validate_WithZeroDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePromotionLevelCommand(
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: 0,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == "Promotion level duration must be greater than zero."
            );
    }

    [Fact]
    public async Task Validate_WithNegativeDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePromotionLevelCommand(
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: -1,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreatePromotionLevelCommand.DurationDays)
                && e.ErrorMessage == "Promotion level duration must be greater than zero."
            );
    }

    #endregion

    #region PriceUsd Validation Tests

    [Fact]
    public async Task Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var command = new CreatePromotionLevelCommand(
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: -0.01m
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .ContainSingle(e =>
                e.PropertyName == nameof(CreatePromotionLevelCommand.PriceUsd)
                && e.ErrorMessage == "Promotion level price must be zero or greater."
            );
    }

    #endregion
}
