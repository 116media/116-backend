using System.Globalization;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Sets the formatting culture and the resource-lookup culture for the duration of a test
/// and restores both on dispose. Tests run on pooled threads reused across collections, so
/// a culture left behind changes the meaning of whatever test runs next.
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previousCulture;
    private readonly CultureInfo _previousUiCulture;

    /// <summary>
    /// Initializes a new instance, setting both cultures to the specified culture name.
    /// </summary>
    /// <param name="cultureName">
    /// The culture name to set (e.g., "en", "fr").
    /// </param>
    public CultureScope(string cultureName)
    {
        _previousCulture = CultureInfo.CurrentCulture;
        _previousUiCulture = CultureInfo.CurrentUICulture;

        var culture = new CultureInfo(cultureName);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CultureInfo.CurrentCulture = _previousCulture;
        CultureInfo.CurrentUICulture = _previousUiCulture;
    }
}
