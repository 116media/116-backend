namespace _116.Content.Domain.StateMachines;

/// <summary>
/// Stable identifiers for the content domain rules reported through
/// <see cref="Exceptions.DomainRuleException" />.
/// </summary>
public static class ContentRuleCodes
{
    /// <summary>
    /// The requested publication-state move is not in the transition table.
    /// Args: [0] content type, [1] source status, [2] target status.
    /// </summary>
    public const string InvalidStatusTransition = "content.invalid-status-transition";

    /// <summary>
    /// The content has moved past review and can no longer be edited.
    /// Args: [0] content type, [1] current status.
    /// </summary>
    public const string NotEditable = "content.not-editable";

    /// <summary>
    /// A video cannot publish without a YouTube URL attached. Args: none.
    /// </summary>
    public const string PublicationRequiresYoutubeUrl = "content.publication-requires-youtube-url";
}
