using _116.BuildingBlocks.Constants;
using _116.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Admin.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="AdminUpdateAvatarValidator"/>.
/// </summary>
public class AdminUpdateAvatarValidatorTests
{
    private readonly AdminUpdateAvatarValidator _validator = new();

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile validFile = FileTestHelpers.CreateMockFormFile("avatar.jpg", "image/jpeg", 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidPngFile_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile validFile = FileTestHelpers.CreateMockFormFile("avatar.png", "image/png", 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region AvatarFile Null Tests

    [Fact]
    public async Task Validate_WithNullAvatarFile_ShouldHaveError()
    {
        // Arrange
        AdminUpdateAvatarCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid(), AvatarFile: null!);

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage("Avatar file is required");
    }

    #endregion

    #region File Size Validation Tests

    [Fact]
    public async Task Validate_WithFileSizeTooLarge_ShouldHaveError()
    {
        // Arrange
        IFormFile largeFile = FileTestHelpers.CreateMockFormFile(
            "avatar.jpg",
            "image/jpeg",
            FileConstants.MaxAvatarFileSizeBytes + 1
        );
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: largeFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage($"File size must not exceed {FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)}MB");
    }

    [Fact]
    public async Task Validate_WithZeroFileSize_ShouldHaveError()
    {
        // Arrange
        IFormFile emptyFile = FileTestHelpers.CreateMockFormFile("avatar.jpg", "image/jpeg", 0);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: emptyFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile);
    }

    #endregion

    #region File Type Validation Tests

    [Fact]
    public async Task Validate_WithInvalidMimeType_ShouldHaveError()
    {
        // Arrange
        IFormFile invalidFile = FileTestHelpers.CreateMockFormFile("document.pdf", "application/pdf", 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(
                $"Only image files are allowed: {string.Join(", ", FileConstants.AllowedAvatarMimeTypes)}"
            );
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("image/gif")]
    [InlineData("image/webp")]
    public async Task Validate_WithValidImageMimeTypes_ShouldNotHaveError(string mimeType)
    {
        // Arrange
        string fileName = $"avatar.{mimeType.Split('/')[1]}";
        IFormFile validFile = FileTestHelpers.CreateMockFormFile(fileName, mimeType, 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region File Extension Validation Tests

    [Fact]
    public async Task Validate_WithInvalidExtension_ShouldHaveError()
    {
        // Arrange
        IFormFile invalidFile = FileTestHelpers.CreateMockFormFile("avatar.bmp", "image/bmp", 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(
                $"Only these extensions are allowed: {string.Join(", ", FileConstants.AllowedAvatarExtensions)}"
            );
    }

    [Theory]
    [InlineData(".jpg")]
    [InlineData(".jpeg")]
    [InlineData(".png")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    public async Task Validate_WithValidExtensions_ShouldNotHaveError(string extension)
    {
        // Arrange
        IFormFile validFile = FileTestHelpers.CreateMockFormFile($"avatar{extension}", "image/jpeg", 500_000);
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Multiple Validation Errors Tests

    [Fact]
    public async Task Validate_WithAllInvalidValues_ShouldHaveMultipleErrors()
    {
        // Arrange
        IFormFile invalidFile = FileTestHelpers.CreateMockFormFile(
            "document.pdf",
            "application/pdf",
            FileConstants.MaxAvatarFileSizeBytes + 1
        );
        AdminUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<AdminUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    #endregion
}
