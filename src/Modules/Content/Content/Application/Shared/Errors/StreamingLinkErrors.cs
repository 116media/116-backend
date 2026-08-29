using _116.Content.Application.Shared.Errors.Messages;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Errors;

/// <summary>
/// Streaming link domain error factory providing simple, readable exception creation.
/// Provider resolution failures are raised by the resolution service as
/// <c>StreamingLinkResolutionException</c> and mapped by its strategy handler, so they are not
/// created here.
/// </summary>
public class StreamingLinkErrors(StreamingLinkErrorMessage i18n)
{
    /// <summary>
    /// Exposes the localized message provider for use in validator extensions.
    /// </summary>
    public StreamingLinkErrorMessage Msg => i18n;

    /// <summary>
    /// Throws when the provider resolved no platforms at all for the source URL, so nothing
    /// was stored — surfaced instead of a silent success.
    /// </summary>
    public NotFoundException NothingResolved()
    {
        return new NotFoundException(i18n.NothingResolved());
    }
}
