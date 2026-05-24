using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CoreErrors"/>.
/// </summary>
public class CoreErrorsTests
{
    private readonly CoreErrors _coreErrors = TestErrorsFactory.CreateCoreErrors();
    private readonly ValidationErrorMessage _validationMessage = LocalizerFactory.CreateMessage<ValidationErrorMessage>(
        "en"
    );

    [Fact]
    public void FileUploadFailed_WithFileNameAndReason_ShouldReturnConflictException()
    {
        // Arrange
        string fileName = "avatar.jpg";
        string reason = "Network timeout";

        // Act
        ConflictException exception = _coreErrors.FileUploadFailed(fileName, reason);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(fileName);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void UnsupportedFileType_WithFileTypeAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        string fileType = "application/exe";
        string[] allowedTypes = ["image/jpeg", "image/png"];

        // Act
        BadRequestException exception = _coreErrors.UnsupportedFileType(fileType, allowedTypes);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileType);
    }

    [Fact]
    public void FileTooLarge_WithFileSizeAndMaxSize_ShouldReturnBadRequestException()
    {
        // Arrange
        long fileSize = 10485760; // 10MB
        long maxSize = 5242880; // 5MB

        // Act
        BadRequestException exception = _coreErrors.FileTooLarge(fileSize, maxSize);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileSize.ToString());
        exception.Message.Should().Contain(maxSize.ToString());
    }

    [Fact]
    public void CorruptedFile_WithFileName_ShouldReturnBadRequestException()
    {
        // Arrange
        string fileName = "corrupted.pdf";

        // Act
        BadRequestException exception = _coreErrors.CorruptedFile(fileName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileName);
    }

    [Fact]
    public void FileNotFound_WithFileId_ShouldReturnNotFoundException()
    {
        // Arrange
        int fileId = 123;

        // Act
        NotFoundException exception = _coreErrors.FileNotFound(fileId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("File");
        exception.Message.Should().Contain(fileId.ToString());
    }

    [Fact]
    public void FileNotFoundByName_WithFileName_ShouldReturnNotFoundException()
    {
        // Arrange
        string fileName = "missing.jpg";

        // Act
        NotFoundException exception = _coreErrors.FileNotFoundByName(fileName);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("File");
        exception.Message.Should().Contain(fileName);
    }

    [Fact]
    public void InvalidConfiguration_WithConfigKey_ShouldReturnBadRequestException()
    {
        // Arrange
        string configKey = "Cloudinary:ApiKey";

        // Act
        BadRequestException exception = _coreErrors.InvalidConfiguration(configKey);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(configKey);
    }

    [Fact]
    public void ServiceUnavailable_WithServiceName_ShouldReturnInternalServerException()
    {
        // Arrange
        string serviceName = "Cloudinary";

        // Act
        InternalServerException exception = _coreErrors.ServiceUnavailable(serviceName);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(serviceName);
    }

    [Fact]
    public void DatabaseConnectionFailed_ShouldReturnInternalServerException()
    {
        // Act
        InternalServerException exception = _coreErrors.DatabaseConnectionFailed();

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain("database");
    }

    [Fact]
    public void FileNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.FileNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File name is required.");
    }

    [Fact]
    public void OriginalFileNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.OriginalFileNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("Original file name is required.");
    }

    [Fact]
    public void MimeTypeRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.MimeTypeRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("MIME type is required.");
    }

    [Fact]
    public void StorageUrlRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.StorageUrlRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("Storage URL is required.");
    }

    [Fact]
    public void FileSizeMustBeGreaterThanZero_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.FileSizeMustBeGreaterThanZero();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File size must be greater than zero.");
    }

    [Fact]
    public void FileDownloadFailed_WithFileUrlAndReason_ShouldReturnInternalServerException()
    {
        // Arrange
        string fileUrl = "https://example.com/avatar.jpg";
        string reason = "Connection timeout";

        // Act
        InternalServerException exception = _coreErrors.FileDownloadFailed(fileUrl, reason);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(fileUrl);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void InvalidFileUrl_WithFileUrl_ShouldReturnBadRequestException()
    {
        // Arrange
        string fileUrl = "invalid-url";

        // Act
        BadRequestException exception = _coreErrors.InvalidFileUrl(fileUrl);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileUrl);
    }

    [Fact]
    public void FileStorageFailed_WithReason_ShouldReturnInternalServerException()
    {
        // Arrange
        string reason = "Disk full";

        // Act
        InternalServerException exception = _coreErrors.FileStorageFailed(reason);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void FileUrlRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.FileUrlRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File URL is required.");
    }

    [Fact]
    public void BadRequest_WithCustomMessage_ShouldReturnBadRequestException()
    {
        // Arrange
        string message = "Custom error message";

        // Act
        BadRequestException exception = _coreErrors.BadRequest(message);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void FileRequired_ShouldReturnBadRequestException()
    {
        // Act
        BadRequestException exception = _coreErrors.FileRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain("No file was provided");
    }

    [Fact]
    public void FileTooLarge_WithDetailedInfo_ShouldReturnBadRequestException()
    {
        // Arrange
        long actualSize = 10485760; // 10MB in bytes
        long maxSize = 5242880; // 5MB in bytes
        long maxSizeMB = 5;

        // Act
        BadRequestException exception = _coreErrors.FileTooLarge(actualSize, maxSize, maxSizeMB);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(maxSizeMB.ToString());
    }

    [Fact]
    public void InvalidFileType_WithProvidedAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        string providedType = "application/exe";
        string allowedTypes = "image/jpeg, image/png";

        // Act
        BadRequestException exception = _coreErrors.InvalidFileType(providedType, allowedTypes);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(providedType);
        exception.Message.Should().Contain(allowedTypes);
    }

    [Fact]
    public void InvalidFileExtension_WithProvidedAndAllowedExtensions_ShouldReturnBadRequestException()
    {
        // Arrange
        string providedExtension = ".exe";
        string allowedExtensions = ".jpg, .png, .gif";

        // Act
        BadRequestException exception = _coreErrors.InvalidFileExtension(providedExtension, allowedExtensions);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(providedExtension);
        exception.Message.Should().Contain(allowedExtensions);
    }

    [Fact]
    public void FileUploadFailed_WithJustReason_ShouldReturnBadGatewayException()
    {
        // Arrange
        string reason = "External service unavailable";

        // Act
        BadGatewayException exception = _coreErrors.FileUploadFailed(reason);

        // Assert
        exception.Should().BeOfType<BadGatewayException>();
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void ValidationErrorMessage_StorageUrlCannotBeEmpty_ShouldReturnCorrectMessage()
    {
        // Arrange & Act
        string message = _validationMessage.StorageUrlCannotBeEmpty();

        // Assert
        message.Should().Be("Storage URL cannot be empty.");
    }
}
