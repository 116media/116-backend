using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the <c>StreamingLink</c> domain.
/// Covers failures of the external link-resolution provider and unusable source URLs.
/// </summary>
public class StreamingLinkErrorMessage(IStringLocalizer<StreamingLinkErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when the link-resolution provider is unreachable or answers
    /// with a non-success status.
    /// </summary>
    /// <returns>
    /// An error message indicating the provider could not be reached.
    /// </returns>
    public string ResolutionFailed()
    {
        return localizer["ResolutionFailed"];
    }

    /// <summary>
    /// Gets an error message for when the link-resolution provider is rate-limiting us.
    /// </summary>
    /// <returns>
    /// An error message telling the admin to wait before retrying.
    /// </returns>
    public string ResolutionRateLimited()
    {
        return localizer["ResolutionRateLimited"];
    }

    /// <summary>
    /// Gets an error message for when the pasted source URL is not one the provider
    /// recognises.
    /// </summary>
    /// <returns>
    /// An error message asking for a track or album link from a supported platform.
    /// </returns>
    public string UnresolvableSourceUrl()
    {
        return localizer["UnresolvableSourceUrl"];
    }

    /// <summary>
    /// Gets an error message for when the provider resolved no platforms for the source URL.
    /// </summary>
    /// <returns>
    /// An error message indicating nothing was found or stored.
    /// </returns>
    public string NothingResolved()
    {
        return localizer["NothingResolved"];
    }
}
