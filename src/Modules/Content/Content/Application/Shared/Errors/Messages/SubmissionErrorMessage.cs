using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the community lyrics submission domain.
/// Covers not-found lookups and status-transition failures for a
/// <c>LyricsSubmissionEntity</c> moving through the moderation queue.
/// </summary>
public class SubmissionErrorMessage(IStringLocalizer<SubmissionErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a moderator attempts to decide a submission that has
    /// already left the <c>Pending</c> status.
    /// </summary>
    public string NotPending() => localizer["NotPending"];
}
