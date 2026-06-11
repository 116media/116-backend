using System.Globalization;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Sets CultureInfo.CurrentUICulture for the duration of a test and restores it on dispose.
/// </summary>
public sealed class CultureScope : IDisposable
{
    private readonly CultureInfo _previous;

    /// <summary>
    /// Initializes a new instance, setting the current UI culture to the specified culture name.
    /// </summary>
    /// <param name="cultureName">
    /// The culture name to set (e.g., "en", "fr").
    /// </param>
    public CultureScope(string cultureName)
    {
        _previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CultureInfo.CurrentUICulture = _previous;
    }
}
