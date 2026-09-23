using _116.Content.Application.Shared.Messages;
using _116.Mailer.Contracts.Application.Messages;

namespace _116.Content.Application.Interactions.Messages;

/// <summary>
/// Someone replied to a comment: a courtesy to the parent comment's author.
/// </summary>
/// <param name="ParentAuthor">The author of the comment that was replied to.</param>
/// <param name="ReplierName">The display name of whoever replied.</param>
/// <param name="ArticleTitle">The article the conversation sits on.</param>
/// <param name="ReplyExcerpt">The opening of the reply.</param>
/// <param name="ArticleUrl">Where the conversation can be read.</param>
public record CommentReplyMessage(
    MessageRecipient ParentAuthor,
    string ReplierName,
    string ArticleTitle,
    string ReplyExcerpt,
    string ArticleUrl
) : Message
{
    /// <inheritdoc />
    public override EnumMessageClass Class => EnumMessageClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentMessageTemplates.CommentReply;

    /// <inheritdoc />
    public override IReadOnlyList<MessageRecipient> Recipients => [ParentAuthor];

    /// <inheritdoc />
    public override IReadOnlyDictionary<string, string> Tokens =>
        new Dictionary<string, string>
        {
            ["userName"] = ParentAuthor.DisplayName ?? string.Empty,
            ["replierName"] = ReplierName,
            ["articleTitle"] = ArticleTitle,
            ["replyExcerpt"] = ReplyExcerpt,
            ["articleUrl"] = ArticleUrl,
        };
}
