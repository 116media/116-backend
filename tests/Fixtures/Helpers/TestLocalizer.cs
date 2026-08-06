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
    private static readonly IStringLocalizerFactory Factory = new ResourceManagerStringLocalizerFactory(
        new OptionsWrapper<LocalizationOptions>(new LocalizationOptions()),
        NullLoggerFactory.Instance
    );

    /// <summary>
    /// Builds a real <see cref="IStringLocalizer{T}"/> backed by the embedded .resx resources
    /// compiled into the assembly that contains <typeparamref name="T"/>. Strings resolve at
    /// access time against the ambient UI culture; pin it around the assertion with a <see cref="CultureScope"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The message class whose assembly contains the matching .resx resource.
    /// </typeparam>
    /// <returns>
    /// A real <see cref="IStringLocalizer{T}"/> over the embedded resources.
    /// </returns>
    public static IStringLocalizer<T> For<T>()
        where T : class => (IStringLocalizer<T>)Factory.Create(typeof(T));
}
