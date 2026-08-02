using _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UpdateArtist;

/// <summary>
/// Unit tests for <see cref="AdminUpdateArtistValidator"/>.
/// </summary>
public class AdminUpdateArtistValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();
    private readonly AdminUpdateArtistValidator _validator;

    public AdminUpdateArtistValidatorTests()
    {
        _validator = new AdminUpdateArtistValidator(_i18n);
    }

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArtistCommand(Guid.NewGuid(), "Valid Name", "Valid Bio");

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WithEmptyName_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUpdateArtistCommand(Guid.NewGuid(), string.Empty, null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUpdateArtistCommand.Name)
                && e.ErrorMessage == _i18n.Artist.Msg.NameRequired()
            );
    }

    [Fact]
    public async Task Validate_WithNullBio_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUpdateArtistCommand(Guid.NewGuid(), "Valid Name", null);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
