using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Identity.Contracts.Application;
using _116.Mailer.Contracts.Application;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Interactions.EventHandlers;

/// <summary>
/// Tells a top-level comment's author that someone replied, over email and the
/// in-app feed. Both channels are handled together because they share every
/// lookup. Self-replies notify nobody; unresolvable rows or authors skip
/// entirely; an author without an email address (OAuth accounts) still gets
/// the in-app row.
/// </summary>
/// <param name="articleRepository">Repository resolving the reply, its parent comment, and the article.</param>
/// <param name="userLookupService">Lookup resolving the parent author and the replier by id.</param>
/// <param name="mailer">Outbox mailer sending the reply notice.</param>
/// <param name="notifier">Writer for the in-app notification row.</param>
/// <param name="logger">Logger recording skipped deliveries.</param>
public class CommentReplyAddedNotificationsHandler(
    IArticleRepository articleRepository,
    IUserLookupService userLookupService,
    IMailer mailer,
    INotifier notifier,
    ILogger<CommentReplyAddedNotificationsHandler> logger
) : IDomainEventHandler<CommentReplyAddedEvent>
{
    /// <summary>
    /// The wording used when the replier's profile cannot be resolved.
    /// </summary>
    private const string FallbackReplierName = "Someone";

    /// <inheritdoc />
    public async Task Handle(CommentReplyAddedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        ArticleCommentEntity? parent = await articleRepository.GetCommentByIdAsync(
            commentId: domainEvent.ParentCommentId,
            cancellationToken: cancellationToken
        );

        if (parent is null || parent.UserId == domainEvent.ReplierUserId)
        {
            return;
        }

        ArticleCommentEntity? reply = await articleRepository.GetCommentByIdAsync(
            commentId: domainEvent.ReplyId,
            cancellationToken: cancellationToken
        );

        ArticleEntity? article = await articleRepository.GetByIdAsync(
            id: domainEvent.ArticleId,
            cancellationToken: cancellationToken
        );

        if (reply is null || article is null)
        {
            logger.LogDebug(
                "Comment reply notifications skipped: reply {ReplyId} or article {ArticleId} not found.",
                domainEvent.ReplyId,
                domainEvent.ArticleId
            );
            return;
        }

        AuthorInfo? parentAuthor = await userLookupService.GetAuthorInfoByIdAsync(
            userId: parent.UserId,
            ct: cancellationToken
        );

        if (parentAuthor is null)
        {
            logger.LogDebug("Comment reply notifications skipped: user {UserId} not found.", parent.UserId);
            return;
        }

        string? replierName = await userLookupService.GetUserNameByIdAsync(
            userId: domainEvent.ReplierUserId,
            ct: cancellationToken
        );

        string culture = EmailCulture.Current();

        if (parentAuthor.Email is not null)
        {
            await mailer.EnqueueAsync(
                template: EnumEmailTemplate.CommentReply,
                to: new EmailRecipient(Address: parentAuthor.Email, DisplayName: parentAuthor.UserName),
                tokens: new Dictionary<string, string>
                {
                    ["userName"] = parentAuthor.UserName,
                    ["replierName"] = replierName ?? FallbackReplierName,
                    ["articleTitle"] = article.Title,
                    ["replyExcerpt"] = Excerpt(reply.Body),
                    ["articleUrl"] = ContentPublicLinks.Article(article.Slug),
                },
                culture: culture,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            logger.LogDebug("Comment reply email skipped: user {UserId} has no email address.", parent.UserId);
        }

        await notifier.NotifyAsync(
            userId: parent.UserId,
            type: EnumNotificationType.CommentReply,
            tokens: new Dictionary<string, string>
            {
                ["replierName"] = replierName ?? FallbackReplierName,
                ["articleTitle"] = article.Title,
                ["linkPath"] = $"/articles/{article.Slug}",
            },
            culture: culture,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Truncates a reply body to 140 characters at a word boundary.
    /// </summary>
    /// <param name="body">The full reply text.</param>
    /// <returns>The body unchanged when short enough, otherwise a word-boundary cut with an ellipsis.</returns>
    internal static string Excerpt(string body)
    {
        const int maxLength = 140;

        if (body.Length <= maxLength)
        {
            return body;
        }

        int cut = body.LastIndexOf(' ', maxLength);
        return $"{body[..(cut > 0 ? cut : maxLength)]}…";
    }
}
