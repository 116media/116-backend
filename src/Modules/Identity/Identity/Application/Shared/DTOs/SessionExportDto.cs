using _116.Identity.Domain.Enums;
using _116.Shared.Application.DTOs;

namespace _116.Identity.Application.Shared.DTOs;

/// <summary>
/// DTO for exporting session data.
/// </summary>
/// <param name="Id">Session unique identifier.</param>
/// <param name="UserId">User unique identifier.</param>
/// <param name="IpAddress">IP address of the session.</param>
/// <param name="UserAgent">Full user agent string.</param>
/// <param name="Browser">Browser type detected from the user agent.</param>
/// <param name="Device">Device type detected from the user agent.</param>
/// <param name="Platform">Platform/OS detected from the user agent.</param>
/// <param name="Client">Client application type that initiated the session.</param>
/// <param name="ExpiresAt">Session expiration timestamp.</param>
/// <param name="IsActive">Whether the session is currently active.</param>
/// <param name="DeletedAt">Soft delete timestamp if applicable.</param>
public record SessionExportDto(
    Guid Id,
    Guid UserId,
    string? IpAddress,
    string? UserAgent,
    EnumBrowser Browser,
    EnumDevice Device,
    EnumPlatform Platform,
    EnumClient Client,
    DateTime ExpiresAt,
    bool IsActive,
    DateTime? DeletedAt
) : AuditableDto;
