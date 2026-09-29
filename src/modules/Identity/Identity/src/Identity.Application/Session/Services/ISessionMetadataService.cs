using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Session.Services;

/// <summary>
/// Service for extracting session metadata from HTTP context.
/// </summary>
public interface ISessionMetadataService
{
    /// <summary>
    /// Extracts the client's IP address from the HTTP context.
    /// </summary>
    /// <returns>The IP address as a string, or null if unavailable.</returns>
    string? ExtractIpAddress();

    /// <summary>
    /// Extracts the User-Agent header from the HTTP context.
    /// </summary>
    /// <returns>The User-Agent string, or null if unavailable.</returns>
    string? ExtractUserAgent();

    /// <summary>
    /// Gets client origin information (browser, device, platform) from the current HTTP request.
    /// </summary>
    /// <returns>Client origin information containing browser, device, and platform details.</returns>
    ClientOriginInfo GetClientOriginInfo();

    /// <summary>
    /// Extracts the client application type from the Client-App header.
    /// Expected values: "mobile-app", "web-app", "dashboard".
    /// </summary>
    /// <returns>The client application type enum. Returns Unknown if header is missing or invalid.</returns>
    EnumClient ExtractClientApp();

    /// <summary>
    /// Extracts the device identifier from the X-Device-Id header.
    /// This is a client-generated unique identifier (GUID/UUID/NanoID) that persists across app restarts.
    /// Used to prevent duplicate active sessions on the same device.
    /// </summary>
    /// <returns>The device identifier string, or null if the header is missing.</returns>
    string? ExtractDeviceId();
}
