using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using _116.Identity.Domain.Constants;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services.ExportStrategies;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service for exporting session data to file formats using the Strategy pattern.
/// </summary>
public class SessionExportService : ISessionExportService
{
    private readonly Dictionary<SessionExportFormat, IExportStrategy> _strategies = new()
    {
        [key: SessionExportFormat.Csv] = new CsvExportStrategy(),
        [key: SessionExportFormat.Xlsx] = new XlsxExportStrategy()
    };

    /// <inheritdoc />
    public byte[] ExportToCsv(List<SessionExportDto> sessions, List<string>? columns = null)
    {
        return _strategies[key: SessionExportFormat.Csv].Export(sessions: sessions, columns: columns);
    }

    /// <inheritdoc />
    public byte[] ExportToXlsx(List<SessionExportDto> sessions, List<string>? columns = null)
    {
        return _strategies[key: SessionExportFormat.Xlsx].Export(sessions: sessions, columns: columns);
    }

    /// <inheritdoc />
    public byte[] Export(List<SessionExportDto> sessions, SessionExportFormat format, List<string>? columns = null)
    {
        if (!_strategies.TryGetValue(key: format, out IExportStrategy? strategy))
        {
            throw new ArgumentException($"No export strategy found for format: {format}", nameof(format));
        }

        return strategy.Export(sessions: sessions, columns: columns);
    }

    /// <inheritdoc />
    public string GetContentType(SessionExportFormat format)
    {
        return format switch
        {
            SessionExportFormat.Csv => "text/csv",
            SessionExportFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };
    }

    /// <inheritdoc />
    public string GenerateFileName(SessionExportFormat format, string? baseFileName = null)
    {
        string fileName = baseFileName ?? SessionConstants.Export.DefaultBaseFileName;

        string fileExtension = format switch
        {
            SessionExportFormat.Csv => "csv",
            SessionExportFormat.Xlsx => "xlsx",
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format))
        };

        return $"{fileName}-{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
    }
}
