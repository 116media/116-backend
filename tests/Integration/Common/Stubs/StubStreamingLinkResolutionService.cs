using _116.Content.Application.Shared.Exceptions;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Enums;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub replacing the Odesli-backed resolution service so integration tests never
/// call the real provider. Behaviour is scripted per test through the instance hooks below,
/// which the base classes reset before each test.
/// </summary>
public class StubStreamingLinkResolutionService : IStreamingLinkResolutionService, IResettableStub
{
    /// <summary>
    /// The platform links the next resolutions return. Defaults to every modelled platform
    /// so the happy path needs no arrangement.
    /// </summary>
    public IReadOnlyDictionary<EnumStreamingPlatform, string> NextResult { get; set; } = DefaultResult();

    /// <summary>
    /// When set, the next resolutions throw this instead of returning
    /// <see cref="NextResult" />.
    /// </summary>
    public StreamingLinkResolutionException? NextException { get; set; }

    /// <summary>
    /// When set, the next resolutions throw this instead of returning <see cref="NextResult" />.
    /// Unlike <see cref="NextException" /> this carries an arbitrary, unmapped exception, so a test
    /// can drive whatever escapes a handler through the real global exception pipeline (for example
    /// an unexpected fault reaching the fallback strategy, or a cancellation).
    /// </summary>
    public Exception? NextUnhandledException { get; set; }

    /// <inheritdoc />
    public void Reset()
    {
        NextResult = DefaultResult();
        NextException = null;
        NextUnhandledException = null;
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default
    )
    {
        if (NextUnhandledException is not null)
        {
            throw NextUnhandledException;
        }

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
