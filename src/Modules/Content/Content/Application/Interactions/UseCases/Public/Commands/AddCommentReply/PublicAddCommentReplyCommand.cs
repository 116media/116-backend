using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Command to post a reply to an existing top-level article comment.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article being commented on.</param>
/// <param name="ParentCommentId">The top-level comment being replied to.</param>
/// <param name="UserId">The identity user UUID of the replier.</param>
/// <param name="Body">The text body of the reply.</param>
public record PublicAddCommentReplyCommand(Guid ArticleId, Guid ParentCommentId, Guid UserId, string Body)
    : ICommand<PublicAddCommentReplyResult>;

/// <summary>
/// Result of the <see cref="PublicAddCommentReplyCommand" />.
/// </summary>
/// <param name="Reply">The newly created reply DTO, with its author resolved.</param>
public record PublicAddCommentReplyResult(ArticleCommentDto Reply);
