using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddArticleComment;

/// <summary>
/// Command to post a comment on an article.
/// </summary>
/// <param name="ArticleId">The unique identifier of the article being commented on.</param>
/// <param name="UserId">The identity user UUID of the commenter.</param>
/// <param name="Body">The text body of the comment.</param>
public record PublicAddArticleCommentCommand(Guid ArticleId, Guid UserId, string Body)
    : ICommand<PublicAddArticleCommentResult>;

/// <summary>
/// Result of the <see cref="PublicAddArticleCommentCommand" />.
/// </summary>
/// <param name="Comment">The newly created comment DTO.</param>
public record PublicAddArticleCommentResult(ArticleCommentDto Comment);
