using System.Dynamic;
using _116.Identity.Application.Session.Services;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;
using ClosedXML.Excel;

namespace _116.Identity.Infrastructure.Services.SessionExport.Strategies;

/// <summary>
/// Strategy for exporting session data to XLSX (Excel) format.
/// </summary>
public class XlsxExportStrategy : SessionExportBase, IExportStrategy
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
            List<ExpandoObject> filteredData = GetFilteredColumns(sessions: sessions, columns: columns);
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
}
