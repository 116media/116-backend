using _116.Core.Application.Shared.Errors.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors.Messages;

/// <summary>
/// Unit tests for <see cref="ValidationErrorMessage"/>.
/// </summary>
public class ValidationErrorMessageTests
{
    private readonly ValidationErrorMessage _message = LocalizerFactory.CreateMessage<ValidationErrorMessage>("en");

    [Fact]
    public void Localizer_ShouldNotBeNull()
    {
        _message.Localizer.Should().NotBeNull();
    }

    [Fact]
    public void UnsupportedFileType_WithTypeAndAllowed_ShouldReturnFormattedMessage()
    {
        // Arrange
        string fileType = "application/exe";
        string[] allowedTypes = ["image/jpeg", "image/png"];

        // Act
        string message = _message.UnsupportedFileType(fileType, allowedTypes);

        // Assert
        message
            .Should()
            .Be($"File type '{fileType}' is not supported. Allowed types: {string.Join(", ", allowedTypes)}.");
    }

    [Fact]
    public void FileTooLarge_WithSizes_ShouldReturnFormattedMessage()
    {
        // Arrange
        long fileSize = 10_000_000;
        long maxSize = 5_000_000;

        // Act
        string message = _message.FileTooLarge(fileSize, maxSize);

        // Assert
        message.Should().Be($"File size {fileSize} bytes exceeds maximum allowed size of {maxSize} bytes.");
    }

    [Fact]
    public void CorruptedFile_WithFileName_ShouldReturnFormattedMessage()
    {
        // Arrange
        string fileName = "photo.jpg";

        // Act
        string message = _message.CorruptedFile(fileName);

        // Assert
        message.Should().Be($"File '{fileName}' appears to be corrupted or invalid.");
    }

    [Fact]
    public void InvalidConfiguration_WithConfigKey_ShouldReturnFormattedMessage()
    {
        // Arrange
        string configKey = "JWT_SECRET";

        // Act
        string message = _message.InvalidConfiguration(configKey);

        // Assert
        message.Should().Be($"Configuration '{configKey}' is missing or invalid.");
    }

    [Fact]
    public void FileNameRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.FileNameRequired();

        // Assert
        message.Should().Be("File name is required.");
    }

    [Fact]
    public void OriginalFileNameRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.OriginalFileNameRequired();

        // Assert
        message.Should().Be("Original file name is required.");
    }

    [Fact]
    public void MimeTypeRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.MimeTypeRequired();

        // Assert
        message.Should().Be("MIME type is required.");
    }

    [Fact]
    public void StorageUrlRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.StorageUrlRequired();

        // Assert
        message.Should().Be("Storage URL is required.");
    }

    [Fact]
    public void StorageUrlCannotBeEmpty_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.StorageUrlCannotBeEmpty();

        // Assert
        message.Should().Be("Storage URL cannot be empty.");
    }

    [Fact]
    public void FileSizeMustBeGreaterThanZero_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.FileSizeMustBeGreaterThanZero();

        // Assert
        message.Should().Be("File size must be greater than zero.");
    }

    [Fact]
    public void InvalidFileUrl_WithUrl_ShouldReturnFormattedMessage()
    {
        // Arrange
        string fileUrl = "not-a-url";

        // Act
        string message = _message.InvalidFileUrl(fileUrl);

        // Assert
        message.Should().Be($"File URL '{fileUrl}' has an invalid format. Must be a valid HTTP or HTTPS URL.");
    }

    [Fact]
    public void FileUrlRequired_ShouldReturnCorrectMessage()
    {
        // Act
        string message = _message.FileUrlRequired();

        // Assert
        message.Should().Be("File URL is required.");
    }
}
