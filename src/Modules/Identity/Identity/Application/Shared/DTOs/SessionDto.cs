namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing session information for display to users.
/// </summary>
/// <param name="Id">The unique identifier of the session.</param>
/// <param name="IpAddress">IP address where the login happened.</param>
/// <param name="DeviceName">Friendly device name (e.g., "Chrome on Windows").</param>
/// <param name="UserAgent">Raw user agent string from the browser/device.</param>
/// <param name="ClientPlatform">Client platform identifier (e.g., "ios-mobile", "android-mobile", "browser-web", "pwa-browser").</param>
/// <param name="CreatedAt">When this session was created (login time).</param>
/// <param name="ExpiresAt">When this session expires.</param>
/// <param name="IsActive">Whether this session is currently active (not deleted and not expired).</param>
public record SessionDto(
    Guid Id,
    string? IpAddress,
    string? DeviceName,
    string? UserAgent,
    string? ClientPlatform,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive
);
