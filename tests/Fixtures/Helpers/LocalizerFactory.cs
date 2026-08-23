using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace _116.Tests.Fixtures.Helpers;

/// <summary>
/// Builds real <c>*ErrorMessage</c> instances backed by embedded .resx resources for use in unit tests.
/// Avoids the need to mock <see cref="Microsoft.Extensions.Localization.IStringLocalizer{T}"/> in tests.
/// </summary>
public static class LocalizerFactory
{
    private static readonly IServiceProvider Provider = new ServiceCollection()
        .AddLogging()
        .AddLocalization()
        .BuildServiceProvider();

    private static readonly ConcurrentDictionary<Type, object> Cache = new();

    /// <summary>
    /// Returns the shared <typeparamref name="T"/> message instance, backed by the real embedded
    /// .resx resources. Strings resolve at access time against the ambient UI culture, so a test
    /// that needs a specific catalogue wraps its assertion — not this call — in a <see cref="CultureScope"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The <c>*ErrorMessage</c> class to instantiate (e.g., <c>ValidationErrorMessage</c>).
    /// </typeparam>
    /// <returns>
    /// The process-wide <typeparamref name="T"/> instance; these message classes are stateless
    /// wrappers over a thread-safe localizer, so one instance is shared across tests.
    /// </returns>
    public static T CreateMessage<T>()
        where T : class =>
        (T)Cache.GetOrAdd(typeof(T), static type => ActivatorUtilities.CreateInstance(Provider, type));
}
