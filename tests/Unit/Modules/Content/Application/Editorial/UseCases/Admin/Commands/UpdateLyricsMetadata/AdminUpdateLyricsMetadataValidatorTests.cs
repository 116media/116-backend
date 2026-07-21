using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateLyricsMetadata;

/// <summary>
/// Unit tests for <see cref="AdminUpdateLyricsMetadataValidator"/>.
/// </summary>
public class AdminUpdateLyricsMetadataValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUpdateLyricsMetadataValidator _validator;

    public AdminUpdateLyricsMetadataValidatorTests()
    {
        _validator = new AdminUpdateLyricsMetadataValidator(_i18n);
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
    [InlineData((short)1899)]
    [InlineData((short)1800)]
    public async Task Validate_WithReleaseYearBeforeMinimum_ShouldHaveError(short releaseYear)
    {
        // Arrange
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
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.ReleaseYear));
    }

    [Fact]
    public async Task Validate_WithReleaseYearAfterMaximum_ShouldHaveError()
    {
        // Arrange
        var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 2);
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
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateLyricsMetadataCommand.ReleaseYear));
    }

    [Fact]
    public async Task Validate_WithReleaseYearAtMinimumBoundary_ShouldNotHaveError()
    {
        // Arrange
        var command = new AdminUpdateLyricsMetadataCommand(
            Id: Guid.NewGuid(),
            Album: null,
            ReleaseYear: 1900,
            Label: null,
            Songwriter: null,
            Producer: null
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithReleaseYearAtMaximumBoundary_ShouldNotHaveError()
    {
        // Arrange
        var releaseYear = (short)(DateTimeOffset.UtcNow.Year + 1);
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
        result.IsValid.Should().BeTrue();
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
