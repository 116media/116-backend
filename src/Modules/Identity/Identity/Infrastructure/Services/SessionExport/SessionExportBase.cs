using System.Dynamic;
using System.Reflection;
using _116.Identity.Application.Session.UseCases.Admin.Queries.ExportSessionData;

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
                IDictionary<string, object?> expando = new ExpandoObject();

                foreach (PropertyInfo property in properties)
                {
                    expando[property.Name] = property.GetValue(obj: session);
                }

                return (ExpandoObject)expando;
            })
            .ToList();
    }
}
