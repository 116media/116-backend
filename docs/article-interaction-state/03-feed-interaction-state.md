# Phase 2 — Feed Interaction State

Add the same `IsLiked` / `IsBookmarked` flags to `ArticleSummaryDto` and populate them across
every list surface — the published feed, the promoted list, and the promotion feed — using a
**batch** lookup so a page of N articles costs **two** queries total, not `2 × N`.

Full C# with `## Tasks` checklist is in
[specs/02-feed-flags-batch.md](specs/02-feed-flags-batch.md). This document is the design
narrative. The caching correctness concern is expanded in
[07-caching-and-rollout.md](07-caching-and-rollout.md).

---

## 1. DTO — add two flags to `ArticleSummaryDto`

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleSummaryDto.cs`

Same shape as Phase 1 — two trailing optional booleans defaulting to `false`:

```csharp
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article.
/// False for anonymous requests and for users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article.
/// False for anonymous requests and for users who have not bookmarked it.
/// </param>
public record ArticleSummaryDto(
    // ... existing 15 parameters unchanged ...
    int BookmarkCount,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
```

---

## 2. Repository — two batch lookups (new)

**Interface** — `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`

```csharp
/// <summary>
/// Returns the subset of the given article ids that the specified user has liked.
/// Executes as a single query; ids the user has not liked are absent from the result.
/// </summary>
/// <param name="userId">The authenticated user's id.</param>
/// <param name="articleIds">The candidate article ids (typically one page of a feed).</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The distinct set of article ids from the input that the user has liked.</returns>
Task<HashSet<Guid>> GetLikedArticleIdsAsync(
    Guid userId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken cancellationToken = default
);

/// <summary>
/// Returns the subset of the given article ids that the specified user has bookmarked.
/// Executes as a single query; ids the user has not bookmarked are absent from the result.
/// </summary>
/// <param name="userId">The authenticated user's id.</param>
/// <param name="articleIds">The candidate article ids (typically one page of a feed).</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The distinct set of article ids from the input that the user has bookmarked.</returns>
Task<HashSet<Guid>> GetBookmarkedArticleIdsAsync(
    Guid userId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken cancellationToken = default
);
```

**Implementation** — `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`

```csharp
public async Task<HashSet<Guid>> GetLikedArticleIdsAsync(
    Guid userId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken cancellationToken = default
)
{
    if (articleIds.Count == 0)
    {
        return [];
    }

    List<Guid> liked = await context
        .ArticleLikes.Where(l => l.UserId == userId && articleIds.Contains(l.ArticleId))
        .Select(l => l.ArticleId)
        .ToListAsync(cancellationToken);

    return liked.ToHashSet();
}
```

`GetBookmarkedArticleIdsAsync` is identical against `context.ArticleBookmarks`. Both emit a
single `WHERE user_id = @u AND article_id = ANY(@ids)` served by the unique
`(user_id, article_id)` index. Returning a `HashSet<Guid>` makes the per-item flag a
constant-time `Contains` in the mapper.

> The empty-input guard matters: an empty `articleIds` (e.g. an empty feed page) must short
> circuit to an empty set, never build a `Contains` over nothing.

---

## 3. Mapper — a batch overload that stamps the flags

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

Add a batch mapper overload that takes the two liked/bookmarked id sets and stamps each DTO
as it is built. The existing `ToArticleSummaryDtoAsync` / `ToArticleSummaryDtosAsync` stay as
the user-agnostic (all-`false`) path used by cached and admin call sites.

```csharp
/// <summary>
/// Maps a list of articles to summaries, stamping per-user interaction flags from the
/// supplied liked/bookmarked id sets. Pass empty sets for an anonymous request.
/// </summary>
public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
    this IReadOnlyList<ArticleEntity> entities,
    IMapper mapper,
    IFileRepository fileRepository,
    IReadOnlySet<Guid> likedArticleIds,
    IReadOnlySet<Guid> bookmarkedArticleIds,
    CancellationToken ct = default
)
{
    var results = new List<ArticleSummaryDto>(entities.Count);
    foreach (ArticleEntity entity in entities)
    {
        ArticleSummaryDto dto = await entity.ToArticleSummaryDtoAsync(mapper, fileRepository, ct);
        results.Add(dto with
        {
            IsLiked = likedArticleIds.Contains(entity.Id),
            IsBookmarked = bookmarkedArticleIds.Contains(entity.Id),
        });
    }
    return results;
}
```

Using `record with` keeps the single-item mapper untouched and layers the flags on top —
which is exactly the shape needed for the "layer flags after cache read" strategy in doc 07.

---

## 4. A shared helper to resolve both id sets

Each list handler needs the same three lines: collect ids, batch-query likes, batch-query
bookmarks. Extract it once so the four handlers stay tidy and consistent. A private static
helper (or a small internal service) on the handler side:

```csharp
private async Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> ResolveInteractionSetsAsync(
    Guid? currentUserId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken ct
)
{
    if (currentUserId is not Guid userId || articleIds.Count == 0)
    {
        return (new HashSet<Guid>(), new HashSet<Guid>());
    }

    HashSet<Guid> liked = await articleRepository.GetLikedArticleIdsAsync(userId, articleIds, ct);
    HashSet<Guid> bookmarked = await articleRepository.GetBookmarkedArticleIdsAsync(userId, articleIds, ct);
    return (liked, bookmarked);
}
```

Anonymous → both empty sets → every flag `false` → no extra queries.

---

## 5. Wire the four list handlers

All under `src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/`. Each
query gains an optional `Guid? CurrentUserId = null`, and each endpoint resolves it with the
optional-auth pattern (identical to Phase 1 §5).

### 5.1 `PublicGetPublishedArticlesHandler`

```csharp
(List<ArticleEntity> articles, int totalCount) = await articleRepository.GetAllAsync(/* ... */);

var articleIds = articles.Select(a => a.Id).ToList();
(IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
    await ResolveInteractionSetsAsync(query.CurrentUserId, articleIds, cancellationToken);

IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
    mapper, fileRepository, liked, bookmarked, cancellationToken);

var paginatedResult = new PaginatedResult<ArticleSummaryDto>(pageIndex, pageSize, totalCount, dtoList);
return new PublicGetPublishedArticlesResult(Articles: paginatedResult);
```

### 5.2 `PublicGetPromotedArticlesHandler`

Same three steps around the `GetPromotedAsync` result.

### 5.3 `PublicGetArticlePromotionFeedHandler`

This handler builds several summary collections (3 promotion spots + a gossip strip). Collect
the ids from **all** of them into one list, run **one** liked + **one** bookmarked batch
query, and stamp each collection from the shared sets. Do not run a batch per sub-collection.

---

## 6. Endpoints — resolve the optional user

Each of the four feed endpoints gains `ClaimsPrincipal user` + `IClaimsProvider claimsProvider`
and passes `Guid? userId` into the query, exactly as in Phase 1 §5. All remain
`.AllowAnonymous()`.

| Endpoint | Route |
|----------|-------|
| `PublicGetPublishedArticlesEndpointV1` | `GET /api/v1/public/articles/` |
| `PublicGetPromotedArticlesEndpointV1` | `GET /api/v1/public/articles/promoted` |
| `PublicGetArticlePromotionFeedEndpointV1` | `GET /api/v1/public/articles/promotion/feed` |

---

## 7. Caching implication (read this before caching any of these lists)

None of these list handlers is cached **today**, so shipping Phase 2 as described introduces
no cache bug. But the promoted / promotion-feed lists are obvious cache candidates, and the
flags are **per-user** — a cached response carrying one user's `IsLiked = true` served to
another user is a correctness (and privacy) defect.

The rule: **the cached payload must stay user-agnostic.** Two safe strategies:

1. **Cache the user-agnostic list, layer flags after read.** Cache the result of the
   *original* all-`false` `ToArticleSummaryDtosAsync`. On each request, read from cache, then
   run the two batch id lookups and re-stamp with `record with` before returning. The cache
   key contains **no** user id; the flags never enter the cache.
2. **Skip the cache for authenticated requests.** Serve anonymous traffic (all flags `false`)
   from cache; bypass the cache entirely when `CurrentUserId` is set.

Strategy 1 is preferred — it keeps the cache hit rate high while staying correct. Full detail,
including cache-key rules and the anti-pattern to avoid, is in
[07-caching-and-rollout.md](07-caching-and-rollout.md).

---

## Files changed (Phase 2)

| File | Change |
|------|--------|
| `Content/Application/Shared/DTOs/ArticleSummaryDto.cs` | Add `bool IsLiked = false`, `bool IsBookmarked = false` |
| `Content/Application/Shared/Repositories/IArticleRepository.cs` | Add `GetLikedArticleIdsAsync`, `GetBookmarkedArticleIdsAsync` |
| `Content/Infrastructure/Repositories/ArticleRepository.cs` | Implement both batch methods |
| `Content/Application/Shared/Mappers/ArticleMapper.cs` | Add batch `ToArticleSummaryDtosAsync` overload that stamps flags |
| `.../GetPublishedArticles/*` | Add `Guid? CurrentUserId`; resolve sets; stamp DTOs; endpoint resolves user |
| `.../GetPromotedArticles/*` | Same |
| `.../GetArticlePromotionFeed/*` | Same, one batch over all sub-collections |

No EF migration.
