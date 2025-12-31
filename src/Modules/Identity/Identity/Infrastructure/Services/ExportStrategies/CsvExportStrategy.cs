using System.Dynamic;
using System.Globalization;
using System.Reflection;
using System.Text;

using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

using CsvHelper;

namespace _116.Identity.Infrastructure.Services.ExportStrategies;

/// <summary>
/// Strategy for exporting session data to CSV format.
/// </summary>
public class CsvExportStrategy : IExportStrategy
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
            var filteredData = ProjectToFilteredData(sessions: sessions, columns: columns);
            csvWriter.WriteRecords(records: filteredData);
        }

        streamWriter.Flush();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Projects session data to include only specified columns using dynamic objects.
    /// This is done once upfront to avoid nested loops during CSV writing.
    /// </summary>
    /// <param name="sessions">The session data.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>Projected data as dynamic objects.</returns>
    private static List<ExpandoObject> ProjectToFilteredData(List<SessionExportDto> sessions, List<string> columns)
    {
        var properties = typeof(SessionExportDto)
            .GetProperties()
            .Where(p => columns.Contains(value: p.Name, comparer: StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return sessions.Select(session =>
        {
            var expando = new ExpandoObject() as IDictionary<string, object?>;

            foreach (var property in properties)
            {
                expando[property.Name] = property.GetValue(obj: session);
            }

            return (ExpandoObject)expando;
        }).ToList();
    }
}
