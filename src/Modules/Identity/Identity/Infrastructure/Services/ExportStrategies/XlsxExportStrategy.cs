using System.Dynamic;
using System.Reflection;

using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

using ClosedXML.Excel;

namespace _116.Identity.Infrastructure.Services.ExportStrategies;

/// <summary>
/// Strategy for exporting session data to XLSX (Excel) format.
/// </summary>
public class XlsxExportStrategy : IExportStrategy
{
    /// <inheritdoc />
    public byte[] Export(List<SessionExportDto> sessions, List<string>? columns)
    {
        using var workbook = new XLWorkbook();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Sessions");

        if (columns is null || columns.Count == 0)
        {
            // Export all columns using InsertData - efficiently handles bulk insertion
            worksheet.Cell(row: 1, column: 1).InsertData(data: sessions, transpose: false);
        }
        else
        {
            // Project to filtered data and export
            List<ExpandoObject> filteredData = ProjectToFilteredData(sessions: sessions, columns: columns);
            worksheet.Cell(row: 1, column: 1).InsertData(data: filteredData, transpose: false);
        }

        // Style headers
        IXLRow headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Auto-fit columns
        worksheet.Columns().AdjustToContents();

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(stream: memoryStream);

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Projects session data to include only specified columns using dynamic objects.
    /// This is done once upfront to avoid nested loops during XLSX writing.
    /// </summary>
    /// <param name="sessions">The session data.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>Projected data as dynamic objects.</returns>
    private static List<ExpandoObject> ProjectToFilteredData(List<SessionExportDto> sessions, List<string> columns)
    {
        PropertyInfo[] properties = typeof(SessionExportDto)
            .GetProperties()
            .Where(p => columns.Contains(value: p.Name, comparer: StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return sessions.Select(session =>
        {
            IDictionary<string, object?> expando = new ExpandoObject();

            foreach (PropertyInfo property in properties)
            {
                expando[property.Name] = property.GetValue(obj: session);
            }

            return (ExpandoObject)expando;
        }).ToList();
    }
}
