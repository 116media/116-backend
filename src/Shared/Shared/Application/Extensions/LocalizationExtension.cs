using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Shared.Application.Extensions;

/// <summary>
/// Extension methods for configuring request localization with supported cultures
/// and Accept-Language header detection.
/// </summary>
public static class LocalizationExtension
{
    /// <summary>
    /// The supported culture codes for the application.
    /// </summary>
    private static readonly string[] SupportedCultures = ["fr", "en"];

    /// <summary>
    /// The default culture used when no Accept-Language header is provided.
    /// </summary>
    private const string DefaultCulture = "fr";

    /// <summary>
    /// Registers localization services and configures supported cultures
    /// with Accept-Language header detection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAppLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = string.Empty);

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options
                .SetDefaultCulture(DefaultCulture)
                .AddSupportedCultures(SupportedCultures)
                .AddSupportedUICultures(SupportedCultures);

            options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
        });

        return services;
    }

    /// <summary>
    /// Adds the request localization middleware and a lightweight language-detection
    /// middleware that reads the Accept-Language header and stores the resolved
    /// language code in <c>HttpContext.Items["Language"]</c>.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseAppLocalization(this IApplicationBuilder app)
    {
        app.UseRequestLocalization();

        app.Use(
            async (context, next) =>
            {
                string? header = context.Request.Headers.AcceptLanguage.FirstOrDefault();

                string lang =
                    header?.Split(',').FirstOrDefault()?.Trim().StartsWith("en", StringComparison.OrdinalIgnoreCase)
                    == true
                        ? "en"
                        : DefaultCulture;

                context.Items["Language"] = lang;
                await next();
            }
        );

        return app;
    }
}
