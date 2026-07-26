using System.Globalization;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Unit tests for <see cref="AdminUploadArtistAvatarValidator"/>.
/// </summary>
public class AdminUploadArtistAvatarValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUploadArtistAvatarValidator _validator;

    public AdminUploadArtistAvatarValidatorTests()
    {
        _validator = new AdminUploadArtistAvatarValidator(_i18n);
    }

    private static IFormFile BuildFile()
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        return fileMock.Object;
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new AdminUploadArtistAvatarCommand(ArtistId: Guid.NewGuid(), File: BuildFile());

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region File Validation Tests

    [Fact]
    public async Task Validate_WithNullFile_ShouldHaveError()
    {
        // Arrange
        var command = new AdminUploadArtistAvatarCommand(ArtistId: Guid.NewGuid(), File: null!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArtistAvatarCommand.File)
                && e.ErrorMessage == _i18n.Lyrics.Msg.FileRequired()
            );
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        var validator = new AdminUploadArtistAvatarValidator(_i18n);
        var command = new AdminUploadArtistAvatarCommand(ArtistId: Guid.NewGuid(), File: null!);

        // Act
        ValidationResult result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadArtistAvatarCommand.File)
                && e.ErrorMessage == _i18n.Lyrics.Msg.FileRequired()
            );
    }

    #endregion
}
