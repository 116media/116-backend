using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateAlbum;

/// <summary>
/// Unit tests for <see cref="AdminUpdateAlbumValidator"/>.
/// </summary>
public class AdminUpdateAlbumValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUpdateAlbumValidator _validator;

    public AdminUpdateAlbumValidatorTests()
    {
        _validator = new AdminUpdateAlbumValidator(
            _i18n,
            new FakeTimeProvider(new DateTimeOffset(2026, 6, 30, 0, 0, 0, TimeSpan.Zero))
        );
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateAlbumCommand(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
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
        var command = new AdminUpdateAlbumCommand(Guid.NewGuid(), string.Empty, null, null, EnumReleaseType.Album);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateAlbumCommand.Name)
                && e.ErrorMessage == _i18n.Album.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithReleaseYearOutOfBounds_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateAlbumCommand(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            1899,
            null,
            EnumReleaseType.Album
        );

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateAlbumCommand.ReleaseYear));
    }
}
