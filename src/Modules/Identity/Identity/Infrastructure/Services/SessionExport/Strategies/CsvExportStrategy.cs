using System.Dynamic;
using System.Globalization;
using System.Text;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using CsvHelper;

namespace _116.Identity.Infrastructure.Services.SessionExport.Strategies;

/// <summary>
/// Strategy for exporting session data to CSV format.
/// </summary>
public class CsvExportStrategy : SessionExportBase, IExportStrategy
{
    /// <inheritdoc />
    public byte[] Export(List<SessionExportDto> sessions, List<string>? columns)
    {
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(stream: memoryStream, encoding: Encoding.UTF8);
        using var csvWriter = new CsvWriter(writer: streamWriter, culture: CultureInfo.InvariantCulture);

        if (columns is null || columns.Count == 0)
        {
            // Export all columns using built-in WriteRecords
            csvWriter.WriteRecords(records: sessions);
        }
        else
        {
            // Project to filtered data and export
            List<ExpandoObject> filteredData = GetFilteredColumns(sessions: sessions, columns: columns);
            csvWriter.WriteRecords(records: filteredData);
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }
}
