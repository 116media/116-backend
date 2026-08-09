using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateAlbum;

/// <summary>
/// Unit tests for <see cref="AdminCreateAlbumValidator"/>.
/// </summary>
public class AdminCreateAlbumValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateAlbumValidator _validator;

    public AdminCreateAlbumValidatorTests()
    {
        _validator = new AdminCreateAlbumValidator(
            _i18n,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero))
        );
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            null,
            TestConstants.Album.ValidReleaseYear,
            TestConstants.Album.ValidLabel,
            EnumReleaseType.Album
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateAlbumCommand(string.Empty, null, null, null, EnumReleaseType.Album);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateAlbumCommand.Name)
                && e.ErrorMessage == _i18n.Album.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReleaseYearOutOfBounds_ShouldHaveError()
    {
        // Arrange
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            null,
            1899,
            null,
            EnumReleaseType.Album
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateAlbumCommand.ReleaseYear));
    }

    [Fact]
    public async Task Validate_WithNullReleaseYear_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminCreateAlbumCommand(
            TestConstants.Album.ValidName,
            null,
            null,
            null,
            EnumReleaseType.Album
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
