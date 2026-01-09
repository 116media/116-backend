using _116.Identity.Application.Adapters.Wangkanai.Detection;
using _116.Identity.Application.Session.Services;
using _116.Identity.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service for extracting session metadata from HTTP context.
/// </summary>
/// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
/// <param name="clientOriginDetectionAdapter">Adapter for detecting client origin information.</param>
public class SessionMetadataService(
    IHttpContextAccessor httpContextAccessor,
    IClientOriginDetectionAdapter clientOriginDetectionAdapter
) : ISessionMetadataService
{
    /// <summary>
    /// Extracts the client's IP address from the HTTP context.
    /// </summary>
    public string? ExtractIpAddress()
    {
        return httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Extracts the User-Agent header from the HTTP context.
    /// </summary>
    public string? ExtractUserAgent()
    {
        return httpContextAccessor.HttpContext?.Request.Headers.UserAgent.FirstOrDefault();
    }

    /// <summary>
    /// Gets client origin information (browser, device, platform) from the current HTTP request.
    /// </summary>
    public ClientOriginInfo GetClientOriginInfo()
    {
        return clientOriginDetectionAdapter.GetInfo();
    }

    /// <summary>
    /// Extracts the client application type from the Client-App header.
    /// </summary>
    public EnumClient ExtractClientApp()
    {
        string? clientAppHeader = httpContextAccessor.HttpContext?.Request.Headers["Client-App"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(value: clientAppHeader))
        {
            return EnumClient.Unknown;
        }

        return clientAppHeader.ToLower() switch
        {
            "mobile-app" => EnumClient.MobileApp,
            "web-app" => EnumClient.WebApp,
            "dashboard" => EnumClient.Dashboard,
            _ => EnumClient.Unknown,
        };
    }

    /// <summary>
    /// Extracts the device identifier from the X-Device-Id header.
    /// </summary>
    public string? ExtractDeviceId()
    {
        return httpContextAccessor.HttpContext?.Request.Headers["X-Device-Id"].FirstOrDefault();
    }
}
