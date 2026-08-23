using _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Unit tests for <see cref="AdminUploadShortVideoFileValidator"/>.
/// </summary>
public class AdminUploadShortVideoFileValidatorTests
{
    private readonly ContentI18n _i18n = TestErrorsFactory.CreateContentI18n();

    private readonly AdminUploadShortVideoFileValidator _validator;

    public AdminUploadShortVideoFileValidatorTests()
    {
        _validator = new AdminUploadShortVideoFileValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidData_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: Guid.NewGuid().ToString(), File: fileMock);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ShortVideoId Validation Tests

    [Fact]
    public async Task Validate_WithEmptyShortVideoId_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: string.Empty, File: fileMock);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUploadShortVideoFileCommand.ShortVideoId));
    }

    [Fact]
    public async Task Validate_WithInvalidGuidShortVideoId_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockVideoFile();
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: "not-a-guid", File: fileMock);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUploadShortVideoFileCommand.ShortVideoId));
    }

    #endregion

    #region File Validation Tests

    [Fact]
    public async Task Validate_WithNullFile_ShouldHaveError()
    {
        // Arrange
        IFormFile? file = null;
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: Guid.NewGuid().ToString(), File: file!);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .Errors.Should()
            .Contain(e =>
                e.PropertyName == nameof(AdminUploadShortVideoFileCommand.File)
                && e.ErrorMessage == _i18n.ShortVideo.Msg.FileRequired()
            );
    }

    [Fact]
    public async Task Validate_WithInvalidVideoExtension_ShouldHaveError()
    {
        // Arrange
        IFormFile fileMock = FileTestHelpers.CreateMockFormFile("photo.jpg", "image/jpeg", 5_000_000);
        var command = new AdminUploadShortVideoFileCommand(ShortVideoId: Guid.NewGuid().ToString(), File: fileMock);

        // Act
        ValidationResult result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUploadShortVideoFileCommand.File));
    }

    #endregion
}
