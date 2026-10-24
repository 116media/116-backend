using System.Dynamic;
using System.Reflection;
using _116.Identity.Application.Shared.DTOs;

namespace _116.Identity.Infrastructure.Services.SessionExport;

public class SessionExportBase
{
    /// <summary>
    /// Projects session data to include only specified columns using dynamic objects.
    /// </summary>
    /// <param name="sessions">The session data.</param>
    /// <param name="columns">The columns to include.</param>
    /// <returns>Projected data as dynamic objects.</returns>
    protected static List<ExpandoObject> GetFilteredColumns(List<SessionExportDto> sessions, List<string> columns)
    {
        PropertyInfo[] properties = typeof(SessionExportDto)
            .GetProperties()
            .Where(p => columns.Contains(value: p.Name, comparer: StringComparer.OrdinalIgnoreCase))
            .ToArray();

        return sessions
            .Select(session =>
            {
                var expando = new ExpandoObject();
                IDictionary<string, object?> columnValues = expando;

                foreach (PropertyInfo property in properties)
                {
                    columnValues[property.Name] = property.GetValue(obj: session);
                }

                return expando;
            })
            .ToList();
    }
}
