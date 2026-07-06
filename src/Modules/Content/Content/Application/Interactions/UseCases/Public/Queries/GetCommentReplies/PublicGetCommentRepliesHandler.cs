using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetCommentReplies;

/// <summary>
/// Handles the <see cref="PublicGetCommentRepliesQuery" /> to retrieve a paginated list of
/// non-deleted replies to a comment, enriching each reply with its commenter's author profile
/// (batch-resolved, no N+1) and, when a viewer is supplied, whether the viewer has liked it.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving commenter profiles.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicGetCommentRepliesHandler(
    IArticleRepository articleRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetCommentRepliesQuery, PublicGetCommentRepliesResult>
{
    /// <inheritdoc />
    public async Task<PublicGetCommentRepliesResult> Handle(
        PublicGetCommentRepliesQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<ArticleCommentEntity> replies, int totalCount) = await articleRepository.GetRepliesAsync(
            parentCommentId: query.CommentId,
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyDictionary<Guid, AuthorDto> authorsByUserId = await ResolveAuthorsAsync(replies, cancellationToken);

        IReadOnlyList<ArticleCommentDto> dtoList = replies.AsReadOnly().ToArticleCommentDtos(mapper, authorsByUserId);

        dtoList = await StampViewerLikesAsync(replies, dtoList, query.ViewerUserId, cancellationToken);

        var paginated = new PaginatedResult<ArticleCommentDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetCommentRepliesResult(Replies: paginated);
    }

    /// <summary>
    /// Batch-resolves the public author profile for every distinct non-deleted commenter on the
    /// page, keyed by commenter user id. Executes one identity lookup plus one avatar-URL lookup
    /// for the whole page (no N+1). Deleted comments are excluded so no identity is leaked, and
    /// the commenter email is never populated on the public projection.
    /// </summary>
    /// <param name="comments">The page of reply entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved author profiles keyed by commenter user id.</returns>
    private async Task<IReadOnlyDictionary<Guid, AuthorDto>> ResolveAuthorsAsync(
        IReadOnlyList<ArticleCommentEntity> comments,
        CancellationToken cancellationToken
    )
    {
        Guid[] userIds = comments.Where(c => !c.IsDeleted).Select(c => c.UserId).Distinct().ToArray();

        if (userIds.Length == 0)
        {
            return new Dictionary<Guid, AuthorDto>();
        }

        IReadOnlyDictionary<Guid, AuthorInfo> authorInfos = await userLookup.GetAuthorInfosByIdsAsync(
            userIds: userIds,
            ct: cancellationToken
        );

        Guid[] avatarFileIds = authorInfos
            .Values.Where(info => info.AvatarFileId.HasValue)
            .Select(info => info.AvatarFileId!.Value)
            .Distinct()
            .ToArray();

        IReadOnlyDictionary<Guid, string> avatarUrls =
            avatarFileIds.Length == 0
                ? new Dictionary<Guid, string>()
                : await fileRepository.GetStorageUrlsByIdsAsync(avatarFileIds, cancellationToken);

        return authorInfos.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                AuthorInfo info = pair.Value;
                string? avatarUrl = info.AvatarFileId.HasValue
                    ? avatarUrls.GetValueOrDefault(info.AvatarFileId.Value)
                    : null;

                return new AuthorDto(UserName: info.UserName, Email: null, AvatarUrl: avatarUrl, Role: info.Role);
            }
        );
    }

    /// <summary>
    /// Stamps each reply DTO with whether the current viewer has liked it, resolved for the
    /// whole page in one query. Anonymous viewers get all <c>IsLiked</c> false with no query.
    /// </summary>
    /// <param name="replies">The page of reply entities.</param>
    /// <param name="dtoList">The mapped reply DTOs to stamp.</param>
    /// <param name="viewerUserId">The current viewer's user id, or null when anonymous.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reply DTOs with the viewer's like state applied.</returns>
    private async Task<IReadOnlyList<ArticleCommentDto>> StampViewerLikesAsync(
        IReadOnlyList<ArticleCommentEntity> replies,
        IReadOnlyList<ArticleCommentDto> dtoList,
        Guid? viewerUserId,
        CancellationToken cancellationToken
    )
    {
        if (viewerUserId is not Guid viewerId || dtoList.Count == 0)
        {
            return dtoList;
        }

        Guid[] commentIds = replies.Select(c => c.Id).ToArray();

        IReadOnlySet<Guid> likedIds = await articleRepository.GetLikedCommentIdsAsync(
            viewerUserId: viewerId,
            commentIds: commentIds,
            cancellationToken: cancellationToken
        );

        return dtoList.Select(dto => dto with { IsLiked = likedIds.Contains(dto.Id) }).ToList();
    }
}
