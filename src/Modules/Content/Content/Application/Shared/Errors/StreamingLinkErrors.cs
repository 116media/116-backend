using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Streaming link domain error factory providing simple, readable exception creation.
/// Usage: StreamingLinkErrors.ResolutionFailed() or StreamingLinkErrors.NothingResolved()
/// </summary>
public class StreamingLinkErrors(StreamingLinkErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public StreamingLinkErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when the external link-resolution provider is unreachable or answers with a
    /// non-success status.
    /// </summary>
    public BadGatewayException ResolutionFailed()
    {
        return new BadGatewayException(i18n.ResolutionFailed());
    }

    /// <summary>
    /// Throws when the external link-resolution provider is rate-limiting us — the admin
    /// should wait rather than retry immediately.
    /// </summary>
    public RateLimitExceededException ResolutionRateLimited()
    {
        return new RateLimitExceededException(i18n.ResolutionRateLimited(), retryAfter: TimeSpan.FromMinutes(1));
    }

    /// <summary>
    /// Throws when the pasted source URL is not a track or album link the provider
    /// recognises.
    /// </summary>
    public BadRequestException UnresolvableSourceUrl()
    {
        return new BadRequestException(i18n.UnresolvableSourceUrl());
    }

    /// <summary>
    /// Throws when the provider resolved no platforms at all for the source URL, so nothing
    /// was stored — surfaced instead of a silent success.
    /// </summary>
    public NotFoundException NothingResolved()
    {
        return new NotFoundException(i18n.NothingResolved());
    }
}
