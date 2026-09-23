namespace _116.Mailer.Contracts.Application.Messages;

/// <summary>
/// One thing that happened, described once: its delivery policy, its recipients, the template
/// its copy comes from, and the tokens that copy renders with.
/// </summary>
public abstract record Message
{
    /// <summary>
    /// The delivery policy this message is sent under.
    /// </summary>
    public abstract EnumMessageClass Class { get; }

    /// <summary>
    /// The template name the copy is resolved from, in every supported culture.
    /// </summary>
    public abstract string TemplateName { get; }

    /// <summary>
    /// Who receives this message; each recipient carries their own locale.
    /// </summary>
    public abstract IReadOnlyList<MessageRecipient> Recipients { get; }

    /// <summary>
    /// The dynamic values the template requires.
    /// </summary>
    public abstract IReadOnlyDictionary<string, string> Tokens { get; }
}
