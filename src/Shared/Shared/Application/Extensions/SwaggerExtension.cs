using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring Swagger documentation.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Contains Swagger configurations.
    /// </summary>
    /// <param name="options">The Swagger generation options.</param>
    /// <returns>The Swagger generation options for chaining.</returns>
    public static SwaggerGenOptions AddSwaggerOptions(this SwaggerGenOptions options)
    {
        options.AddSecurityDefinition(
            "Bearer",
            new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header uses Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            }
        );

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
                    },
                    Array.Empty<string>()
                },
            }
        );

        options.SupportNonNullableReferenceTypes();
        options.NonNullableReferenceTypesAsRequired();

        options.UseAllOfToExtendReferenceSchemas();
        options.SchemaFilter<EnumSchemaFilter>();

        return options;
    }
}
