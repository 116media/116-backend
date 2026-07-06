using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Interactions.UseCases.Public.Queries.GetArticleComments;

/// <summary>
/// Handles the <see cref="PublicGetArticleCommentsQuery" /> to retrieve paginated article
/// comments, enriching each non-deleted comment with the commenter's author profile
/// (user name, avatar URL, role) resolved through the same cross-module mechanism used for
/// article authors. Commenter profiles and avatars are batch-resolved to avoid N+1 lookups.
/// When a viewer is supplied, each comment is also stamped with whether that viewer has
/// liked it, resolved in a single batch query.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="userLookup">Cross-module service for resolving commenter profiles.</param>
/// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
/// <param name="mapper">The mapper used to project entities to DTOs.</param>
public class PublicGetArticleCommentsHandler(
    IArticleRepository articleRepository,
    IUserLookupService userLookup,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetArticleCommentsQuery, PublicGetArticleCommentsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetArticleCommentsResult> Handle(
        PublicGetArticleCommentsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageIndex = query.PaginatedRequest.PageIndex;
        int pageSize = query.PaginatedRequest.PageSize;

        (List<ArticleCommentEntity> comments, int totalCount) = await articleRepository.GetCommentsAsync(
            articleId: query.ArticleId,
            page: pageIndex + 1,
            pageSize: pageSize,
            cancellationToken: cancellationToken
        );

        IReadOnlyDictionary<Guid, AuthorDto> authorsByUserId = await ResolveAuthorsAsync(comments, cancellationToken);

        IReadOnlyList<ArticleCommentDto> dtoList = comments.AsReadOnly().ToArticleCommentDtos(mapper, authorsByUserId);

        dtoList = await StampReplyCountsAsync(comments, dtoList, cancellationToken);

        dtoList = await StampViewerLikesAsync(comments, dtoList, query.ViewerUserId, cancellationToken);

        var paginated = new PaginatedResult<ArticleCommentDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetArticleCommentsResult(Comments: paginated);
    }

    /// <summary>
    /// Batch-resolves the public author profile for every distinct non-deleted commenter on the
    /// page, keyed by commenter user id. Executes one identity lookup plus one avatar-URL lookup
    /// for the whole page (no N+1). Deleted comments are excluded so no identity is leaked, and
    /// the commenter email is never populated on the public projection.
    /// </summary>
    /// <param name="comments">The page of comment entities.</param>
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
    /// Stamps each top-level comment DTO with its number of non-deleted direct replies,
    /// resolved for the whole page in one query.
    /// </summary>
    /// <param name="comments">The page of comment entities.</param>
    /// <param name="dtoList">The mapped comment DTOs to stamp.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The comment DTOs with reply counts applied.</returns>
    private async Task<IReadOnlyList<ArticleCommentDto>> StampReplyCountsAsync(
        IReadOnlyList<ArticleCommentEntity> comments,
        IReadOnlyList<ArticleCommentDto> dtoList,
        CancellationToken cancellationToken
    )
    {
        if (dtoList.Count == 0)
        {
            return dtoList;
        }

        Guid[] commentIds = comments.Select(c => c.Id).ToArray();

        IReadOnlyDictionary<Guid, int> replyCounts = await articleRepository.GetReplyCountsAsync(
            parentCommentIds: commentIds,
            cancellationToken: cancellationToken
        );

        if (replyCounts.Count == 0)
        {
            return dtoList;
        }

        return dtoList.Select(dto => dto with { ReplyCount = replyCounts.GetValueOrDefault(dto.Id) }).ToList();
    }

    /// <summary>
    /// Stamps each comment DTO with whether the current viewer has liked it. Resolves the
    /// viewer's liked comment ids for the whole page in one query. For an anonymous viewer
    /// (<paramref name="viewerUserId" /> null) the DTOs are returned unchanged with all
    /// <c>IsLiked</c> false.
    /// </summary>
    /// <param name="comments">The page of comment entities, aligned by index with <paramref name="dtoList" />.</param>
    /// <param name="dtoList">The mapped comment DTOs to stamp.</param>
    /// <param name="viewerUserId">The current viewer's user id, or null when anonymous.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The comment DTOs with the viewer's like state applied.</returns>
    private async Task<IReadOnlyList<ArticleCommentDto>> StampViewerLikesAsync(
        IReadOnlyList<ArticleCommentEntity> comments,
        IReadOnlyList<ArticleCommentDto> dtoList,
        Guid? viewerUserId,
        CancellationToken cancellationToken
    )
    {
        if (viewerUserId is not Guid viewerId || dtoList.Count == 0)
        {
            return dtoList;
        }

        Guid[] commentIds = comments.Select(c => c.Id).ToArray();

        IReadOnlySet<Guid> likedIds = await articleRepository.GetLikedCommentIdsAsync(
            viewerUserId: viewerId,
            commentIds: commentIds,
            cancellationToken: cancellationToken
        );

        return dtoList.Select(dto => dto with { IsLiked = likedIds.Contains(dto.Id) }).ToList();
    }
}
