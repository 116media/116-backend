using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Swagger schema filter that converts enum schemas to use string values
/// matching the JsonStringEnumConverter runtime behavior.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
        {
            return;
        }

        string[] enumNames = Enum.GetNames(context.Type);

        schema.Type = "string";
        schema.Format = null;
        schema.Enum.Clear();

        foreach (string name in enumNames)
        {
            schema.Enum.Add(new OpenApiString(name));
        }
    }
}
