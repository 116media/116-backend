using _116.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.CreateArtist;

/// <summary>
/// Unit tests for <see cref="AdminCreateArtistValidator"/>.
/// </summary>
public class AdminCreateArtistValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminCreateArtistValidator _validator;

    public AdminCreateArtistValidatorTests()
    {
        _validator = new AdminCreateArtistValidator(_i18n);
    }

    private static AdminCreateArtistCommand BuildValidCommand(
        string? name = null,
        string? slug = null,
        string? bio = null
    ) =>
        new(
            name ?? TestConstants.Content.Editorial.Artist.ValidName,
            slug ?? TestConstants.Content.Editorial.Artist.ValidSlug,
            bio,
            null,
            null,
            null,
            null
        );

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
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(name: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArtistCommand.Name)
                && e.ErrorMessage == _i18n.Artist.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithEmptySlug_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(slug: string.Empty);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminCreateArtistCommand.Slug)
                && e.ErrorMessage == _i18n.Artist.Msg.SlugRequired()
            );
    }

    [Fact]
    public async Task Validate_WithUppercaseSlug_ShouldHaveError()
    {
        // Arrange
        var command = BuildValidCommand(slug: "Invalid-Slug");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminCreateArtistCommand.Slug));
    }

    [Fact]
    public async Task Validate_WithNullBio_ShouldNotHaveErrors()
    {
        // Arrange
        var command = BuildValidCommand(bio: null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
