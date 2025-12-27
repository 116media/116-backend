using _116.Identity.Application.Session.Services;

using DeviceDetectorNET;

using Microsoft.AspNetCore.Http;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service for extracting session metadata from HTTP context using DeviceDetector.NET.
/// </summary>
public class SessionMetadataService : ISessionMetadataService
{
    /// <summary>
    /// Extracts the client's IP address from the HTTP context.
    /// </summary>
    public string? ExtractIpAddress(HttpContext? httpContext)
    {
        return httpContext?.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Extracts the User-Agent header from the HTTP context.
    /// </summary>
    public string? ExtractUserAgent(HttpContext? httpContext)
    {
        return httpContext?.Request.Headers.UserAgent.FirstOrDefault();
    }

    /// <summary>
    /// Parses a device name from the User-Agent string using DeviceDetector.NET.
    /// Returns a formatted string like "Chrome on Windows" or "Safari on iOS".
    /// </summary>
    public string? ParseDeviceName(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(value: userAgent))
        {
            return null;
        }

        var deviceDetector = new DeviceDetector(userAgent: userAgent);
        deviceDetector.Parse();

        string? client = deviceDetector.GetClient().Match?.Name;
        string? os = deviceDetector.GetOs().Match?.Name;

        return (client, os) switch
        {
            (not null, not null) => $"{client} - {os}",
            (not null, _) => client,
            (_, not null) => os,
            _ => "Unknown"
        };
    }
}
