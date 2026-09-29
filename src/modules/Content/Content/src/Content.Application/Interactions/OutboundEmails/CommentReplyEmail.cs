using _116.Content.Application.Shared.OutboundEmails;
using _116.Mailer.Contracts.Application.OutboundEmails;

namespace _116.Content.Application.Interactions.OutboundEmails;

/// <summary>
/// Someone replied to a comment: a courtesy to the parent comment's author.
/// </summary>
/// <param name="ParentAuthor">The author of the comment that was replied to.</param>
/// <param name="ReplierName">The display name of whoever replied.</param>
/// <param name="ArticleTitle">The article the conversation sits on.</param>
/// <param name="ReplyExcerpt">The opening of the reply.</param>
/// <param name="ArticleUrl">Where the conversation can be read.</param>
public record CommentReplyEmail(
    EmailRecipient ParentAuthor,
    string ReplierName,
    string ArticleTitle,
    string ReplyExcerpt,
    string ArticleUrl
) : OutboundEmail
{
    /// <inheritdoc />
    public override EnumEmailClass Class => EnumEmailClass.Notification;

    /// <inheritdoc />
    public override string TemplateName => ContentEmailTemplates.CommentReply;

    /// <inheritdoc />
    public override IReadOnlyList<EmailRecipient> Recipients => [ParentAuthor];

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
