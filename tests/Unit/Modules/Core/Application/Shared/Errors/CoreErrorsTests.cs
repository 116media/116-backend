using _116.Core.Application.Shared.Errors;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="CoreErrors"/>.
/// </summary>
public class CoreErrorsTests
{
    [Fact]
    public void FileUploadFailed_WithFileNameAndReason_ShouldReturnConflictException()
    {
        // Arrange
        var fileName = "avatar.jpg";
        var reason = "Network timeout";

        // Act
        var exception = CoreErrors.FileUploadFailed(fileName, reason);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Contain(fileName);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void UnsupportedFileType_WithFileTypeAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        var fileType = "application/exe";
        var allowedTypes = new[] { "image/jpeg", "image/png" };

        // Act
        var exception = CoreErrors.UnsupportedFileType(fileType, allowedTypes);

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
        var exception = CoreErrors.FileTooLarge(fileSize, maxSize);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileSize.ToString());
        exception.Message.Should().Contain(maxSize.ToString());
    }

    [Fact]
    public void CorruptedFile_WithFileName_ShouldReturnBadRequestException()
    {
        // Arrange
        var fileName = "corrupted.pdf";

        // Act
        var exception = CoreErrors.CorruptedFile(fileName);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileName);
    }

    [Fact]
    public void FileNotFound_WithFileId_ShouldReturnNotFoundException()
    {
        // Arrange
        var fileId = 123;

        // Act
        var exception = CoreErrors.FileNotFound(fileId);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("File");
        exception.Message.Should().Contain(fileId.ToString());
    }

    [Fact]
    public void FileNotFoundByName_WithFileName_ShouldReturnNotFoundException()
    {
        // Arrange
        var fileName = "missing.jpg";

        // Act
        var exception = CoreErrors.FileNotFoundByName(fileName);

        // Assert
        exception.Should().BeOfType<NotFoundException>();
        exception.Message.Should().Contain("File");
        exception.Message.Should().Contain(fileName);
    }

    [Fact]
    public void InvalidConfiguration_WithConfigKey_ShouldReturnBadRequestException()
    {
        // Arrange
        var configKey = "Cloudinary:ApiKey";

        // Act
        var exception = CoreErrors.InvalidConfiguration(configKey);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(configKey);
    }

    [Fact]
    public void ServiceUnavailable_WithServiceName_ShouldReturnInternalServerException()
    {
        // Arrange
        var serviceName = "Cloudinary";

        // Act
        var exception = CoreErrors.ServiceUnavailable(serviceName);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(serviceName);
    }

    [Fact]
    public void DatabaseConnectionFailed_ShouldReturnInternalServerException()
    {
        // Act
        var exception = CoreErrors.DatabaseConnectionFailed();

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain("database");
    }

    [Fact]
    public void FileNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.FileNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File name is required");
    }

    [Fact]
    public void OriginalFileNameRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.OriginalFileNameRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("Original file name is required");
    }

    [Fact]
    public void MimeTypeRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.MimeTypeRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("MIME type is required");
    }

    [Fact]
    public void StorageUrlRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.StorageUrlRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("Storage URL is required");
    }

    [Fact]
    public void FileSizeMustBeGreaterThanZero_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.FileSizeMustBeGreaterThanZero();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File size must be greater than zero");
    }

    [Fact]
    public void FileDownloadFailed_WithFileUrlAndReason_ShouldReturnInternalServerException()
    {
        // Arrange
        var fileUrl = "https://example.com/avatar.jpg";
        var reason = "Connection timeout";

        // Act
        var exception = CoreErrors.FileDownloadFailed(fileUrl, reason);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(fileUrl);
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void InvalidFileUrl_WithFileUrl_ShouldReturnBadRequestException()
    {
        // Arrange
        var fileUrl = "invalid-url";

        // Act
        var exception = CoreErrors.InvalidFileUrl(fileUrl);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileUrl);
    }

    [Fact]
    public void FileStorageFailed_WithReason_ShouldReturnInternalServerException()
    {
        // Arrange
        var reason = "Disk full";

        // Act
        var exception = CoreErrors.FileStorageFailed(reason);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void FileUrlRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.FileUrlRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File URL is required");
    }

    [Fact]
    public void BadRequest_WithCustomMessage_ShouldReturnBadRequestException()
    {
        // Arrange
        var message = "Custom error message";

        // Act
        var exception = CoreErrors.BadRequest(message);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void FileRequired_ShouldReturnBadRequestException()
    {
        // Act
        var exception = CoreErrors.FileRequired();

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File.Required");
        exception.Details.Should().Be("No file was provided for upload");
    }

    [Fact]
    public void FileTooLarge_WithDetailedInfo_ShouldReturnBadRequestException()
    {
        // Arrange
        long actualSize = 10485760; // 10MB in bytes
        long maxSize = 5242880; // 5MB in bytes
        long maxSizeMB = 5;

        // Act
        var exception = CoreErrors.FileTooLarge(actualSize, maxSize, maxSizeMB);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File.TooLarge");
        exception.Details.Should().Contain(actualSize.ToString());
        exception.Details.Should().Contain(maxSize.ToString());
        exception.Details.Should().Contain(maxSizeMB.ToString());
    }

    [Fact]
    public void InvalidFileType_WithProvidedAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        var providedType = "application/exe";
        var allowedTypes = "image/jpeg, image/png";

        // Act
        var exception = CoreErrors.InvalidFileType(providedType, allowedTypes);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File.InvalidType");
        exception.Details.Should().Contain(providedType);
        exception.Details.Should().Contain(allowedTypes);
    }

    [Fact]
    public void InvalidFileExtension_WithProvidedAndAllowedExtensions_ShouldReturnBadRequestException()
    {
        // Arrange
        var providedExtension = ".exe";
        var allowedExtensions = ".jpg, .png, .gif";

        // Act
        var exception = CoreErrors.InvalidFileExtension(providedExtension, allowedExtensions);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be("File.InvalidExtension");
        exception.Details.Should().Contain(providedExtension);
        exception.Details.Should().Contain(allowedExtensions);
    }

    [Fact]
    public void FileUploadFailed_WithJustReason_ShouldReturnBadGatewayException()
    {
        // Arrange
        var reason = "External service unavailable";

        // Act
        var exception = CoreErrors.FileUploadFailed(reason);

        // Assert
        exception.Should().BeOfType<BadGatewayException>();
        exception.Message.Should().Be("File.UploadFailed");
        exception.Details.Should().Contain(reason);
    }

    [Fact]
    public void ValidationErrorMessage_StorageUrlCannotBeEmpty_ShouldReturnCorrectMessage()
    {
        // Arrange & Act
        var message = _116.Core.Application.Shared.Errors.Messages.ValidationErrorMessage.StorageUrlCannotBeEmpty();

        // Assert
        message.Should().Be("Storage URL cannot be empty");
    }
}
