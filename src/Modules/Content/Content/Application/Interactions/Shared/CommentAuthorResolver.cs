using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Identity.Contracts.Application;

namespace _116.Content.Application.Interactions.Shared;

/// <summary>
/// Shared helper that batch-resolves the public author profile for a page of comments,
/// reusing the same cross-module mechanism used for article authors. Deleted comments are
/// excluded so no identity is leaked, and the commenter email is never exposed publicly.
/// </summary>
public static class CommentAuthorResolver
{
    /// <summary>
    /// Resolves the author profile for every distinct non-deleted commenter in the given set,
    /// keyed by commenter user id. Author user names, avatar URLs, and roles are resolved in a
    /// single identity lookup plus a single avatar-URL lookup. The email is intentionally
    /// dropped: it is never exposed on the public endpoints.
    /// </summary>
    /// <param name="userLookup">Cross-module service for resolving commenter profiles.</param>
    /// <param name="fileRepository">Repository for resolving avatar file URLs.</param>
    /// <param name="comments">The page of comment entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved author profiles keyed by commenter user id.</returns>
    public static async Task<IReadOnlyDictionary<Guid, AuthorDto>> ResolveAsync(
        IUserLookupService userLookup,
        IFileRepository fileRepository,
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
}
