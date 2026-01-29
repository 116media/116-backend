using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Configurations;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
    private readonly Mock<ILogger<CloudinaryService>> _loggerMock;
    private readonly CloudinarySettings _settings;

    public CloudinaryServiceTests()
    {
        _loggerMock = new Mock<ILogger<CloudinaryService>>();
        _settings = new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret",
        };
    }

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
        await Assert.ThrowsAsync<BadRequestException>(() => service.UploadImageAsync(null!, "test-id"));
    }

    [Fact]
    public async Task UploadImageAsync_WithEmptyFile_ShouldThrowBadRequestException()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("test.jpg");

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadRequestException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadRequestException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UploadImageAsync(fileMock.Object, "test-id")
        );

        exception.Message.Should().Be("File.InvalidExtension");
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
    }

    [Fact]
    public async Task UploadImageAsync_WithNullContentType_ShouldPassValidation()
    {
        // Arrange
        var service = new CloudinaryService(_settings, _loggerMock.Object);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.FileName).Returns("avatar.jpg");
        fileMock.Setup(f => f.ContentType).Returns((string?)null);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        // Act & Assert - Should allow null content type (common in mobile uploads)
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        var exception = await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UploadImageAsync(fileMock.Object, "test-id")
        );

        exception.Message.Should().Be("File.InvalidType");
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
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
        await Assert.ThrowsAsync<BadGatewayException>(() => service.UploadImageAsync(fileMock.Object, "test-id"));
    }

    #endregion

    #region DeleteImageAsync Tests

    // Note: DeleteImageAsync would also require integration testing or complex mocking
    // The service instantiation validates basic setup

    #endregion
}
