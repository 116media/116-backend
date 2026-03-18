using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="CloudinaryService"/>.
/// </summary>
public class CloudinaryServiceTests
{
    private readonly Mock<ILogger<CloudinaryService>> _loggerMock = new();
    private readonly CloudinarySettings _settings = new()
    {
        CloudName = "test-cloud",
        ApiKey = "test-key",
        ApiSecret = "test-secret",
    };

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidSettings_ShouldNotThrow()
    {
        // Act
        var service = new CloudinaryService(_settings, _loggerMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    #endregion

    #region UploadImageAsync Tests - Note: These would require integration tests or heavy mocking

    // Note: Full upload testing requires actual Cloudinary integration or complex mocking
    // These tests verify the service can be instantiated and basic validation works

    [Fact]
    public async Task UploadImageAsync_WithNullFile_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);

        // Act & Assert
        Func<Task> act = async () => await service.UploadImageAsync(null!, "test-id");
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithEmptyFile_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        Func<Task> act = () => service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithTooLargeFile_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11MB, over limit
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act & Assert
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithInvalidFileType_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.exe");
        fileMock.Setup(f => f.ContentType).Returns("application/x-msdownload");

        // Act & Assert
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithInvalidExtension_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("test.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");

        // Act & Assert
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        ExceptionAssertions<BadRequestException>? exception = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();

        exception.Which.Message.Should().Be("File.InvalidExtension");
    }

    [Fact]
    public async Task UploadImageAsync_WithJpgExtension_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should not throw during validation, but will fail at actual upload
        // which is expected since we're not mocking the Cloudinary SDK
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithJpegExtension_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(2048);
        fileMock.Setup(f => f.FileName).Returns("photo.jpeg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should not throw during validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithPngExtension_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1536);
        fileMock.Setup(f => f.FileName).Returns("image.png");
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should not throw during validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithGifExtension_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(2048);
        fileMock.Setup(f => f.FileName).Returns("animation.gif");
        fileMock.Setup(f => f.ContentType).Returns("image/gif");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should not throw during validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithWebpExtension_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("modern.webp");
        fileMock.Setup(f => f.ContentType).Returns("image/webp");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should not throw during validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithContentTypeParameters_ShouldParseCorrectly()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg; boundary=----WebKitFormBoundary");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should parse "image/jpeg" from "image/jpeg; boundary=..."
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithNullContentType_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns((Func<string>)null!);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should allow null content type (common in mobile uploads)
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithEmptyContentType_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("image.png");
        fileMock.Setup(f => f.ContentType).Returns(string.Empty);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should allow empty content type
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithOctetStreamContentType_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should allow generic binary content type
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithMultipartFormDataContentType_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("photo.jpeg");
        fileMock.Setup(f => f.ContentType).Returns("multipart/form-data");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should allow multipart/form-data content type
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithInvalidContentTypeButValidExtension_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns("video/mp4");

        // Act & Assert
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        ExceptionAssertions<BadRequestException>? exception = await act.Should()
            .ThrowExactlyAsync<BadRequestException>();

        exception.Which.Message.Should().Be("File.InvalidType");
    }

    [Fact]
    public async Task UploadImageAsync_WithUppercaseExtension_ShouldNormalizeAndValidate()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("IMAGE.JPG");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should normalize .JPG to .jpg and pass validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadImageAsync_WithMixedCaseContentType_ShouldNormalizeAndValidate()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("photo.png");
        fileMock.Setup(f => f.ContentType).Returns("Image/PNG");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should normalize to lowercase and pass validation
        Func<Task> act = async () => await service.UploadImageAsync(fileMock.Object, "test-id");
        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    #endregion

    #region UploadRawAsync Tests

    [Fact]
    public async Task UploadRawAsync_WithNullFile_ShouldThrowBadRequestException()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);

        Func<Task> act = async () => await service.UploadRawAsync(null!, "test-id");

        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithEmptyFile_ShouldThrowBadRequestException()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.pdf");

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithTooLargeFile_ShouldThrowBadRequestException()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(6 * 1024 * 1024); // 6 MB, over the 5 MB limit
        fileMock.Setup(f => f.FileName).Returns("proof.pdf");

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithInvalidExtension_ShouldThrowBadRequestException()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.exe");
        fileMock.Setup(f => f.ContentType).Returns("application/x-msdownload");

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithInvalidContentType_ShouldThrowBadRequestException()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");
        fileMock.Setup(f => f.ContentType).Returns("video/mp4");

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithValidPdfFile_ShouldPassValidation()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.pdf");
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Validation passes, upload fails because there's no real Cloudinary connection
        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithValidImageFile_ShouldPassValidation()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Validation passes, upload fails because there's no real Cloudinary connection
        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithNullContentType_ShouldPassValidation()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.png");
        fileMock.Setup(f => f.ContentType).Returns((Func<string>)null!);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    [Fact]
    public async Task UploadRawAsync_WithOctetStreamContentType_ShouldPassValidation()
    {
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("proof.jpg");
        fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        Func<Task> act = async () => await service.UploadRawAsync(fileMock.Object, "test-id");

        await act.Should().ThrowExactlyAsync<BadGatewayException>();
    }

    #endregion

    #region DeleteImageAsync Tests

    // Note: DeleteImageAsync would also require integration testing or complex mocking
    // The service instantiation validates basic setup

    #endregion
}
