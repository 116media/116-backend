using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Enums;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub replacing the Odesli-backed resolution service so integration tests never
/// call the real provider. Behaviour is scripted per test via the static hooks and reset in
/// each test's arrange step — the same external-service stub pattern as Cloudinary.
/// </summary>
public class StubStreamingLinkResolutionService : IStreamingLinkResolutionService
{
    /// <summary>
    /// The platform links the next resolutions return. Defaults to every modelled platform
    /// so the happy path needs no arrangement.
    /// </summary>
    public static IReadOnlyDictionary<EnumStreamingPlatform, string> NextResult { get; set; } = DefaultResult();

    /// <summary>
    /// When set, the next resolutions throw this instead of returning
    /// <see cref="NextResult" />.
    /// </summary>
    public static StreamingLinkResolutionException? NextException { get; set; }

    /// <summary>
    /// Restores the default happy-path behaviour. Call at the start of any test that
    /// scripted the hooks, so state never leaks between tests in the collection.
    /// </summary>
    public static void Reset()
    {
        NextResult = DefaultResult();
        NextException = null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default
    )
    {
        if (NextException is not null)
        {
            throw NextException;
        }

        return Task.FromResult(NextResult);
    }

    /// <summary>
    /// One https deep link per modelled platform.
    /// </summary>
    private static IReadOnlyDictionary<EnumStreamingPlatform, string> DefaultResult()
    {
        return Enum.GetValues<EnumStreamingPlatform>()
            .ToDictionary(platform => platform, platform => $"https://resolved.example/{platform}".ToLowerInvariant());
    }
}
