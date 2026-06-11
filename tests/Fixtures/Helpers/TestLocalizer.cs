using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Creates real IStringLocalizer instances backed by embedded .resx files in a given assembly.
/// Used in tests to assert against actual localized strings without hardcoding.
/// </summary>
public static class TestLocalizer
{
    /// <summary>
    /// Builds a real <see cref="IStringLocalizer{T}"/> backed by the embedded .resx resources
    /// compiled into the assembly that contains <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The message class whose assembly contains the matching .resx resource.
    /// </typeparam>
    /// <param name="culture">
    /// The culture to use when resolving strings (e.g., "en", "fr"). Defaults to "en".
    /// </param>
    /// <returns>
    /// A real <see cref="IStringLocalizer{T}"/> resolved against the specified culture.
    /// </returns>
    public static IStringLocalizer<T> For<T>(string culture = "en")
        where T : class
    {
        var options = new OptionsWrapper<LocalizationOptions>(new LocalizationOptions());
        var factory = new ResourceManagerStringLocalizerFactory(options, NullLoggerFactory.Instance);
        using var scope = new CultureScope(culture);
        return (IStringLocalizer<T>)factory.Create(typeof(T));
    }
}
