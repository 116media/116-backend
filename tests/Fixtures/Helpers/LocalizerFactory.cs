using Microsoft.Extensions.DependencyInjection;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Builds real <c>*ErrorMessage</c> instances backed by embedded .resx resources for use in unit tests.
/// Avoids the need to mock <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/> in tests.
/// </summary>
public static class LocalizerFactory
{
    /// <summary>
    /// Creates a real <typeparamref name="T"/> instance resolved against the given culture.
    /// The message class must be registered as a scoped service and must accept
    /// <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/> via its primary constructor.
    /// </summary>
    /// <typeparam name="T">
    /// The <c>*ErrorMessage</c> class to instantiate (e.g., <c>ValidationErrorMessage</c>).
    /// </typeparam>
    /// <param name="culture">
    /// The culture to use when resolving strings (e.g., "en", "fr"). Defaults to "en".
    /// </param>
    /// <returns>
    /// A real instance of <typeparamref name="T"/> with strings resolved from .resx for the given culture.
    /// </returns>
    public static T CreateMessage<T>(string culture = "en")
        where T : class
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddLocalization();
        services.AddScoped<T>();

        ServiceProvider sp = services.BuildServiceProvider();
        using var scope = new CultureScope(culture);
        return sp.GetRequiredService<T>();
    }
}
