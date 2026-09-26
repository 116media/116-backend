using _116.Tests.Fixtures.Helpers;

namespace _116.Integration.Tests.Common.Helpers;

/// <summary>
/// Resolves the application's own <c>*ErrorMessage</c> / <c>*ExceptionMessage</c> classes out of
/// the test host's container and invokes them under an explicit culture, so a test asserts the
/// string the application would have produced instead of a hardcoded copy of the .resx text.
/// </summary>
public static class LocalizedMessage
{
    /// <summary>
    /// The culture the host falls back to when a request carries no <c>Accept-Language</c>
    /// header, mirroring the default culture configured by <c>AddAppLocalization</c>.
    /// A request without the header is answered in this culture, so an assertion on such a
    /// response must resolve its expected detail in this culture too.
    /// </summary>
    public const string DefaultCulture = "fr";

    /// <summary>
    /// The culture a request selects by sending <c>Accept-Language: en</c>.
    /// </summary>
    public const string EnglishCulture = "en";

    /// <summary>
    /// Resolves <typeparamref name="TMessage" /> from the host container and invokes
    /// <paramref name="select" /> against it while <paramref name="culture" /> is the ambient
    /// culture. The message classes read the ambient UI culture at call time, not at resolution
    /// time, so the scope has to stay open across the invocation, not merely across the resolve.
    /// </summary>
    /// <typeparam name="TMessage">The message class to resolve (e.g. <c>ArticleErrorMessage</c>).</typeparam>
    /// <param name="services">The test host's root service provider.</param>
    /// <param name="select">Selects the message method to invoke, with its arguments.</param>
    /// <param name="culture">The culture to resolve the message in.</param>
    /// <returns>The localized message text.</returns>
    public static string Resolve<TMessage>(IServiceProvider services, Func<TMessage, string> select, string culture)
        where TMessage : notnull
    {
        using var cultureScope = new CultureScope(culture);
        using IServiceScope scope = services.CreateScope();

        return select(scope.ServiceProvider.GetRequiredService<TMessage>());
    }
}
