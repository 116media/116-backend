namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing session information for display to users.
/// </summary>
/// <param name="Id">The unique identifier of the session.</param>
/// <param name="IpAddress">IP address where the login happened.</param>
/// <param name="UserAgent">Raw user agent string from the browser/device.</param>
/// <param name="Browser">Browser type detected from the user agent.</param>
/// <param name="Device">Device type detected from the user agent.</param>
/// <param name="Platform">Platform/OS detected from the user agent.</param>
/// <param name="Client">Client application type that initiated the session.</param>
/// <param name="CreatedAt">When this session was created (login time).</param>
/// <param name="ExpiresAt">When this session expires.</param>
/// <param name="IsActive">Whether this session is currently active (not deleted and not expired).</param>
public record SessionDto(
    Guid Id,
    string? IpAddress,
    string? UserAgent,
    string Browser,
    string Device,
    string Platform,
    string Client,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive
);
