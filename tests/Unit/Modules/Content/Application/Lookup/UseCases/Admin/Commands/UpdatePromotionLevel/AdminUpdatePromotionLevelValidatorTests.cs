using _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;
using _116.Content.Application.Shared.Errors.Messages;
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
    private readonly AdminUpdatePromotionLevelValidator _validator = new(
        LocalizerFactory.CreateMessage<PromotionLevelErrorMessage>()
    );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidValues_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ZeroPriceUsd,
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
                && e.ErrorMessage == "Promotion level ID is required."
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
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
                && e.ErrorMessage == "Promotion level name is required."
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: 0,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
                && e.ErrorMessage == "Promotion level duration must be greater than zero."
            );
    }

    [Fact]
    public async Task Validate_WithNegativeDurationDays_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdatePromotionLevelCommand(
            Id: Guid.NewGuid().ToString(),
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: -5,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
                && e.ErrorMessage == "Promotion level duration must be greater than zero."
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
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
                && e.ErrorMessage == "Promotion level price must be zero or greater."
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
                && e.ErrorMessage == "Spot priority must be 1, 2, or 3."
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
            Name: TestConstants.Content.PromotionLevel.ValidName,
            DurationDays: TestConstants.Content.PromotionLevel.ValidDurationDays,
            PriceUsd: TestConstants.Content.PromotionLevel.ValidPriceUsd,
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
