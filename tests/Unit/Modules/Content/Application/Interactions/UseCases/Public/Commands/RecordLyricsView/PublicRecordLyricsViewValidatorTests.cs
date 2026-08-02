using _116.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Interactions.UseCases.Public.Commands.RecordLyricsView;

/// <summary>
/// Unit tests for <see cref="PublicRecordLyricsViewValidator"/>.
/// </summary>
public class PublicRecordLyricsViewValidatorTests
{
    private readonly PublicRecordLyricsViewValidator _validator = new();

    private static PublicRecordLyricsViewCommand BuildValidCommand(
        int dwellMs = 30_000,
        double scrollDepthRatio = 0.5
    ) =>
        new(
            LyricsId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            DeviceId: "device-116",
            IpAddress: "127.0.0.1",
            UserAgent: "unit-test-agent",
            DwellMs: dwellMs,
            ScrollDepthRatio: scrollDepthRatio
        );

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand();

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAnonymousViewer_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new PublicRecordLyricsViewCommand(
            LyricsId: Guid.NewGuid(),
            UserId: null,
            DeviceId: null,
            IpAddress: null,
            UserAgent: null,
            DwellMs: 1_000,
            ScrollDepthRatio: 0.25
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region DwellMs Validation Tests

    [Fact]
    public async Task Validate_WithZeroDwellMs_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(dwellMs: 0);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithNegativeDwellMs_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(dwellMs: -1);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRecordLyricsViewCommand.DwellMs));
    }

    #endregion

    #region ScrollDepthRatio Validation Tests

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    public async Task Validate_WithScrollDepthRatioAtBoundary_ShouldNotHaveErrors(double scrollDepthRatio)
    {
        // Arrange
        var command = BuildValidCommand(scrollDepthRatio: scrollDepthRatio);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-0.01d)]
    [InlineData(1.01d)]
    public async Task Validate_WithScrollDepthRatioOutOfRange_ShouldHaveError(double scrollDepthRatio)
    {
        // Arrange
        var command = BuildValidCommand(scrollDepthRatio: scrollDepthRatio);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(PublicRecordLyricsViewCommand.ScrollDepthRatio));
    }

    #endregion
}
