using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Adapters.Wangkanai.Detection;

/// <summary>
/// Adapter interface for detecting client origin information (browser, device, platform).
/// Abstracts away the underlying device detection library (Wangkanai.Detection).
/// </summary>
public interface IClientOriginDetectionAdapter
{
    /// <summary>
    /// Gets client origin information from the current HTTP request.
    /// </summary>
    /// <returns>Client origin information containing browser, device type, and platform.</returns>
    ClientOriginInfo GetInfo();
}

/// <summary>
/// Contains information about the client's origin (browser, device, platform).
/// </summary>
/// <param name="Browser">The browser type detected from the user agent.</param>
/// <param name="Device">The device type detected from the user agent.</param>
/// <param name="Platform">The platform/OS detected from the user agent.</param>
public record ClientOriginInfo(EnumBrowser Browser, EnumDevice Device, EnumPlatform Platform);
