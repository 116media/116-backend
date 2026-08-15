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
    private readonly ValidationErrorMessage _i18n = LocalizerFactory.CreateMessage<ValidationErrorMessage>();
    private readonly InternalServerErrorMessage _internalServer =
        LocalizerFactory.CreateMessage<InternalServerErrorMessage>();

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
    public void FileTooLarge_WithLimit_ShouldReturnBadRequestException()
    {
        // Arrange
        long maxSizeMB = 5;

        // Act
        BadRequestException exception = _errors.FileTooLarge(maxSizeMB);

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
    public void InternalServerErrorMessage_Localizer_FileDownloadFailed_ShouldReturnLocalizedString()
    {
        _internalServer.Localizer["FileDownloadFailed"].Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidationErrorMessage_Localizer_FileNameRequired_ShouldReturnLocalizedString()
    {
        _i18n.Localizer["FileNameRequired"].Value.Should().NotBeNullOrEmpty();
    }
}
