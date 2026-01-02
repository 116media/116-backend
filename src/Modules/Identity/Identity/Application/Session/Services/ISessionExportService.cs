using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Domain.Enums;

namespace _116.Identity.Application.Session.Services;

/// <summary>
/// Service for exporting session data to various formats (CSV, XLSX).
/// </summary>
public interface ISessionExportService
{
    /// <summary>
    /// Exports session data to CSV format.
    /// </summary>
    /// <param name="sessions">The session data to export.</param>
    /// <param name="columns">Optional list of column names to export. If null, export all columns.</param>
    /// <returns>CSV file content as a byte array.</returns>
    byte[] ExportToCsv(List<SessionExportDto> sessions, List<string>? columns = null);

    /// <summary>
    /// Exports session data to XLSX (Excel) format.
    /// </summary>
    /// <param name="sessions">The session data to export.</param>
    /// <param name="columns">Optional list of column names to export. If null, export all columns.</param>
    /// <returns>XLSX file content as byte array.</returns>
    byte[] ExportToXlsx(List<SessionExportDto> sessions, List<string>? columns = null);

    /// <summary>
    /// Exports session data to the specified format.
    /// </summary>
    /// <param name="sessions">The session data to export.</param>
    /// <param name="format">The export format.</param>
    /// <param name="columns">Optional list of column names to export. If null, export all columns.</param>
    /// <returns>File content as a byte array.</returns>
    byte[] Export(List<SessionExportDto> sessions, SessionExportFormat format, List<string>? columns = null);

    /// <summary>
    /// Gets the content type (MIME type) for the export format.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <returns>Content type string.</returns>
    string GetContentType(SessionExportFormat format);

    /// <summary>
    /// Generates a timestamped filename for the export.
    /// </summary>
    /// <param name="format">The export format.</param>
    /// <param name="baseFileName">The base file name without extension. Uses constant if null.</param>
    /// <returns>Filename with timestamp and extension.</returns>
    string GenerateFileName(SessionExportFormat format, string? baseFileName = null);
}
