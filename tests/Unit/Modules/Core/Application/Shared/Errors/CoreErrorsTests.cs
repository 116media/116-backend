using _116.Core.Application.Shared.Errors;
using _116.Core.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Application.Shared.Errors;

/// <summary>
/// Unit tests for <see cref="FileErrors"/>.
/// </summary>
public class CoreErrorsTests
{
    private readonly FileErrors _errors = TestErrorsFactory.CreateFileErrors();
    private readonly ConflictErrorMessage _conflict = LocalizerFactory.CreateMessage<ConflictErrorMessage>("en");
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>("en");
    private readonly InternalServerErrorMessage _internalServer =
        LocalizerFactory.CreateMessage<InternalServerErrorMessage>("en");

    [Fact]
    public void FileUploadFailed_WithFileNameAndReason_ShouldReturnConflictException()
    {
        // Arrange
        string fileName = "avatar.jpg";
        string reason = "Network timeout";

        // Act
        ConflictException exception = _errors.FileUploadFailed(fileName, reason);

        // Assert
        exception.Should().BeOfType<ConflictException>();
        exception.Message.Should().Be(_conflict.FileUploadFailed(fileName, reason));
    }

    [Fact]
    public void UnsupportedFileType_WithFileTypeAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        string fileType = "application/exe";
        string[] allowedTypes = ["image/jpeg", "image/png"];

        // Act
        BadRequestException exception = _errors.UnsupportedFileType(fileType, allowedTypes);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Contain(fileType);
    }

    [Fact]
    public void FileTooLarge_WithFileSizeAndMaxSize_ShouldReturnBadRequestException()
    {
        // Arrange
        long fileSize = 10485760;
        long maxSize = 5242880;

        // Act
        BadRequestException exception = _errors.FileTooLarge(fileSize, maxSize);

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
        BadRequestException exception = _errors.CorruptedFile(fileName);

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
        NotFoundException exception = _errors.FileNotFound(fileId);

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
        NotFoundException exception = _errors.FileNotFoundByName(fileName);

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
        BadRequestException exception = _errors.InvalidConfiguration(configKey);

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
        InternalServerException exception = _errors.ServiceUnavailable(serviceName);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(serviceName);
    }

    [Fact]
    public void DatabaseConnectionFailed_ShouldReturnInternalServerException()
    {
        InternalServerException exception = _errors.DatabaseConnectionFailed();

        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Be(_internalServer.DatabaseConnectionFailed());
    }

    [Fact]
    public void FileNameRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.FileNameRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.FileNameRequired());
    }

    [Fact]
    public void OriginalFileNameRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.OriginalFileNameRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.OriginalFileNameRequired());
    }

    [Fact]
    public void MimeTypeRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.MimeTypeRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.MimeTypeRequired());
    }

    [Fact]
    public void StorageUrlRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.StorageUrlRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.StorageUrlRequired());
    }

    [Fact]
    public void FileSizeMustBeGreaterThanZero_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.FileSizeMustBeGreaterThanZero();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.FileSizeMustBeGreaterThanZero());
    }

    [Fact]
    public void FileDownloadFailed_WithFileUrlAndReason_ShouldReturnInternalServerException()
    {
        // Arrange
        string fileUrl = "https://example.com/avatar.jpg";
        string reason = "Connection timeout";

        // Act
        InternalServerException exception = _errors.FileDownloadFailed(fileUrl, reason);

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
        BadRequestException exception = _errors.InvalidFileUrl(fileUrl);

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
        InternalServerException exception = _errors.FileStorageFailed(reason);

        // Assert
        exception.Should().BeOfType<InternalServerException>();
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void FileUrlRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.FileUrlRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.FileUrlRequired());
    }

    [Fact]
    public void FileRequired_ShouldReturnBadRequestException()
    {
        BadRequestException exception = _errors.FileRequired();

        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.FileRequired());
    }

    [Fact]
    public void FileTooLarge_WithDetailedInfo_ShouldReturnBadRequestException()
    {
        // Arrange
        long actualSize = 10485760;
        long maxSize = 5242880;
        long maxSizeMB = 5;

        // Act
        BadRequestException exception = _errors.FileTooLarge(actualSize, maxSize, maxSizeMB);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.FileTooLargeWithLimit(maxSizeMB));
    }

    [Fact]
    public void InvalidFileType_WithProvidedAndAllowedTypes_ShouldReturnBadRequestException()
    {
        // Arrange
        string providedType = "application/exe";
        string allowedTypes = "image/jpeg, image/png";

        // Act
        BadRequestException exception = _errors.InvalidFileType(providedType, allowedTypes);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.InvalidFileType(providedType, allowedTypes));
    }

    [Fact]
    public void InvalidFileExtension_WithProvidedAndAllowedExtensions_ShouldReturnBadRequestException()
    {
        // Arrange
        string providedExtension = ".exe";
        string allowedExtensions = ".jpg, .png, .gif";

        // Act
        BadRequestException exception = _errors.InvalidFileExtension(providedExtension, allowedExtensions);

        // Assert
        exception.Should().BeOfType<BadRequestException>();
        exception.Message.Should().Be(_i18n.InvalidFileExtension(providedExtension, allowedExtensions));
    }

    [Fact]
    public void FileUploadFailed_WithJustReason_ShouldReturnBadGatewayException()
    {
        // Arrange
        string reason = "some reason";

        // Act
        BadGatewayException exception = _errors.FileUploadFailed(reason);

        // Assert
        exception.Should().BeOfType<BadGatewayException>();
        exception.Message.Should().Be(_i18n.FileUploadFailed(reason));
    }

    [Fact]
    public void ValidationErrorMessage_StorageUrlCannotBeEmpty_ShouldReturnCorrectMessage()
    {
        string message = _i18n.StorageUrlCannotBeEmpty();

        message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ConflictErrorMessage_Localizer_FileUploadFailed_ShouldReturnLocalizedString()
    {
        _conflict.Localizer["FileUploadFailed"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void InternalServerErrorMessage_Localizer_ServiceUnavailable_ShouldReturnLocalizedString()
    {
        _internalServer.Localizer["ServiceUnavailable"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidationErrorMessage_Localizer_UnsupportedFileType_ShouldReturnLocalizedString()
    {
        _i18n.Localizer["UnsupportedFileType"].Value.Should().NotBeNullOrEmpty();
    }
}
