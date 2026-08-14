using _116.Content.Application.Shared.Exceptions;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Shared.Services;

/// <summary>
/// Resolves one verified platform URL into deep links across every streaming platform, via an
/// external link-aggregation provider. Called once per admin resolve action — never from a
/// public read path, which keeps third-party latency and rate limits off public routes.
/// </summary>
public interface IStreamingLinkResolutionService
{
    /// <summary>
    /// Returns the platform-to-URL pairs the provider could match for the given source URL.
    /// Platforms the release is not on are simply absent from the result. Throws
    /// <see cref="StreamingLinkResolutionException" /> when the provider is unreachable,
    /// rate-limited, or does not recognise the URL.
    /// </summary>
    /// <param name="sourceUrl">A verified track or album URL on any supported platform.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The resolved deep links, keyed by platform.</returns>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        string sourceUrl,
        CancellationToken cancellationToken = default
    );
}
