namespace _116.Content.Application.Shared.Services;

/// <summary>
/// Raised by <see cref="IStreamingLinkResolutionService" /> implementations when the external
/// provider cannot serve a resolution — unreachable, rate-limited, or an unrecognised source
/// URL. Part of the port contract: handlers catch it and translate to a localized error, so
/// the infrastructure layer never touches i18n.
/// </summary>
/// <param name="message">A provider-level description of what failed.</param>
/// <param name="isRateLimited">Whether the provider rejected the call for rate-limit reasons.</param>
public class StreamingLinkResolutionException(string message, bool isRateLimited = false) : Exception(message)
{
    /// <summary>
    /// Whether the provider rejected the call for rate-limit reasons — surfaced separately so
    /// the admin is told to wait rather than to retry immediately.
    /// </summary>
    public bool IsRateLimited => isRateLimited;
}
