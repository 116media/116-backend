using System.Net;
using _116.Core.Application.Shared.Services;
using _116.Core.Infrastructure.Services;
using _116.Shared.Application.Exceptions;
using AwesomeAssertions;
using AwesomeAssertions.Specialized;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace _116.Unit.Tests.Modules.Core.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="FileService"/>.
/// </summary>
public class FileServiceTests
{
    private readonly FileService _service;
    private readonly Mock<ICloudinaryService> _cloudinaryServiceMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public FileServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _cloudinaryServiceMock = new Mock<ICloudinaryService>();
        _service = new FileService(httpClient, _cloudinaryServiceMock.Object);
    }

    #region UploadFileAsync Tests

    [Fact]
    public async Task UploadFileAsync_WithValidFile_ShouldReturnFileUploadResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var uploadResult = new CloudinaryUploadResult(
            PublicId: "test-id",
            SecureUrl: "https://cloudinary.com/test.jpg",
            Format: "jpg",
            Width: 800,
            Height: 600,
            Bytes: 1024,
            ResourceType: "image"
        );

        _cloudinaryServiceMock
            .Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploadResult);

        // Act
        FileUploadResult result = await _service.UploadFileAsync(fileMock.Object, "test-public-id");

        // Assert
        result.Should().NotBeNull();
        result.SecureUrl.Should().Be("https://cloudinary.com/test.jpg");
        result.Format.Should().Be("jpg");
        result.Width.Should().Be(800);
        result.Height.Should().Be(600);
        result.Bytes.Should().Be(1024);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldCallCloudinaryServiceWithCorrectParameters()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("test.jpg");
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var uploadResult = new CloudinaryUploadResult(
            PublicId: "test-id",
            SecureUrl: "https://cloudinary.com/test.jpg",
            Format: "jpg",
            Width: 800,
            Height: 600,
            Bytes: 1024,
            ResourceType: "image"
        );

        _cloudinaryServiceMock
            .Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploadResult);

        string publicId = "test-public-id";
        string folder = "avatars";

        // Act
        await _service.UploadFileAsync(fileMock.Object, publicId, folder);

        // Assert
        _cloudinaryServiceMock.Verify(
            x => x.UploadImageAsync(fileMock.Object, publicId, folder, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    #endregion

    #region DownloadFileAsync Tests

    [Fact]
    public async Task DownloadFileAsync_WithValidUrl_ShouldReturnFileDownloadResult()
    {
        // Arrange
        const string fileUrl = "https://example.com/test.jpg";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("test content"),
        };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.StorageUrl.Should().Be(fileUrl);
        result.MimeType.Should().Be("image/jpeg");
        result.SizeInBytes.Should().Be(1024);
    }

    [Fact]
    public async Task DownloadFileAsync_WithNullUrl_ShouldThrowBadRequestException()
    {
        // Act & Assert
        Func<Task> act = async () => await _service.DownloadFileAsync(null!);
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task DownloadFileAsync_WithEmptyUrl_ShouldThrowBadRequestException()
    {
        // Act & Assert
        Func<Task> act = async () => await _service.DownloadFileAsync("");
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task DownloadFileAsync_WithInvalidUrl_ShouldThrowBadRequestException()
    {
        // Act & Assert
        Func<Task> act = async () => await _service.DownloadFileAsync("not-a-valid-url");
        await act.Should().ThrowExactlyAsync<BadRequestException>();
    }

    [Fact]
    public async Task DownloadFileAsync_WithHttpRequestException_ShouldThrowInternalServerException()
    {
        // Arrange
        const string fileUrl = "https://example.com/test.jpg";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        Func<Task> act = async () => await _service.DownloadFileAsync(fileUrl);
        await act.Should().ThrowExactlyAsync<InternalServerException>();
    }

    [Fact]
    public async Task DownloadFileAsync_WithGenericException_ShouldThrowInternalServerException()
    {
        // Arrange
        const string fileUrl = "https://example.com/test.jpg";

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        Func<Task> act = async () => await _service.DownloadFileAsync(fileUrl);
        ExceptionAssertions<InternalServerException>? exception = await act.Should()
            .ThrowExactlyAsync<InternalServerException>();

        exception.Which.Message.Should().Contain("Unexpected error");
    }

    [Fact]
    public async Task DownloadFileAsync_WhenContentLengthIsZero_ShouldUseFallbackWithRangeRequest()
    {
        // Arrange
        const string fileUrl = "https://example.com/test.jpg";

        // HEAD request returns 0 content length
        var headResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        headResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        headResponse.Content.Headers.ContentLength = 0;

        // Range request returns content range with length
        var rangeResponse = new HttpResponseMessage(HttpStatusCode.PartialContent) { Content = new StringContent("") };
        rangeResponse.Content.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(0, 0, 2048);

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(headResponse)
            .ReturnsAsync(rangeResponse);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.SizeInBytes.Should().Be(2048);
    }

    [Fact]
    public async Task DownloadFileAsync_WhenRangeRequestFails_ShouldUseFallbackGetRequest()
    {
        // Arrange
        const string fileUrl = "https://example.com/test.jpg";

        // HEAD request returns 0 content length
        var headResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        headResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        headResponse.Content.Headers.ContentLength = 0;

        // Range request returns no content range
        var rangeResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };

        // Fallback GET request returns content length
        var getResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        getResponse.Content.Headers.ContentLength = 3072;

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(headResponse)
            .ReturnsAsync(rangeResponse)
            .ReturnsAsync(getResponse);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.SizeInBytes.Should().Be(3072);
    }

    [Fact]
    public async Task DownloadFileAsync_WhenAllFallbacksFail_ShouldReturnZeroSize()
    {
        // Arrange
        string fileUrl = "https://example.com/test.jpg";

        // HEAD request returns 0 content length
        var headResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        headResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        headResponse.Content.Headers.ContentLength = 0;

        // Range request returns no content range
        var rangeResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };

        // Fallback GET request also returns null
        var getResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(headResponse)
            .ReturnsAsync(rangeResponse)
            .ReturnsAsync(getResponse);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.SizeInBytes.Should().Be(0);
    }

    [Fact]
    public async Task DownloadFileAsync_WithUrlWithoutExtension_ShouldResolveExtensionFromContentType()
    {
        // Arrange
        string fileUrl = "https://example.com/files/avatar";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".png");
        result.MimeType.Should().Be("image/png");
    }

    [Fact]
    public async Task DownloadFileAsync_WithUnknownContentType_ShouldUseBinExtension()
    {
        // Arrange
        string fileUrl = "https://example.com/files/document";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
            "application/unknown"
        );
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".bin");
        result.MimeType.Should().Be("application/unknown");
    }

    [Fact]
    public async Task DownloadFileAsync_WithNoContentType_ShouldResolveFromExtension()
    {
        // Arrange
        string fileUrl = "https://example.com/image.webp";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        // Explicitly clear content type header
        responseMessage.Content.Headers.ContentType = null;
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".webp");
        result.MimeType.Should().Be("image/webp");
    }

    [Fact]
    public async Task DownloadFileAsync_WithUnknownExtension_ShouldUseOctetStream()
    {
        // Arrange
        string fileUrl = "https://example.com/file.xyz";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        // Explicitly clear content type header
        responseMessage.Content.Headers.ContentType = null;
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".xyz");
        result.MimeType.Should().Be("application/octet-stream");
    }

    [Fact]
    public async Task DownloadFileAsync_WithGifContentType_ShouldResolveGifExtension()
    {
        // Arrange
        string fileUrl = "https://example.com/animation";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/gif");
        responseMessage.Content.Headers.ContentLength = 2048;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".gif");
        result.MimeType.Should().Be("image/gif");
    }

    [Fact]
    public async Task DownloadFileAsync_WithWebpContentType_ShouldResolveWebpExtension()
    {
        // Arrange
        string fileUrl = "https://example.com/image";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/webp");
        responseMessage.Content.Headers.ContentLength = 1536;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".webp");
        result.MimeType.Should().Be("image/webp");
    }

    [Fact]
    public async Task DownloadFileAsync_WithJpegExtension_ShouldResolveJpegContentType()
    {
        // Arrange
        string fileUrl = "https://example.com/photo.jpeg";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        // Explicitly clear content type header
        responseMessage.Content.Headers.ContentType = null;
        responseMessage.Content.Headers.ContentLength = 2048;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".jpeg");
        result.MimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DownloadFileAsync_WithGifExtension_ShouldResolveGifContentType()
    {
        // Arrange
        string fileUrl = "https://example.com/animation.gif";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        // Explicitly clear content type header
        responseMessage.Content.Headers.ContentType = null;
        responseMessage.Content.Headers.ContentLength = 3072;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".gif");
        result.MimeType.Should().Be("image/gif");
    }

    [Fact]
    public async Task DownloadFileAsync_WithHttpCompletionOptionResponseHeadersRead_ShouldWork()
    {
        // Arrange
        string fileUrl = "https://example.com/file.jpg";

        // HEAD request returns 0 content length
        var headResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        headResponse.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        headResponse.Content.Headers.ContentLength = 0;

        // Range request returns no content range
        var rangeResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };

        // Fallback GET request (ResponseHeadersRead)
        var getResponse = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        getResponse.Content.Headers.ContentLength = 4096;

        _httpMessageHandlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(headResponse)
            .ReturnsAsync(rangeResponse)
            .ReturnsAsync(getResponse);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.SizeInBytes.Should().Be(4096);
    }

    [Fact]
    public async Task DownloadFileAsync_WithSuccessfulHeadRequest_ShouldNotUseFallbacks()
    {
        // Arrange
        string fileUrl = "https://example.com/image.png";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        responseMessage.Content.Headers.ContentLength = 5120;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.SizeInBytes.Should().Be(5120);
        result.MimeType.Should().Be("image/png");

        // Verify only one HEAD request was made (no fallback requests)
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Head),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    [Fact]
    public async Task UploadFileAsync_ShouldGenerateFileIdAndReturnResult()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("upload.jpg");
        fileMock.Setup(f => f.Length).Returns(2048);
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());

        var uploadResult = new CloudinaryUploadResult(
            PublicId: "upload-id",
            SecureUrl: "https://cloudinary.com/upload.jpg",
            Format: "jpg",
            Width: 1024,
            Height: 768,
            Bytes: 2048,
            ResourceType: "image"
        );

        _cloudinaryServiceMock
            .Setup(x =>
                x.UploadImageAsync(
                    It.IsAny<IFormFile>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(uploadResult);

        // Act
        FileUploadResult result = await _service.UploadFileAsync(fileMock.Object, "test-id", "folder");

        // Assert
        result.Should().NotBeNull();
        result.FileId.Should().NotBe(Guid.Empty);
        result.SecureUrl.Should().Be("https://cloudinary.com/upload.jpg");
        result.Format.Should().Be("jpg");
        result.Width.Should().Be(1024);
        result.Height.Should().Be(768);
        result.Bytes.Should().Be(2048);
    }

    [Fact]
    public async Task DownloadFileAsync_WithNoExtensionAndJpegContentType_ShouldResolveJpgExtension()
    {
        // Arrange
        string fileUrl = "https://example.com/image";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        responseMessage.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        responseMessage.Content.Headers.ContentLength = 1024;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".jpg");
        result.MimeType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DownloadFileAsync_WithPngExtension_ShouldResolvePngContentType()
    {
        // Arrange
        string fileUrl = "https://example.com/image.png";
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("") };
        // Explicitly clear content type header
        responseMessage.Content.Headers.ContentType = null;
        responseMessage.Content.Headers.ContentLength = 2048;

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Head && req.RequestUri!.ToString() == fileUrl
                ),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        FileDownloadResult result = await _service.DownloadFileAsync(fileUrl);

        // Assert
        result.Should().NotBeNull();
        result.FileName.Should().EndWith(".png");
        result.MimeType.Should().Be("image/png");
    }

    #endregion
}
