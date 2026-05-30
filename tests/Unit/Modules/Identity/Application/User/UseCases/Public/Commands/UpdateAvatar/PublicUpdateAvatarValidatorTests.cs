using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.User.UseCases.Public.Commands.UpdateAvatar;

/// <summary>
/// Unit tests for <see cref="PublicUpdateAvatarValidator"/>.
/// </summary>
public class PublicUpdateAvatarValidatorTests
{
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>("en");
    private readonly PublicUpdateAvatarValidator _validator;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicUpdateAvatarValidatorTests"/>.
    /// </summary>
    public PublicUpdateAvatarValidatorTests()
    {
        _validator = new PublicUpdateAvatarValidator(_i18n);
    }

    #region Valid Command Tests

    [Fact]
    public async Task Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile validFile = FileTestHelpers.CreateMockFormFile("avatar.jpg", "image/jpeg", 500_000);
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_WithValidPngFile_ShouldNotHaveErrors()
    {
        // Arrange
        IFormFile validFile = FileTestHelpers.CreateMockFormFile("avatar.png", "image/png", 500_000);
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicUpdateAvatarCommand command = new(UserId: Guid.NewGuid(), SessionId: Guid.NewGuid(), AvatarFile: null!);

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage(_i18n.AvatarFileRequired());
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
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: largeFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(_i18n.AvatarFileTooLarge(FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)));
    }

    [Fact]
    public async Task Validate_WithZeroFileSize_ShouldHaveError()
    {
        // Arrange
        IFormFile emptyFile = FileTestHelpers.CreateMockFormFile("avatar.jpg", "image/jpeg", 0);
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: emptyFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(_i18n.AvatarFileInvalidType(string.Join(", ", FileConstants.AllowedAvatarMimeTypes)));
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
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region File Extension Validation Tests

    [Fact]
    public async Task Validate_WithInvalidExtension_ShouldHaveError()
    {
        // Arrange
        IFormFile invalidFile = FileTestHelpers.CreateMockFormFile("avatar.bmp", "image/jpeg", 500_000);
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(
                _i18n.AvatarFileInvalidExtension(string.Join(", ", FileConstants.AllowedAvatarExtensions))
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
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: validFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

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
        PublicUpdateAvatarCommand command = new(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: invalidFile
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await _validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    #endregion

    #region Culture Tests

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    public async Task Validate_ErrorMessages_ShouldBeLocalizedForCulture(string culture)
    {
        // Arrange
        var i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>(culture);
        var validator = new PublicUpdateAvatarValidator(i18n);
        var command = new PublicUpdateAvatarCommand(
            UserId: Guid.NewGuid(),
            SessionId: Guid.NewGuid(),
            AvatarFile: null!
        );

        // Act
        TestValidationResult<PublicUpdateAvatarCommand>? result = await validator.TestValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage(i18n.AvatarFileRequired());
    }

    #endregion
}
