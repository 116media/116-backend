using _116.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RateVideo;

/// <summary>
/// Unit tests for <see cref="PublicRateVideoValidator"/>.
/// </summary>
public class PublicRateVideoValidatorTests
{
    private readonly PublicRateVideoValidator _validator = new();

    #region Valid Command Tests

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task Validate_WhenStarsIsValid_ShouldPass(short stars)
    {
        // Arrange
        var command = new PublicRateVideoCommand(VideoId: Guid.NewGuid(), UserId: Guid.NewGuid(), Stars: stars);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Stars Validation Tests

    [Fact]
    public async Task Validate_WhenStarsIsBelowMinimum_ShouldFail()
    {
        // Arrange
        var command = new PublicRateVideoCommand(
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Stars: TestConstants.Content.Interactions.InvalidStarRatingBelowMin
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRateVideoCommand.Stars));
    }

    [Fact]
    public async Task Validate_WhenStarsIsAboveMaximum_ShouldFail()
    {
        // Arrange
        var command = new PublicRateVideoCommand(
            VideoId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Stars: TestConstants.Content.Interactions.InvalidStarRatingAboveMax
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRateVideoCommand.Stars));
    }

    #endregion
}
