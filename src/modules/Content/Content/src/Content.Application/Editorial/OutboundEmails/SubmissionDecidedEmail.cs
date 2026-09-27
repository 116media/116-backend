using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Editorial.OutboundEmails;

/// <summary>
/// A lyrics submission was reviewed: the outcome, with whatever note the reviewer left.
/// </summary>
/// <param name="Submitter">The user who submitted the lyrics.</param>
/// <param name="SongTitle">The song the submission covers.</param>
/// <param name="Outcome">The review outcome as the copy words it.</param>
/// <param name="ReviewNote">The reviewer's note, empty when none was left.</param>
public record SubmissionDecidedEmail(EmailRecipient Submitter, string SongTitle, string Outcome, string ReviewNote)
    : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.SubmissionDecided;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [Submitter];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = Submitter.DisplayName ?? string.Empty,
            ["songTitle"] = SongTitle,
            ["outcome"] = Outcome,
            ["reviewNote"] = ReviewNote,
        };
}
