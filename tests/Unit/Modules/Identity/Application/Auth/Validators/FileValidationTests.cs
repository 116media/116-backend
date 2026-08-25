using System.Reflection;
using _116.BuildingBlocks.Constants;
using _116.Identity.Application.Auth.Validators;
using _116.Identity.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
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
    private const long SampleLength = 1024 * 500;
    private const long BytesPerMegabyte = 1024 * 1024;

    private readonly ValidationErrorMessage _enMsg = LocalizerFactory.CreateMessage<ValidationErrorMessage>();

    /// <summary>
    /// The branch of the avatar rule chain a rejected file is required to trigger. The chain
    /// cascades NotNull (when required), size, media type, then extension.
    /// </summary>
    public enum AvatarFailure
    {
        /// <summary>No file was supplied to a rule configured as required.</summary>
        Required,

        /// <summary>The file is empty or above <see cref="FileConstants.MaxAvatarFileSizeBytes"/>.</summary>
        TooLarge,

        /// <summary>The media type is neither allowed nor a generic type backed by a valid extension.</summary>
        InvalidType,

        /// <summary>The extension is outside <see cref="FileConstants.AllowedAvatarExtensions"/>.</summary>
        InvalidExtension,
    }

    private class TestCommand
    {
        public IFormFile? AvatarFile { get; set; }
    }

    private class TestCommandValidator : AbstractValidator<TestCommand>
    {
        public TestCommandValidator(ValidationErrorMessage i18n, bool isRequired = false)
        {
            RuleFor(x => x.AvatarFile).ValidAvatar(i18n, isRequired: isRequired);
        }
    }

    /// <summary>
    /// Command shape without a <see cref="TestCommand.AvatarFile"/> property, used to exercise the
    /// null-reflection path inside <see cref="FileValidation"/>.
    /// </summary>
    private class TestCommandNoAvatarProperty
    {
        public IFormFile? OtherFile { get; set; }
    }

    private class TestCommandNoAvatarPropertyValidator : AbstractValidator<TestCommandNoAvatarProperty>
    {
        public TestCommandNoAvatarPropertyValidator(ValidationErrorMessage i18n)
        {
            RuleFor(x => x.OtherFile).ValidAvatar(i18n, isRequired: false);
        }
    }

    /// <summary>
    /// Builds an uploaded-file stand-in carrying only the descriptor the avatar rule reads.
    /// </summary>
    /// <param name="fileName">The client-supplied file name, including its extension.</param>
    /// <param name="contentType">The client-supplied media type, which may be absent.</param>
    /// <param name="length">The reported file size in bytes.</param>
    /// <returns>The configured file.</returns>
    private static IFormFile CreateMockFile(string fileName, string? contentType, long length = 1024)
    {
        var fileMock = new Mock<IFormFile>();

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.ContentType).Returns(contentType!);
        fileMock.Setup(f => f.Length).Returns(length);

        return fileMock.Object;
    }

    /// <summary>
    /// Resolves the localized message the given failure branch must produce.
    /// </summary>
    /// <param name="failure">The branch the rule chain is expected to report.</param>
    /// <returns>The localized message for the branch.</returns>
    private string ExpectedMessage(AvatarFailure failure) =>
        failure switch
        {
            AvatarFailure.Required => _enMsg.AvatarFileRequired(),
            AvatarFailure.TooLarge => _enMsg.AvatarFileTooLarge(
                FileConstants.MaxAvatarFileSizeBytes / BytesPerMegabyte
            ),
            AvatarFailure.InvalidType => _enMsg.AvatarFileInvalidType(
                string.Join(", ", FileConstants.AllowedAvatarMimeTypes)
            ),
            AvatarFailure.InvalidExtension => _enMsg.AvatarFileInvalidExtension(
                string.Join(", ", FileConstants.AllowedAvatarExtensions)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "Unmapped avatar failure"),
        };

    #region ValidAvatar — optional (default)

    [Theory]
    [InlineData("avatar.jpg", "image/jpeg", SampleLength)]
    [InlineData("avatar.jpg", "image/jpg", SampleLength)]
    [InlineData("avatar.jpeg", "image/jpeg", SampleLength)]
    [InlineData("avatar.png", "image/png", SampleLength)]
    [InlineData("avatar.gif", "image/gif", SampleLength)]
    [InlineData("avatar.webp", "image/webp", SampleLength)]
    [InlineData("avatar.jpg", "image/jpeg; boundary=something", SampleLength)]
    [InlineData("avatar.jpg", "IMAGE/JPEG", SampleLength)]
    [InlineData("avatar.JPG", "image/jpeg", SampleLength)]
    [InlineData("avatar.jpg", "application/octet-stream", SampleLength)]
    [InlineData("avatar.png", "multipart/form-data", SampleLength)]
    [InlineData("avatar.jpg", "", SampleLength)]
    [InlineData("avatar.jpg", null, SampleLength)]
    [InlineData("avatar.jpg", "image/jpeg", 1L)]
    [InlineData("avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes)]
    public void ValidAvatar_ShouldAccept(string fileName, string? contentType, long length)
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg);
        var command = new TestCommand { AvatarFile = CreateMockFile(fileName, contentType, length) };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Theory]
    [InlineData("avatar.jpg", "image/jpeg", 0L, AvatarFailure.TooLarge)]
    [InlineData("avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes + 1, AvatarFailure.TooLarge)]
    [InlineData("avatar.exe", "application/exe", 1024L, AvatarFailure.InvalidType)]
    [InlineData("avatar.jpg", "text/plain", 1024L, AvatarFailure.InvalidType)]
    [InlineData("avatar.exe", "application/octet-stream", SampleLength, AvatarFailure.InvalidType)]
    [InlineData("avatar.exe", "image/jpeg", 1024L, AvatarFailure.InvalidExtension)]
    [InlineData("avatar.txt", "image/png", 1024L, AvatarFailure.InvalidExtension)]
    [InlineData("avatar", "image/png", 1024L, AvatarFailure.InvalidExtension)]
    public void ValidAvatar_ShouldRejectWithTheExpectedMessage(
        string fileName,
        string? contentType,
        long length,
        AvatarFailure failure
    )
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg);
        var command = new TestCommand { AvatarFile = CreateMockFile(fileName, contentType, length) };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage(ExpectedMessage(failure));
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndNotRequired_ShouldPass()
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg, isRequired: false);
        var command = new TestCommand { AvatarFile = null };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WhenTypeHasNoAvatarFileProperty_ShouldSkipValidation()
    {
        // Arrange
        var validator = new TestCommandNoAvatarPropertyValidator(_enMsg);
        var command = new TestCommandNoAvatarProperty { OtherFile = CreateMockFile("bad.exe", "application/exe", 1) };

        // Act
        TestValidationResult<TestCommandNoAvatarProperty> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.OtherFile);
    }

    #endregion

    #region ValidAvatar — required (isRequired = true)

    [Theory]
    [InlineData("avatar.jpg", "image/jpeg", 0L, AvatarFailure.TooLarge)]
    [InlineData("avatar.jpg", "image/jpeg", FileConstants.MaxAvatarFileSizeBytes + 1, AvatarFailure.TooLarge)]
    [InlineData("avatar.jpg", "text/plain", 1024L, AvatarFailure.InvalidType)]
    [InlineData("avatar.exe", "image/jpeg", 1024L, AvatarFailure.InvalidExtension)]
    public void ValidAvatar_Required_WithPresentButInvalidFile_ShouldRejectWithTheExpectedMessage(
        string fileName,
        string? contentType,
        long length,
        AvatarFailure failure
    )
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg, isRequired: true);
        var command = new TestCommand { AvatarFile = CreateMockFile(fileName, contentType, length) };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AvatarFile).WithErrorMessage(ExpectedMessage(failure));
    }

    [Theory]
    [InlineData("avatar.jpg", "image/jpeg", SampleLength)]
    [InlineData("avatar.png", "image/png", FileConstants.MaxAvatarFileSizeBytes)]
    public void ValidAvatar_Required_ShouldAccept(string fileName, string? contentType, long length)
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg, isRequired: true);
        var command = new TestCommand { AvatarFile = CreateMockFile(fileName, contentType, length) };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.AvatarFile);
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndRequired_ShouldFail()
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg, isRequired: true);
        var command = new TestCommand { AvatarFile = null };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result
            .ShouldHaveValidationErrorFor(x => x.AvatarFile)
            .WithErrorMessage(ExpectedMessage(AvatarFailure.Required));
    }

    [Fact]
    public void ValidAvatar_WithNullFileAndRequired_ShouldOnlyReturnAvatarFileErrors()
    {
        // Arrange
        var validator = new TestCommandValidator(_enMsg, isRequired: true);
        var command = new TestCommand { AvatarFile = null };

        // Act
        TestValidationResult<TestCommand> result = validator.TestValidate(command);

        // Assert
        result.Errors.Should().OnlyContain(e => e.PropertyName == nameof(TestCommand.AvatarFile));
    }

    #endregion

    #region Defensive null guards

    // The three private predicate methods contain defensive null guards that are unreachable
    // through ValidAvatar (CascadeMode.Stop + When guard both prevent null reaching them).
    // Reflection is the only way to exercise those branches without changing the source.

    [Theory]
    [InlineData("BeValidFileSize")]
    [InlineData("BeValidImageType")]
    [InlineData("BeValidFileExtension")]
    public void Predicate_WithNullFile_ShouldReturnFalse(string predicateName)
    {
        // Arrange
        MethodInfo method = typeof(FileValidation).GetMethod(
            predicateName,
            BindingFlags.NonPublic | BindingFlags.Static
        )!;

        // Act
        bool result = (bool)method.Invoke(null, [null])!;

        // Assert
        result.Should().BeFalse(predicateName);
    }

    #endregion
}
