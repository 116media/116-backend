# Spec 02 — Phase 2: Feed Flags (Batch)

`IsLiked` / `IsBookmarked` on `ArticleSummaryDto` across the published feed, promoted list,
and promotion feed, populated with **one batch query per interaction type** (no N+1). Adds two
repository methods and a batch mapper overload. **No EF migration.**

---

## 2.1 `ArticleSummaryDto` — add two flags

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleSummaryDto.cs`

```csharp
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article. False for anonymous
/// requests and for authenticated users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article. False for anonymous
/// requests and for authenticated users who have not bookmarked it.
/// </param>
public record ArticleSummaryDto(
    Guid Id,
    Guid CategoryId,
    string CategoryName,
    string Title,
    string Slug,
    string Headline,
    string? CoverImageUrl,
    string AuthorId,
    EnumContentStatus Status,
    bool IsPromoted,
    DateTimeOffset? PublishedAt,
    int LikeCount,
    int CommentCount,
    int ShareCount,
    int BookmarkCount,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
```

---

## 2.2 `IArticleRepository` — two batch methods

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`

```csharp
/// <summary>
/// Returns the subset of the given article ids that the specified user has liked. Executes as
/// a single query; ids the user has not liked are absent from the result.
/// </summary>
/// <param name="userId">The authenticated user's id.</param>
/// <param name="articleIds">The candidate article ids, typically one page of a feed.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The distinct set of input ids that the user has liked.</returns>
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
/// <param name="articleIds">The candidate article ids, typically one page of a feed.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The distinct set of input ids that the user has bookmarked.</returns>
Task<HashSet<Guid>> GetBookmarkedArticleIdsAsync(
    Guid userId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken cancellationToken = default
);
```

---

## 2.3 `ArticleRepository` — implement both

**File:** `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`

```csharp
/// <inheritdoc />
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

/// <inheritdoc />
public async Task<HashSet<Guid>> GetBookmarkedArticleIdsAsync(
    Guid userId,
    IReadOnlyCollection<Guid> articleIds,
    CancellationToken cancellationToken = default
)
{
    if (articleIds.Count == 0)
    {
        return [];
    }

    List<Guid> bookmarked = await context
        .ArticleBookmarks.Where(b => b.UserId == userId && articleIds.Contains(b.ArticleId))
        .Select(b => b.ArticleId)
        .ToListAsync(cancellationToken);

    return bookmarked.ToHashSet();
}
```

Both emit a single `WHERE user_id = @u AND article_id = ANY(@ids)` served by the unique
`(user_id, article_id)` index.

---

## 2.4 `ArticleMapper` — batch overload that stamps flags

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

Keep the existing user-agnostic `ToArticleSummaryDtoAsync` / `ToArticleSummaryDtosAsync`
untouched (cached and admin call sites use them). Add an overload that takes the two id sets.

```csharp
/// <summary>
/// Maps a list of articles to summaries, stamping each with the current user's interaction
/// flags from the supplied liked/bookmarked id sets. Pass empty sets for an anonymous request.
/// </summary>
/// <param name="entities">The articles to map.</param>
/// <param name="mapper">The Mapster mapper.</param>
/// <param name="fileRepository">Repository used to resolve cover image URLs.</param>
/// <param name="likedArticleIds">Ids the current user has liked.</param>
/// <param name="bookmarkedArticleIds">Ids the current user has bookmarked.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The mapped summaries with interaction flags applied.</returns>
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
        results.Add(
            dto with
            {
                IsLiked = likedArticleIds.Contains(entity.Id),
                IsBookmarked = bookmarkedArticleIds.Contains(entity.Id),
            }
        );
    }

    return results;
}
```

---

## 2.5 Shared resolution helper

Add to each list handler (private method) — or a small internal helper injected into them:

```csharp
/// <summary>
/// Resolves the current user's liked and bookmarked id sets for the given articles. Returns
/// empty sets for an anonymous caller or an empty id list, running no queries in that case.
/// </summary>
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

---

## 2.6 `PublicGetPublishedArticlesHandler`

**File:** `.../GetPublishedArticles/PublicGetPublishedArticlesHandler.cs`

```csharp
(List<ArticleEntity> articles, int totalCount) = await articleRepository.GetAllAsync(
    page: pageIndex + 1,
    pageSize: pageSize,
    search: query.Search,
    status: EnumContentStatus.Published,
    categoryId: query.CategoryId,
    cancellationToken: cancellationToken
);

var articleIds = articles.Select(a => a.Id).ToList();
(IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
    await ResolveInteractionSetsAsync(query.CurrentUserId, articleIds, cancellationToken);

IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
    mapper,
    fileRepository,
    liked,
    bookmarked,
    cancellationToken
);

var paginatedResult = new PaginatedResult<ArticleSummaryDto>(
    pageIndex: pageIndex,
    pageSize: pageSize,
    count: totalCount,
    items: dtoList
);

return new PublicGetPublishedArticlesResult(Articles: paginatedResult);
```

Add `Guid? CurrentUserId = null` to `PublicGetPublishedArticlesQuery` (last param, XML docs).

## 2.7 `PublicGetPromotedArticlesHandler`

**File:** `.../GetPromotedArticles/PublicGetPromotedArticlesHandler.cs`

```csharp
IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPromotedAsync(cancellationToken);

var articleIds = articles.Select(a => a.Id).ToList();
(IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
    await ResolveInteractionSetsAsync(query.CurrentUserId, articleIds, cancellationToken);

IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
    mapper, fileRepository, liked, bookmarked, cancellationToken);

return new PublicGetPromotedArticlesResult(Articles: dtoList);
```

Add `Guid? CurrentUserId = null` to `PublicGetPromotedArticlesQuery`.

## 2.8 `PublicGetArticlePromotionFeedHandler`

**File:** `.../GetArticlePromotionFeed/PublicGetArticlePromotionFeedHandler.cs`

Collect ids from **all** sub-collections (spots + gossip strip) into **one** list; run the
resolver **once**; stamp each collection from the shared sets. Do **not** call the resolver per
sub-collection.

```csharp
// After fetching promoted/gossip entities:
var allIds = promoted.Select(a => a.Id)
    .Concat(gossip.Select(a => a.Id))
    // ... any other sub-collections ...
    .Distinct()
    .ToList();

(IReadOnlySet<Guid> liked, IReadOnlySet<Guid> bookmarked) =
    await ResolveInteractionSetsAsync(query.CurrentUserId, allIds, cancellationToken);

// Stamp each collection with the same shared sets:
IReadOnlyList<ArticleSummaryDto> promotedDtos =
    await promoted.ToArticleSummaryDtosAsync(mapper, fileRepository, liked, bookmarked, cancellationToken);
// ... build spot DTOs / gossip strip from the stamped summaries ...
```

Add `Guid? CurrentUserId = null` to `PublicGetArticlePromotionFeedQuery`.

> If a single-item stamped mapper is needed for the gossip strip, add a matching overload of
> `ToArticleSummaryDtoAsync(entity, mapper, fileRepository, liked, bookmarked, ct)` that
> applies the `with` stamp to one entity, reusing the same sets.

---

## 2.9 Endpoints — resolve the optional user

For each feed endpoint (`PublicGetPublishedArticlesEndpointV1`,
`PublicGetPromotedArticlesEndpointV1`, `PublicGetArticlePromotionFeedEndpointV1`), add
`ClaimsPrincipal user` + `IClaimsProvider claimsProvider` to the route delegate, resolve
`Guid? userId`, and pass it into the query — identical to spec 01 §1.5. All remain
`.AllowAnonymous()`. Add usings:

```csharp
using System.Security.Claims;
using _116.Identity.Contracts.Application;
```

---

## Tasks

- [ ] Add `bool IsLiked = false`, `bool IsBookmarked = false` to `ArticleSummaryDto` with XML docs.
- [ ] Add `GetLikedArticleIdsAsync` / `GetBookmarkedArticleIdsAsync` to `IArticleRepository` with XML docs.
- [ ] Implement both in `ArticleRepository` with the empty-input guard and `ToHashSet()`.
- [ ] Add the flag-stamping batch overload of `ToArticleSummaryDtosAsync` (keep the existing user-agnostic overload).
- [ ] (Optional) Add a single-item stamped `ToArticleSummaryDtoAsync` overload for the gossip strip.
- [ ] Add the `ResolveInteractionSetsAsync` helper to the list handlers (or a shared internal helper).
- [ ] Add `Guid? CurrentUserId = null` to the published / promoted / promotion-feed query records.
- [ ] Wire `PublicGetPublishedArticlesHandler` to resolve sets and use the batch mapper.
- [ ] Wire `PublicGetPromotedArticlesHandler` likewise.
- [ ] Wire `PublicGetArticlePromotionFeedHandler` with **one** resolver call over all sub-collection ids.
- [ ] Add `ClaimsPrincipal` + `IClaimsProvider` resolution to the three feed endpoints; add usings.
- [ ] Do **not** add caching in this spec; if caching is later added, follow Strategy 1 in `07-caching-and-rollout.md`.
- [ ] `dotnet csharpier .` on the touched files (user runs).
