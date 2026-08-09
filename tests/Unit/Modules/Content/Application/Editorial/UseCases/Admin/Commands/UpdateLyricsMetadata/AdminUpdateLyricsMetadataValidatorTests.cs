using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsMetadataValidator"/>.
/// </summary>
public class AdminUpdateLyricsMetadataValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUpdateLyricsMetadataValidator _validator;

    /// <summary>
    /// The instant the validator's clock is pinned to, which makes 2027 the last accepted
    /// release year and 2028 the first rejected one regardless of when the suite runs.
    /// </summary>
    private static readonly DateTimeOffset ValidationInstant = new(2026, 6, 30, 0, 0, 0, TimeSpan.Zero);

    public AdminUpdateLyricsMetadataValidatorTests()
    {
        _validator = new AdminUpdateLyricsMetadataValidator(_i18n, new FakeTimeProvider(ValidationInstant));
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithAllFieldsPopulated_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: "Testament",
            ReleaseYear: 1995,
            Label: "Sonodisc",
            Songwriter: "Papa Wemba",
            Producer: "Viviane Arnoux"
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithAllFieldsNull_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: null,
            Label: null,
            Songwriter: null,
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ReleaseYear Validation Tests

    [Theory]
    [InlineData((short)1800, false)]
    [InlineData((short)1899, false)]
    [InlineData((short)1900, true)]
    [InlineData((short)2027, true)]
    [InlineData((short)2028, false)]
    [InlineData((short)2029, false)]
    public async Task Validate_ShouldAcceptReleaseYearsFrom1900ThroughNextYear(short releaseYear, bool expected)
    {
        // Arrange
        string[] expectedErrors = expected ? [] : [nameof(AdminUpdateLyricsMetadataCommand.ReleaseYear)];
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: releaseYear,
            Label: null,
            Songwriter: null,
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().Be(expected);
        result.Errors.Select(e => e.PropertyName).Should().Equal(expectedErrors);
    }

    #endregion

    #region Optional Field Length Tests

    [Fact]
    public async Task Validate_WithAlbumTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: new string('a', 201),
            ReleaseYear: null,
            Label: null,
            Songwriter: null,
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.Album));
    }

    [Fact]
    public async Task Validate_WithLabelTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: null,
            Label: new string('a', 101),
            Songwriter: null,
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.Label));
    }

    [Fact]
    public async Task Validate_WithSongwriterTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: null,
            Label: null,
            Songwriter: new string('a', 101),
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.Songwriter));
    }

    [Fact]
    public async Task Validate_WithProducerTooLong_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: null,
            Label: null,
            Songwriter: null,
            Producer: new string('a', 101)
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.Producer));
    }

    #endregion
}
