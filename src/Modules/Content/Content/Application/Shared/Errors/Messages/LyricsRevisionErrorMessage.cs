using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the lyrics-text community correction domain.
/// Covers not-found lookups and validation failures for a lyrics revision and its votes.
/// </summary>
public class LyricsRevisionErrorMessage(IStringLocalizer<LyricsRevisionErrorMessage> localizer)
{
    /// <summary>
    /// Exposes the underlying localizer for shared validation extensions.
    /// </summary>
    public IStringLocalizer Localizer => localizer;

    /// <summary>
    /// Gets an error message for when a user attempts to vote twice on the same revision.
    /// </summary>
    public string AlreadyVoted() => localizer["AlreadyVoted"];

    /// <summary>
    /// Gets an error message for when a proposed revision's replacement text is required but
    /// not provided.
    /// </summary>
    public string ProposedTextRequired() => localizer["ProposedTextRequired"];
}
