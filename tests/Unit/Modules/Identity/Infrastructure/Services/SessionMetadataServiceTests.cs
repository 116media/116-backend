using System.Net;
using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Services;

/// <summary>
/// Unit tests for <see cref="SessionMetadataService"/>.
/// </summary>
public class SessionMetadataServiceTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IClientOriginDetectionAdapter> _clientOriginDetectionAdapterMock;
    private readonly SessionMetadataService _sut;

    public SessionMetadataServiceTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _clientOriginDetectionAdapterMock = new Mock<IClientOriginDetectionAdapter>();
        _sut = new SessionMetadataService(_httpContextAccessorMock.Object, _clientOriginDetectionAdapterMock.Object);
    }

    #region ExtractIpAddress Tests

    [Fact]
    public void ExtractIpAddress_WithValidIpAddress_ShouldReturnIpAddressString()
    {
        // Arrange
        IPAddress expectedIp = IPAddress.Parse("192.168.1.100");
        SetupHttpContextWithIpAddress(expectedIp);

        // Act
        string? result = _sut.ExtractIpAddress();

        // Assert
        result.Should().Be("192.168.1.100");
    }

    [Fact]
    public void ExtractIpAddress_WithIpv6Address_ShouldReturnIpAddressString()
    {
        // Arrange
        IPAddress expectedIp = IPAddress.Parse("::1");
        SetupHttpContextWithIpAddress(expectedIp);

        // Act
        string? result = _sut.ExtractIpAddress();

        // Assert
        result.Should().Be("::1");
    }

    [Fact]
    public void ExtractIpAddress_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        string? result = _sut.ExtractIpAddress();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractIpAddress_WithNullRemoteIpAddress_ShouldReturnNull()
    {
        // Arrange
        SetupHttpContextWithIpAddress(null);

        // Act
        string? result = _sut.ExtractIpAddress();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ExtractUserAgent Tests

    [Fact]
    public void ExtractUserAgent_WithValidUserAgent_ShouldReturnUserAgentString()
    {
        // Arrange
        string expectedUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
        SetupHttpContextWithUserAgent(expectedUserAgent);

        // Act
        string? result = _sut.ExtractUserAgent();

        // Assert
        result.Should().Be(expectedUserAgent);
    }

    [Fact]
    public void ExtractUserAgent_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        string? result = _sut.ExtractUserAgent();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractUserAgent_WithEmptyUserAgent_ShouldReturnEmptyString()
    {
        // Arrange
        SetupHttpContextWithUserAgent("");

        // Act
        string? result = _sut.ExtractUserAgent();

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ExtractUserAgent_WithMissingUserAgentHeader_ShouldReturnNull()
    {
        // Arrange
        SetupHttpContextWithHeaders(new Dictionary<string, StringValues>());

        // Act
        string? result = _sut.ExtractUserAgent();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetClientOriginInfo Tests

    [Fact]
    public void GetClientOriginInfo_ShouldReturnInfoFromAdapter()
    {
        // Arrange
        ClientOriginInfo expectedInfo = new(EnumBrowser.Chrome, EnumDevice.Desktop, EnumPlatform.Windows);
        _clientOriginDetectionAdapterMock.Setup(x => x.GetInfo()).Returns(expectedInfo);

        // Act
        ClientOriginInfo result = _sut.GetClientOriginInfo();

        // Assert
        result.Should().Be(expectedInfo);
        result.Browser.Should().Be(EnumBrowser.Chrome);
        result.Platform.Should().Be(EnumPlatform.Windows);
        result.Device.Should().Be(EnumDevice.Desktop);
    }

    [Fact]
    public void GetClientOriginInfo_ShouldCallAdapterOnce()
    {
        // Arrange
        _clientOriginDetectionAdapterMock
            .Setup(x => x.GetInfo())
            .Returns(new ClientOriginInfo(EnumBrowser.Firefox, EnumDevice.Desktop, EnumPlatform.Linux));

        // Act
        _sut.GetClientOriginInfo();

        // Assert
        _clientOriginDetectionAdapterMock.Verify(x => x.GetInfo(), Times.Once);
    }

    #endregion

    #region ExtractClientApp Tests

    [Theory]
    [InlineData("WebApp", EnumClient.WebApp)]
    [InlineData("webapp", EnumClient.WebApp)]
    [InlineData("WEBAPP", EnumClient.WebApp)]
    [InlineData("MobileApp", EnumClient.MobileApp)]
    [InlineData("mobileapp", EnumClient.MobileApp)]
    [InlineData("MOBILEAPP", EnumClient.MobileApp)]
    public void ExtractClientApp_WithValidClientApp_ShouldReturnCorrectEnum(string headerValue, EnumClient expected)
    {
        // Arrange
        SetupHttpContextWithClientAppHeader(headerValue);

        // Act
        EnumClient result = _sut.ExtractClientApp();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ExtractClientApp_WithNoHttpContext_ShouldReturnUnknown()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        EnumClient result = _sut.ExtractClientApp();

        // Assert
        result.Should().Be(EnumClient.Unknown);
    }

    [Fact]
    public void ExtractClientApp_WithMissingHeader_ShouldReturnUnknown()
    {
        // Arrange
        SetupHttpContextWithHeaders(new Dictionary<string, StringValues>());

        // Act
        EnumClient result = _sut.ExtractClientApp();

        // Assert
        result.Should().Be(EnumClient.Unknown);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("InvalidClient")]
    [InlineData("NotAClient")]
    [InlineData("123")]
    public void ExtractClientApp_WithInvalidValue_ShouldReturnUnknown(string headerValue)
    {
        // Arrange
        SetupHttpContextWithClientAppHeader(headerValue);

        // Act
        EnumClient result = _sut.ExtractClientApp();

        // Assert
        result.Should().Be(EnumClient.Unknown);
    }

    #endregion

    #region ExtractDeviceId Tests

    [Fact]
    public void ExtractDeviceId_WithValidDeviceId_ShouldReturnDeviceIdString()
    {
        // Arrange
        string expectedDeviceId = "device-uuid-12345";
        SetupHttpContextWithDeviceIdHeader(expectedDeviceId);

        // Act
        string? result = _sut.ExtractDeviceId();

        // Assert
        result.Should().Be(expectedDeviceId);
    }

    [Fact]
    public void ExtractDeviceId_WithNoHttpContext_ShouldReturnNull()
    {
        // Arrange
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        string? result = _sut.ExtractDeviceId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractDeviceId_WithMissingHeader_ShouldReturnNull()
    {
        // Arrange
        SetupHttpContextWithHeaders(new Dictionary<string, StringValues>());

        // Act
        string? result = _sut.ExtractDeviceId();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void ExtractDeviceId_WithEmptyHeader_ShouldReturnEmptyString()
    {
        // Arrange
        SetupHttpContextWithDeviceIdHeader("");

        // Act
        string? result = _sut.ExtractDeviceId();

        // Assert
        result.Should().Be("");
    }

    [Fact]
    public void ExtractDeviceId_WithGuidDeviceId_ShouldReturnGuidString()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        SetupHttpContextWithDeviceIdHeader(deviceId.ToString());

        // Act
        string? result = _sut.ExtractDeviceId();

        // Assert
        result.Should().Be(deviceId.ToString());
    }

    #endregion

    #region Helper Methods

    private void SetupHttpContextWithIpAddress(IPAddress? ipAddress)
    {
        Mock<HttpContext> httpContextMock = new();
        Mock<ConnectionInfo> connectionInfoMock = new();

        connectionInfoMock.Setup(x => x.RemoteIpAddress).Returns(ipAddress);
        httpContextMock.Setup(x => x.Connection).Returns(connectionInfoMock.Object);
        httpContextMock.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
    }

    private void SetupHttpContextWithUserAgent(string userAgent)
    {
        Dictionary<string, StringValues> headers = new() { { "User-Agent", new StringValues(userAgent) } };

        SetupHttpContextWithHeaders(headers);
    }

    private void SetupHttpContextWithClientAppHeader(string clientApp)
    {
        Dictionary<string, StringValues> headers = new() { { "Client-App", new StringValues(clientApp) } };

        SetupHttpContextWithHeaders(headers);
    }

    private void SetupHttpContextWithDeviceIdHeader(string deviceId)
    {
        Dictionary<string, StringValues> headers = new() { { "X-Device-Id", new StringValues(deviceId) } };

        SetupHttpContextWithHeaders(headers);
    }

    private void SetupHttpContextWithHeaders(Dictionary<string, StringValues> headers)
    {
        Mock<HttpContext> httpContextMock = new();
        Mock<HttpRequest> requestMock = new();
        Mock<ConnectionInfo> connectionInfoMock = new();

        HeaderDictionary headerDictionary = new(headers);
        requestMock.Setup(x => x.Headers).Returns(headerDictionary);
        connectionInfoMock.Setup(x => x.RemoteIpAddress).Returns((IPAddress?)null);
        httpContextMock.Setup(x => x.Request).Returns(requestMock.Object);
        httpContextMock.Setup(x => x.Connection).Returns(connectionInfoMock.Object);

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);
    }

    #endregion
}
