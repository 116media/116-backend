using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Query to export session data with optional filtering.
/// </summary>
/// <param name="Status">Filter by status: "active", "expired", or null for all.</param>
/// <param name="FromDate">Optional filter for sessions created after this date.</param>
/// <param name="ToDate">Optional filter for sessions created before this date.</param>
/// <param name="Format">Export file format: csv, xlsx, etc. If null, returns JSON response.</param>
/// <param name="Columns">Comma-separated list of columns to export. If null/empty, exports all columns. Valid columns: Id, UserId, IpAddress, DeviceName, UserAgent, ClientPlatform, CreatedAt, ExpiresAt, IsActive, DeletedAt.</param>
public record AdminExportSessionDataQuery(
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Format = null,
    string? Columns = null
) : IQuery<AdminExportSessionDataResult>;

/// <summary>
/// Result containing the exported session data.
/// </summary>
/// <param name="SessionData">List of session data for export.</param>
public record AdminExportSessionDataResult(List<SessionExportDto> SessionData);

/// <summary>
/// DTO for exporting session data.
/// </summary>
/// <param name="Id">Session unique identifier.</param>
/// <param name="UserId">User unique identifier.</param>
/// <param name="IpAddress">IP address of the session.</param>
/// <param name="DeviceName">Device name extracted from user agent.</param>
/// <param name="UserAgent">Full user agent string.</param>
/// <param name="ClientPlatform">Client platform identifier.</param>
/// <param name="CreatedAt">Session creation timestamp.</param>
/// <param name="ExpiresAt">Session expiration timestamp.</param>
/// <param name="IsActive">Whether the session is currently active.</param>
/// <param name="DeletedAt">Soft delete timestamp if applicable.</param>
public record SessionExportDto(
    Guid Id,
    Guid UserId,
    string? IpAddress,
    string? DeviceName,
    string? UserAgent,
    string? ClientPlatform,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsActive,
    DateTime? DeletedAt
);
