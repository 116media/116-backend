using Microsoft.Extensions.Localization;

namespace _116.Content.Application.Shared.Errors.Messages;

/// <summary>
/// Provides error messages for the lyrics translation and community review domain.
/// Covers not-found lookups and validation failures for translations and their revisions.
/// </summary>
public class TranslationErrorMessage(IStringLocalizer<TranslationErrorMessage> localizer)
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

    /// <summary>
    /// Gets an error message for when a moderator decides a translation revision that has already
    /// been accepted or rejected.
    /// </summary>
    public string AlreadyDecided() => localizer["AlreadyDecided"];

    /// <summary>
    /// Gets an error message for when a vote comment exceeds the maximum length.
    /// </summary>
    /// <param name="max">The maximum allowed length.</param>
    public string VoteCommentTooLong(int max) => string.Format(localizer["VoteCommentTooLong"], max);
}
