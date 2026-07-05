# Spec 01 — Phase 1: DTO, Batch Resolver, Handler

Implementation-ready. Every change to ship the comment author projection.

---

## 1.1 `IUserLookupService` — add batch method

**File:** `src/Modules/Identity/Identity.Contracts/Application/IUserLookupService.cs`

**Add** (alongside the existing methods; do not remove them):

```csharp
/// <summary>
/// Resolves author profiles for a set of user IDs in a single query.
/// </summary>
/// <param name="userIds">
/// The identity user UUIDs to look up. Duplicates and unknown IDs are ignored.
/// </param>
/// <param name="ct">
/// Cancellation token.
/// </param>
/// <returns>
/// A dictionary keyed by user ID containing the resolved author info.
/// IDs that do not match a user are absent from the result.
/// </returns>
Task<IReadOnlyDictionary<Guid, AuthorInfo>> GetAuthorInfosByIdsAsync(
    IReadOnlyCollection<Guid> userIds,
    CancellationToken ct = default
);
```

---

## 1.2 `UserLookupService` — implement batch method

**File:** `src/Modules/Identity/Identity/Infrastructure/Services/UserLookupService.cs`

**Add:**

```csharp
/// <inheritdoc />
public async Task<IReadOnlyDictionary<Guid, AuthorInfo>> GetAuthorInfosByIdsAsync(
    IReadOnlyCollection<Guid> userIds,
    CancellationToken ct = default
)
{
    if (userIds.Count == 0)
    {
        return new Dictionary<Guid, AuthorInfo>();
    }

    Guid[] distinctIds = userIds.Distinct().ToArray();

    List<UserEntity> users = await context
        .Users.Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
        .Where(u => distinctIds.Contains(u.Id))
        .ToListAsync(ct);

    return users.ToDictionary(
        user => user.Id,
        user => new AuthorInfo(
            user.UserName,
            user.Email,
            user.AvatarFileId,
            user.UserRoles.Select(ur => ur.Role.Name).FirstOrDefault()
        )
    );
}
```

> Add `using _116.Identity.Domain.Entities;` if `UserEntity` is not already in scope. The
> projection matches `GetAuthorInfoByIdAsync` exactly.

---

## 1.3 `IFileRepository` — add batch avatar resolution

**File:** `src/Modules/Core/Core/Application/Shared/Repositories/IFileRepository.cs`

**Add:**

```csharp
/// <summary>
/// Returns the storage URLs for the given file IDs, keyed by file ID.
/// Missing or soft-deleted files are absent from the result.
/// </summary>
/// <param name="fileIds">
/// The file UUIDs to resolve. Duplicates and unknown IDs are ignored.
/// </param>
/// <param name="cancellationToken">
/// Cancellation token.
/// </param>
/// <returns>
/// A dictionary keyed by file ID containing each file's storage URL.
/// </returns>
Task<IReadOnlyDictionary<Guid, string>> GetStorageUrlsByIdsAsync(
    IReadOnlyCollection<Guid> fileIds,
    CancellationToken cancellationToken = default
);
```

**Impl** — `src/Modules/Core/Core/Infrastructure/Repositories/FileRepository.cs`:

```csharp
/// <inheritdoc />
public async Task<IReadOnlyDictionary<Guid, string>> GetStorageUrlsByIdsAsync(
    IReadOnlyCollection<Guid> fileIds,
    CancellationToken cancellationToken = default
)
{
    if (fileIds.Count == 0)
    {
        return new Dictionary<Guid, string>();
    }

    Guid[] distinctIds = fileIds.Distinct().ToArray();

    return await context
        .Files.Where(f => distinctIds.Contains(f.Id))
        .ToDictionaryAsync(f => f.Id, f => f.StorageUrl, cancellationToken);
}
```

> Match the existing `FileRepository` field name for the context (e.g. `context`). If a
> soft-delete filter is applied elsewhere in the repository, apply it here too.
>
> **Fallback if this method is deferred:** the handler (§1.6) may instead loop the existing
> `GetByIdAsync` over the de-duplicated avatar file IDs. The batch method is preferred.

---

## 1.4 `ArticleCommentDto` — add `Author`

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleCommentDto.cs`

**Replace with:**

```csharp
using _116.Shared.Application.DTOs;

namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for an article comment.
/// When the comment is deleted, the Body is null, IsDeleted is true, and Author is null.
/// </summary>
/// <param name="Id">
/// The unique identifier of the comment.
/// </param>
/// <param name="UserId">
/// The identity user UUID of the commenter.
/// </param>
/// <param name="Body">
/// The comment text. Null if the comment has been deleted.
/// </param>
/// <param name="IsDeleted">
/// Whether this comment has been soft-deleted.
/// </param>
/// <param name="Author">
/// The resolved commenter profile with avatar URL, or null when the comment is deleted
/// or the commenter could not be resolved. The email is never populated on the public
/// endpoint.
/// </param>
public record ArticleCommentDto(
    Guid Id,
    Guid UserId,
    string? Body,
    bool IsDeleted,
    AuthorDto? Author = null
) : AuditableDto;
```

---

## 1.5 `ArticleMapper` — add author-aware list mapper

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

Keep the existing `ToArticleCommentDto(entity, mapper)` and
`ToArticleCommentDtos(entities, mapper)` (used by add/edit handlers). **Add** an overload
that attaches resolved authors:

```csharp
/// <summary>
/// Maps a list of <see cref="ArticleCommentEntity" /> to a list of
/// <see cref="ArticleCommentDto" />, attaching each commenter's resolved author profile.
/// Deleted comments carry a null body and a null author; commenters absent from
/// <paramref name="authorsByUserId" /> also carry a null author.
/// </summary>
/// <param name="entities">
/// The comment entities to map.
/// </param>
/// <param name="mapper">
/// The Mapster mapper.
/// </param>
/// <param name="authorsByUserId">
/// The resolved author profiles keyed by commenter user ID.
/// </param>
/// <returns>
/// The mapped comment DTOs with authors attached.
/// </returns>
public static IReadOnlyList<ArticleCommentDto> ToArticleCommentDtos(
    this IReadOnlyList<ArticleCommentEntity> entities,
    IMapper mapper,
    IReadOnlyDictionary<Guid, AuthorDto> authorsByUserId
)
{
    return entities
        .Select(entity =>
        {
            ArticleCommentDto dto = entity.ToArticleCommentDto(mapper);

            if (entity.IsDeleted)
            {
                return dto;
            }

            AuthorDto? author = authorsByUserId.GetValueOrDefault(entity.UserId);
            return dto with { Author = author };
        })
        .ToList();
}
```

---

## 1.6 `PublicGetArticleCommentsHandler` — resolve authors (batched)

**File:** `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Queries/GetArticleComments/PublicGetArticleCommentsHandler.cs`

**Replace with:**

```csharp
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

        IReadOnlyDictionary<Guid, AuthorDto> authorsByUserId = await ResolveAuthorsAsync(
            comments,
            cancellationToken
        );

        IReadOnlyList<ArticleCommentDto> dtoList = comments
            .AsReadOnly()
            .ToArticleCommentDtos(mapper, authorsByUserId);

        var paginated = new PaginatedResult<ArticleCommentDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtoList
        );

        return new PublicGetArticleCommentsResult(Comments: paginated);
    }

    /// <summary>
    /// Batch-resolves the author profile for every distinct non-deleted commenter on the
    /// page. Deleted comments are excluded so no identity is leaked. The commenter's email
    /// is intentionally dropped: it is never exposed on the public endpoint.
    /// </summary>
    /// <param name="comments">The page of comment entities.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Resolved author profiles keyed by commenter user ID.</returns>
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

                return new AuthorDto(
                    UserName: info.UserName,
                    Email: null,
                    AvatarUrl: avatarUrl,
                    Role: info.Role
                );
            }
        );
    }
}
```

Key points enforced above:
- **Batch** — one `GetAuthorInfosByIdsAsync`, one `GetStorageUrlsByIdsAsync` per page.
- **Deleted-safe** — deleted comments excluded from the lookup; the mapper also nulls their
  author.
- **Email-stripped** — `Email: null` on the public projection.

---

## Tasks

- [ ] Add `GetAuthorInfosByIdsAsync` to `IUserLookupService`.
- [ ] Implement `GetAuthorInfosByIdsAsync` in `UserLookupService`.
- [ ] Add `GetStorageUrlsByIdsAsync` to `IFileRepository` + `FileRepository` (or use the de-duplicated loop fallback).
- [ ] Add `Author` to `ArticleCommentDto`.
- [ ] Add the author-aware `ToArticleCommentDtos` overload to `ArticleMapper`.
- [ ] Rewrite `PublicGetArticleCommentsHandler` to inject the resolvers and attach authors.
- [ ] Confirm DI already provides `IUserLookupService` (IdentityModule line 140) and `IFileRepository` to the Content read handlers.
