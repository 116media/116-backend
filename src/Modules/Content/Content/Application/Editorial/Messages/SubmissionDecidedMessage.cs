using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Editorial.Messages;

/// <summary>
/// A lyrics submission was reviewed: the outcome, with whatever note the reviewer left.
/// </summary>
/// <param name="Submitter">The user who submitted the lyrics.</param>
/// <param name="SongTitle">The song the submission covers.</param>
/// <param name="Outcome">The review outcome as the copy words it.</param>
/// <param name="ReviewNote">The reviewer's note, empty when none was left.</param>
public record SubmissionDecidedMessage(MessageRecipient Submitter, string SongTitle, string Outcome, string ReviewNote)
    : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.SubmissionDecided;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [Submitter];

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
