using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

namespace _116.Identity.Application.Session.Services;

/// <summary>
/// Strategy interface for exporting session data to different formats.
/// </summary>
public interface IExportStrategy
{
    /// <summary>
    /// Exports session data to a specific format.
    /// </summary>
    /// <param name="sessions">The session data to export.</param>
    /// <param name="columns">Optional list of column names to export. If null, export all columns.</param>
    /// <returns>File content as a byte array.</returns>
    byte[] Export(List<SessionExportDto> sessions, List<string>? columns);
}
