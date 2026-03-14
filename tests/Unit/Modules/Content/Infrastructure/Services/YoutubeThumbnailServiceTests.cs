using System.Net;
using _116.Content.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="YoutubeThumbnailService"/>.
/// </summary>
public class YoutubeThumbnailServiceTests
{
    private const string YoutubeVideoId = "dQw4w9WgXcQ";
    private static readonly byte[] FakeImageBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0x01, 0x02];

    #region DownloadThumbnailAsync — maxresdefault succeeds

    [Fact]
    public async Task DownloadThumbnailAsync_WhenMaxresdefaultSucceeds_ShouldReturnFormFile()
    {
        // Arrange
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(FakeImageBytes),
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new YoutubeThumbnailService(httpClient);

        // Act
        IFormFile result = await service.DownloadThumbnailAsync(YoutubeVideoId);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("thumbnail");
        result.FileName.Should().Be("thumbnail.jpg");
        result.ContentType.Should().Be("image/jpeg");
        result.Length.Should().Be(FakeImageBytes.Length);
    }

    [Fact]
    public async Task DownloadThumbnailAsync_WhenMaxresdefaultSucceeds_ShouldFetchCorrectUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(FakeImageBytes),
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new YoutubeThumbnailService(httpClient);

        // Act
        await service.DownloadThumbnailAsync(YoutubeVideoId);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest.RequestUri!.ToString().Should().Contain("maxresdefault.jpg");
        capturedRequest.RequestUri.ToString().Should().Contain(YoutubeVideoId);
    }

    #endregion

    #region DownloadThumbnailAsync — fallback to hqdefault

    [Fact]
    public async Task DownloadThumbnailAsync_WhenMaxresdefaultFails_ShouldFallbackToHqdefault()
    {
        // Arrange — first call returns 404, second call succeeds
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound })
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(FakeImageBytes),
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new YoutubeThumbnailService(httpClient);

        // Act
        IFormFile result = await service.DownloadThumbnailAsync(YoutubeVideoId);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().Be(FakeImageBytes.Length);
        result.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task DownloadThumbnailAsync_WhenMaxresdefaultFails_ShouldMakeTwoRequests()
    {
        // Arrange
        Mock<HttpMessageHandler> handlerMock = new();
        handlerMock
            .Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound })
            .ReturnsAsync(
                new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(FakeImageBytes),
                }
            );

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new YoutubeThumbnailService(httpClient);

        // Act
        await service.DownloadThumbnailAsync(YoutubeVideoId);

        // Assert — two HTTP calls were made (maxresdefault + hqdefault)
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    #endregion
}
