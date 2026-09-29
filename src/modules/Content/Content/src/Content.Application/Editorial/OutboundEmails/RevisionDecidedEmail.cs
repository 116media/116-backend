using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Editorial.OutboundEmails;

/// <summary>
/// A proposed revision was accepted or rejected. Lyrics and translation revisions share this
/// record: the proposer is told the same thing either way, and the decision token carries which.
/// </summary>
/// <param name="Proposer">The user who proposed the revision.</param>
/// <param name="SongTitle">The song the revision belongs to.</param>
/// <param name="Decision">The decision as the copy words it.</param>
/// <param name="LyricsUrl">Where the lyrics can be read.</param>
public record RevisionDecidedEmail(EmailRecipient Proposer, string SongTitle, string Decision, string LyricsUrl)
    : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.RevisionDecided;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Proposer];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = Proposer.DisplayName ?? string.Empty,
            ["songTitle"] = SongTitle,
            ["decision"] = Decision,
            ["lyricsUrl"] = LyricsUrl,
        };
}
