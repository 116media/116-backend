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

/// <summary>
/// Swagger document filter that replaces inlined enum schemas in DTO
/// properties with <c>$ref</c> references to their component definitions.
/// </summary>
/// <remarks>
/// Swashbuckle inlines non-nullable enum schemas into DTO properties
/// instead of emitting a <c>$ref</c>. This causes code generators
/// (swagger-typescript-api, swagger_dart_code_generator) to produce
/// string literal unions rather than referencing the shared enum type.
///
/// This filter runs after all schemas are generated, detects inlined
/// enum properties that match a component schema, and replaces them
/// with a proper <c>$ref</c> reference.
/// </remarks>
public class EnumReferenceDocumentFilter : IDocumentFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        Dictionary<string, string> enumSignatures = BuildEnumSignatures(swaggerDoc);

        foreach (OpenApiSchema schema in swaggerDoc.Components.Schemas.Values)
        {
            ReplaceInlinedEnums(schema, enumSignatures);
        }
    }

    /// <summary>
    /// Builds a lookup from enum value signature to component schema name.
    /// </summary>
    private static Dictionary<string, string> BuildEnumSignatures(OpenApiDocument doc)
    {
        var signatures = new Dictionary<string, string>();

        foreach ((string name, OpenApiSchema schema) in doc.Components.Schemas)
        {
            if (schema.Type != "string" || schema.Enum.Count == 0)
            {
                continue;
            }

            string signature = BuildSignature(schema);
            signatures.TryAdd(signature, name);
        }

        return signatures;
    }

    /// <summary>
    /// Recursively replaces inlined enum properties with $ref references.
    /// </summary>
    private static void ReplaceInlinedEnums(OpenApiSchema schema, Dictionary<string, string> enumSignatures)
    {
        if (schema.Properties == null)
        {
            return;
        }

        List<string> propertyNames = [.. schema.Properties.Keys];

        foreach (string propName in propertyNames)
        {
            OpenApiSchema propSchema = schema.Properties[propName];

            if (propSchema.Type == "string" && propSchema.Enum.Count > 0 && propSchema.Reference == null)
            {
                string signature = BuildSignature(propSchema);

                if (enumSignatures.TryGetValue(signature, out string? componentName))
                {
                    schema.Properties[propName] = new OpenApiSchema
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = componentName },
                    };
                }
            }
            else
            {
                ReplaceInlinedEnums(propSchema, enumSignatures);
            }
        }
    }

    /// <summary>
    /// Creates a stable signature from an enum schema's sorted values.
    /// </summary>
    private static string BuildSignature(OpenApiSchema schema)
    {
        List<string> values = schema.Enum.OfType<OpenApiString>().Select(e => e.Value).OrderBy(v => v).ToList();

        return string.Join("|", values);
    }
}
