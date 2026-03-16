using System.Reflection;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using AwesomeAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Validators;

/// <summary>
/// Unit tests for <see cref="FileValidation"/>.
/// </summary>
public class FileValidationTests
{
    private class TestCommand
    {
        public IFormFile? AvatarFile { get; set; }
    }

    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator(bool isRequired = false)
        {
            RuleFor(x => x.AvatarFile).ValidAvatar(isRequired);
        }
    }

    // Used to exercise the GetAvatarFileValue null-reflection path:
    // typeof(T).GetProperty("AvatarFile") returns null when T has no such property.
    private class TestCommandNoAvatarProperty
    {
        public IFormFile? OtherFile { get; set; }
    }

    private class TestCommandNoAvatarPropertyValidator : AbstractValidator<TestCommandNoAvatarProperty>
    {
        public TestCommandNoAvatarPropertyValidator()
        {
            RuleFor(x => x.OtherFile).ValidAvatar(isRequired: false);
        }
    }

    private static IFormFile CreateMockFile(string fileName, string contentType, long length = 1024)
    {
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType);
        fileMock.Setup(f => f.Length).Returns(length);
        return fileMock.Object;
    }

    [Fact]
    public void ValidAvatar_WithValidFile_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.jpg", "image/jpeg", 1024 * 500), // 500KB
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndNotRequired_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator(isRequired: false);
        var command = new TestCommand { AvatarFile = null };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndRequired_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator(isRequired: true);
        var command = new TestCommand { AvatarFile = null };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage("Avatar file is required");
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndRequired_ShouldOnlyReturnAvatarFileErrors()
    {
        var validator = new TestCommandValidator(isRequired: true);
        var command = new TestCommand { AvatarFile = null };

        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        result.Errors.Should().OnlyContain(e => e.PropertyName == nameof(TestCommand.AvatarFile));
    }

    [Fact]
    public void ValidAvatar_WithFileTooLarge_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes + 1),
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage($"File size must not exceed {FileConstants.MaxAvatarFileSizeBytes / (1024 * 1024)}MB");
    }

    [Fact]
    public void ValidAvatar_WithZeroFileSize_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.jpg", "image/jpeg", 0) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithInvalidMimeType_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.exe", "application/exe") };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(
                $"Only image files are allowed: {string.Join(", ", FileConstants.AllowedAvatarMimeTypes)}"
            );
    }

    [Fact]
    public void ValidAvatar_WithInvalidExtension_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.exe", "image/jpeg", 1024) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(
                $"Only these extensions are allowed: {string.Join(", ", FileConstants.AllowedAvatarExtensions)}"
            );
    }

    [Fact]
    public void ValidAvatar_WithValidPngFile_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.png", "image/png", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithValidGifFile_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.gif", "image/gif", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithValidWebpFile_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.webp", "image/webp", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithContentTypeParameters_ShouldExtractCorrectly()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.jpg", "image/jpeg; boundary=something", 1024 * 500),
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithGenericContentTypeAndValidExtension_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.jpg", "application/octet-stream", 1024 * 500),
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithMultipartContentTypeAndValidExtension_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.png", "multipart/form-data", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithEmptyContentTypeAndValidExtension_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.jpg", "", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithNullContentTypeAndValidExtension_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.jpg", null!, 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithGenericContentTypeAndInvalidExtension_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.exe", "application/octet-stream", 1024 * 500),
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithUppercaseExtension_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.JPG", "image/jpeg", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithUppercaseContentType_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand { AvatarFile = CreateMockFile("avatar.jpg", "IMAGE/JPEG", 1024 * 500) };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithMaxValidFileSize_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator();
        var command = new TestCommand
        {
            AvatarFile = CreateMockFile("avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes),
        };

        // Act
        TestValidationResult<TestCommand>? result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    // The three private predicate methods contain defensive null guards that are unreachable
    // through ValidAvatar (CascadeMode.Stop + When guard both prevent null reaching them).
    // Reflection is the only way to exercise those branches without changing the source.

    [Fact]
    public void BeValidFileSize_WithNullFile_ShouldReturnFalse()
    {
        MethodInfo method = typeof(FileValidation).GetMethod(
            "BeValidFileSize",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        bool result = (bool)method.Invoke(null, [null])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void BeValidImageType_WithNullFile_ShouldReturnFalse()
    {
        MethodInfo method = typeof(FileValidation).GetMethod(
            "BeValidImageType",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        bool result = (bool)method.Invoke(null, [null])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void BeValidFileExtension_WithNullFile_ShouldReturnFalse()
    {
        MethodInfo method = typeof(FileValidation).GetMethod(
            "BeValidFileExtension",
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        bool result = (bool)method.Invoke(null, [null])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void ValidAvatar_WhenTypeHasNoAvatarFileProperty_ShouldSkipValidation()
    {
        // GetAvatarFileValue<T> uses reflection to find "AvatarFile" on T.
        // When T has no such property, property?.GetValue(...) returns null and
        // the When(...) guard evaluates to false, skipping all Must rules.
        var validator = new TestCommandNoAvatarPropertyValidator();
        var command = new TestCommandNoAvatarProperty { OtherFile = CreateMockFile("bad.exe", "application/exe", 1) };

        TestValidationResult<TestCommandNoAvatarProperty>? result = validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.OtherFile);
    }
}
