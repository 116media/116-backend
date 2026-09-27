using _116.Identity.Application.Session.Constants;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Enums;
using _116.Identity.Infrastructure.Services.SessionExport.Strategies;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Service for exporting session data to file formats using the Strategy pattern.
/// </summary>
public class SessionExportService : ISessionExportService
{
    private readonly Dictionary<EnumSessionExportFormat, IExportStrategy> _strategies = new()
    {
        [key: EnumSessionExportFormat.Csv] = new CsvExportStrategy(),
        [key: EnumSessionExportFormat.Xlsx] = new XlsxExportStrategy(),
    };

    /// <inheritdoc />
    public byte[] Export(List<SessionExportDto> sessions, EnumSessionExportFormat format, List<string>? columns = null)
    {
        if (!_strategies.TryGetValue(key: format, out IExportStrategy? strategy))
        {
            throw new ArgumentException($"No export strategy found for format: {format}", nameof(format));
        }

        return strategy.Export(sessions: sessions, columns: columns);
    }

    /// <inheritdoc />
    public string GetContentType(EnumSessionExportFormat format)
    {
        return format switch
        {
            EnumSessionExportFormat.Csv => "text/csv",
            EnumSessionExportFormat.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format)),
        };
    }

    /// <inheritdoc />
    public string GenerateFileName(EnumSessionExportFormat format, string? baseFileName = null)
    {
        string fileName = baseFileName ?? SessionExportConstants.DefaultBaseFileName;

        string fileExtension = format switch
        {
            EnumSessionExportFormat.Csv => "csv",
            EnumSessionExportFormat.Xlsx => "xlsx",
            _ => throw new ArgumentException($"Unsupported export format: {format}", nameof(format)),
        };

        return $"{fileName}-{DateTime.UtcNow:yyyyMMddHHmmss}.{fileExtension}";
    }
}
