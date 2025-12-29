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
    /// Parses a device name from the User-Agent string.
    /// Extracts basic device/platform information like "Chrome on Windows", "Safari on iOS", etc.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to parse.</param>
    /// <returns>A simplified device name, or null if unavailable.</returns>
    string? ParseDeviceName(string? userAgent);
}
