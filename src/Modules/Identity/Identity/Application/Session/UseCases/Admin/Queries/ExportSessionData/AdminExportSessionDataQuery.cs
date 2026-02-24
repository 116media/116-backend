using _116.Identity.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

/// <summary>
/// Query to export session data with optional filtering.
/// </summary>
/// <param name="Status">Filter by status: "active", "expired", or null for all.</param>
/// <param name="FromDate">Optional filter for sessions created after this date.</param>
/// <param name="ToDate">Optional filter for sessions created before this date.</param>
/// <param name="Format">Export file format: csv, xlsx, etc. If null, returns JSON response.</param>
/// <param name="Columns">Comma-separated list of columns to export. If null/empty, exports all columns. Valid columns: Id, UserId, IpAddress, UserAgent, Browser, Device, Platform, Client, CreatedAt, ExpiresAt, IsActive, DeletedAt.</param>
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
