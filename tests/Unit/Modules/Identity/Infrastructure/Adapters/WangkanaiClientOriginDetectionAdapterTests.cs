using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Adapters.Wangkanai.Detection;
using AwesomeAssertions;
using Moq;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Adapters;

/// <summary>
/// Unit tests for <see cref="WangkanaiClientOriginDetectionAdapter"/>.
/// </summary>
public class WangkanaiClientOriginDetectionAdapterTests
{
    private readonly Mock<IDetectionService> _detectionServiceMock;
    private readonly Mock<IDeviceService> _deviceServiceMock;
    private readonly Mock<IBrowserService> _browserServiceMock;
    private readonly Mock<IPlatformService> _platformServiceMock;
    private readonly WangkanaiClientOriginDetectionAdapter _adapter;

    public WangkanaiClientOriginDetectionAdapterTests()
    {
        _detectionServiceMock = new Mock<IDetectionService>();
        _deviceServiceMock = new Mock<IDeviceService>();
        _browserServiceMock = new Mock<IBrowserService>();
        _platformServiceMock = new Mock<IPlatformService>();

        _detectionServiceMock.Setup(x => x.Device).Returns(_deviceServiceMock.Object);
        _detectionServiceMock.Setup(x => x.Browser).Returns(_browserServiceMock.Object);
        _detectionServiceMock.Setup(x => x.Platform).Returns(_platformServiceMock.Object);

        _adapter = new WangkanaiClientOriginDetectionAdapter(_detectionServiceMock.Object);
    }

    #region Browser Mapping Tests

    [Theory]
    [InlineData(Browser.Chrome, EnumBrowser.Chrome)]
    [InlineData(Browser.InternetExplorer, EnumBrowser.InternetExplorer)]
    [InlineData(Browser.Safari, EnumBrowser.Safari)]
    [InlineData(Browser.Firefox, EnumBrowser.Firefox)]
    [InlineData(Browser.Edge, EnumBrowser.Edge)]
    [InlineData(Browser.Opera, EnumBrowser.Opera)]
    [InlineData(Browser.GoogleSearchApp, EnumBrowser.GoogleSearchApp)]
    [InlineData(Browser.Samsung, EnumBrowser.Samsung)]
    [InlineData(Browser.Unknown, EnumBrowser.Unknown)]
    public void GetInfo_ShouldMapBrowserCorrectly(Browser wangkanaiBrowser, EnumBrowser expectedBrowser)
    {
        // Arrange
        _browserServiceMock.Setup(x => x.Name).Returns(wangkanaiBrowser);
        _deviceServiceMock.Setup(x => x.Type).Returns(Device.Desktop);
        _platformServiceMock.Setup(x => x.Name).Returns(Platform.Windows);

        // Act
        ClientOriginInfo result = _adapter.GetInfo();

        // Assert
        result.Browser.Should().Be(expectedBrowser);
    }

    #endregion

    #region Device Mapping Tests

    [Theory]
    [InlineData(Device.Desktop, EnumDevice.Desktop)]
    [InlineData(Device.Tablet, EnumDevice.Tablet)]
    [InlineData(Device.Mobile, EnumDevice.Mobile)]
    [InlineData(Device.Watch, EnumDevice.Watch)]
    [InlineData(Device.Tv, EnumDevice.Tv)]
    [InlineData(Device.Console, EnumDevice.Console)]
    [InlineData(Device.Car, EnumDevice.Car)]
    [InlineData(Device.IoT, EnumDevice.IoT)]
    [InlineData(Device.Unknown, EnumDevice.Unknown)]
    public void GetInfo_ShouldMapDeviceCorrectly(Device wangkanaiDevice, EnumDevice expectedDevice)
    {
        // Arrange
        _deviceServiceMock.Setup(x => x.Type).Returns(wangkanaiDevice);
        _browserServiceMock.Setup(x => x.Name).Returns(Browser.Chrome);
        _platformServiceMock.Setup(x => x.Name).Returns(Platform.Windows);

        // Act
        ClientOriginInfo result = _adapter.GetInfo();

        // Assert
        result.Device.Should().Be(expectedDevice);
    }

    #endregion

    #region Platform Mapping Tests

    [Theory]
    [InlineData(Platform.Windows, EnumPlatform.Windows)]
    [InlineData(Platform.Mac, EnumPlatform.Mac)]
    [InlineData(Platform.iOS, EnumPlatform.Ios)]
    [InlineData(Platform.iPadOS, EnumPlatform.IpadOs)]
    [InlineData(Platform.Linux, EnumPlatform.Linux)]
    [InlineData(Platform.Android, EnumPlatform.Android)]
    [InlineData(Platform.ChromeOS, EnumPlatform.ChromeOs)]
    [InlineData(Platform.Unknown, EnumPlatform.Unknown)]
    public void GetInfo_ShouldMapPlatformCorrectly(Platform wangkanaiPlatform, EnumPlatform expectedPlatform)
    {
        // Arrange
        _platformServiceMock.Setup(x => x.Name).Returns(wangkanaiPlatform);
        _browserServiceMock.Setup(x => x.Name).Returns(Browser.Chrome);
        _deviceServiceMock.Setup(x => x.Type).Returns(Device.Desktop);

        // Act
        ClientOriginInfo result = _adapter.GetInfo();

        // Assert
        result.Platform.Should().Be(expectedPlatform);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void GetInfo_WithAllFieldsMapped_ShouldReturnCompleteInfo()
    {
        // Arrange
        _browserServiceMock.Setup(x => x.Name).Returns(Browser.Chrome);
        _deviceServiceMock.Setup(x => x.Type).Returns(Device.Desktop);
        _platformServiceMock.Setup(x => x.Name).Returns(Platform.Windows);

        // Act
        ClientOriginInfo result = _adapter.GetInfo();

        // Assert
        result.Should().NotBeNull();
        result.Browser.Should().Be(EnumBrowser.Chrome);
        result.Device.Should().Be(EnumDevice.Desktop);
        result.Platform.Should().Be(EnumPlatform.Windows);
    }

    [Fact]
    public void GetInfo_ShouldAccessDetectionService()
    {
        // Arrange
        _browserServiceMock.Setup(x => x.Name).Returns(Browser.Chrome);
        _deviceServiceMock.Setup(x => x.Type).Returns(Device.Desktop);
        _platformServiceMock.Setup(x => x.Name).Returns(Platform.Windows);

        // Act
        _adapter.GetInfo();

        // Assert
        _detectionServiceMock.Verify(x => x.Device, Times.Once);
        _detectionServiceMock.Verify(x => x.Browser, Times.Once);
        _detectionServiceMock.Verify(x => x.Platform, Times.Once);
    }

    #endregion
}
