using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Domain.Enums;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace _116.Identity.Infrastructure.Adapters.Wangkanai.Detection;

/// <summary>
/// Adapter implementation for Wangkanai.Detection library.
/// Maps Wangkanai's detection results to domain enums.
/// </summary>
/// <param name="detectionService">The Wangkanai detection service.</param>
public class WangkanaiClientOriginDetectionAdapter(IDetectionService detectionService) : IClientOriginDetectionAdapter
{
    /// <inheritdoc />
    public ClientOriginInfo GetInfo()
    {
        Device device = detectionService.Device.Type;
        Browser browser = detectionService.Browser.Name;
        Platform platform = detectionService.Platform.Name;

        return new ClientOriginInfo(
            Device: MapDevice(device),
            Browser: MapBrowser(browser),
            Platform: MapPlatform(platform)
        );
    }

    /// <summary>
    /// Maps Wangkanai Browser enum to Domain Browser enum.
    /// </summary>
    private static EnumBrowser MapBrowser(Browser browser)
    {
        return browser switch
        {
            Browser.Chrome => EnumBrowser.Chrome,
            Browser.InternetExplorer => EnumBrowser.InternetExplorer,
            Browser.Safari => EnumBrowser.Safari,
            Browser.Firefox => EnumBrowser.Firefox,
            Browser.Edge => EnumBrowser.Edge,
            Browser.Opera => EnumBrowser.Opera,
            Browser.GoogleSearchApp => EnumBrowser.GoogleSearchApp,
            Browser.Samsung => EnumBrowser.Samsung,
            _ => EnumBrowser.Unknown,
        };
    }

    /// <summary>
    /// Maps Wangkanai Device enum to Domain Device enum.
    /// </summary>
    private static EnumDevice MapDevice(Device device)
    {
        return device switch
        {
            Device.Desktop => EnumDevice.Desktop,
            Device.Tablet => EnumDevice.Tablet,
            Device.Mobile => EnumDevice.Mobile,
            Device.Watch => EnumDevice.Watch,
            Device.Tv => EnumDevice.Tv,
            Device.Console => EnumDevice.Console,
            Device.Car => EnumDevice.Car,
            Device.IoT => EnumDevice.IoT,
            _ => EnumDevice.Unknown,
        };
    }

    /// <summary>
    /// Maps Wangkanai Platform enum to Domain Platform enum.
    /// </summary>
    private static EnumPlatform MapPlatform(Platform platform)
    {
        return platform switch
        {
            Platform.Windows => EnumPlatform.Windows,
            Platform.Mac => EnumPlatform.Mac,
            Platform.iOS => EnumPlatform.Ios,
            Platform.iPadOS => EnumPlatform.IpadOs,
            Platform.Linux => EnumPlatform.Linux,
            Platform.Android => EnumPlatform.Android,
            Platform.ChromeOS => EnumPlatform.ChromeOs,
            _ => EnumPlatform.Unknown,
        };
    }
}
