using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Identity.Contracts.Application;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.AddCommentReply;

/// <summary>
/// Handles the <see cref="PublicAddCommentReplyCommand" /> to post a single-level reply to a
/// top-level article comment. Enforces that the parent exists, belongs to the given article,
/// and is itself a top-level comment (replies to replies are rejected). The created reply is
/// returned with its author resolved through the same cross-module mechanism used elsewhere.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-articles cache after the comment count changes.</param>
/// <param name="userLookup">Cross-module service for resolving the replier's profile.</param>
/// <param name="fileRepository">Repository for resolving the replier's avatar URL.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicAddCommentReplyHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<PublicAddCommentReplyCommand, PublicAddCommentReplyResult>
{
    /// <inheritdoc />
    public async Task<PublicAddCommentReplyResult> Handle(
        PublicAddCommentReplyCommand command,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity article = await articleRepository.GetByIdOrThrowAsync(
            id: command.ArticleId,
            cancellationToken: cancellationToken
        );

        ArticleCommentEntity? parent = await articleRepository.GetCommentByIdAsync(
            commentId: command.ParentCommentId,
            cancellationToken: cancellationToken
        );

        if (parent is null || parent.ArticleId != command.ArticleId || parent.IsDeleted)
        {
            throw i18n.ArticleInteraction.CommentNotFound(command.ParentCommentId);
        }

        if (parent.ParentCommentId is not null)
        {
            throw i18n.ArticleInteraction.CannotReplyToReply();
        }

        var reply = ArticleCommentEntity.CreateReply(
            id: Guid.NewGuid(),
            userId: command.UserId,
            articleId: command.ArticleId,
            parentCommentId: command.ParentCommentId,
            body: command.Body
        );

        await articleRepository.AddCommentAsync(comment: reply, cancellationToken: cancellationToken);

        article.IncrementCommentCount();
        articleRepository.Update(article: article);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        AuthorDto? author = await ResolveAuthorAsync(command.UserId, cancellationToken);
        ArticleCommentDto dto = reply.ToArticleCommentDto(mapper) with { Author = author };

        return new PublicAddCommentReplyResult(Reply: dto);
    }

    /// <summary>
    /// Resolves the replier's public author profile (user name, avatar URL, role). The email is
    /// never populated on the public projection. Returns null when the user cannot be resolved.
    /// </summary>
    /// <param name="userId">The replier's identity user id.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved author DTO, or null.</returns>
    private async Task<AuthorDto?> ResolveAuthorAsync(Guid userId, CancellationToken cancellationToken)
    {
        AuthorInfo? info = await userLookup.GetAuthorInfoByIdAsync(userId: userId, ct: cancellationToken);

        if (info is null)
        {
            return null;
        }

        string? avatarUrl = null;
        if (info.AvatarFileId.HasValue)
        {
            FileEntity? avatarFile = await fileRepository.GetByIdAsync(info.AvatarFileId.Value, cancellationToken);
            avatarUrl = avatarFile?.StorageUrl;
        }

        return new AuthorDto(UserName: info.UserName, Email: null, AvatarUrl: avatarUrl, Role: info.Role);
    }
}
