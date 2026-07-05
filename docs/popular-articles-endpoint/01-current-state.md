# Current State — Slices, Signals, and the Caching Pattern

Everything the new popular-articles slice is built from already exists in the codebase. This
document maps the exact files, so the new slice can mirror them precisely.

---

## 1. The Published-Articles Slice (the shape to reuse for a public list)

The public published-articles slice shows how a public, anonymous, rate-limited, paginated
article-list endpoint is wired end-to-end.

### Query + Result

`src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetPublishedArticles/PublicGetPublishedArticlesQuery.cs`

```csharp
public record PublicGetPublishedArticlesQuery(
    PaginatedRequest PaginatedRequest,
    string? Search,
    Guid? CategoryId,
    string? TagSlug
) : IQuery<PublicGetPublishedArticlesResult>;

public record PublicGetPublishedArticlesResult(PaginatedResult<ArticleSummaryDto> Articles);
```

### Handler

`src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetPublishedArticles/PublicGetPublishedArticlesHandler.cs`

Injects `IArticleRepository`, `IFileRepository`, `IMapper`. Calls
`articleRepository.GetAllAsync(page, pageSize, search, status: EnumContentStatus.Published,
categoryId, ct)`, maps entities via the `ToArticleSummaryDtosAsync` extension, and wraps them
in a `PaginatedResult`.

### Repository method

`src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`

```csharp
Task<(List<ArticleEntity> Articles, int TotalCount)> GetAllAsync(
    int page,
    int pageSize,
    string? search,
    EnumContentStatus? status,
    Guid? categoryId,
    CancellationToken cancellationToken = default
);
```

Implementation: `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`

```csharp
IQueryable<ArticleEntity> query = context.Articles.Include(a => a.Category);

Specification<ArticleEntity>? spec = new ArticleQueryBuilder()
    .WithSearch(search)
    .WithStatus(status)
    .WithCategory(categoryId)
    .Build();

if (spec is not null)
{
    query = query.ApplySpecification(spec);
}

int totalCount = await query.CountAsync(cancellationToken);

List<ArticleEntity> articles = await query
    .OrderByDescending(a => a.CreatedAt)   // <-- recency, NOT popularity
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync(cancellationToken);

return (articles, totalCount);
```

**Key observation:** the published list orders by `CreatedAt` descending. That is the recency
ordering the popular endpoint replaces with a weighted-score ordering.

### Query builder + specifications (the pattern to extend)

- Builder: `src/Modules/Content/Content/Application/Editorial/Builders/ArticleQueryBuilder.cs`
  — fluent `WithSearch / WithStatus / WithCategory / Build`, combining `Specification<ArticleEntity>`
  via `.And()`.
- Specs: `src/Modules/Content/Content/Application/Editorial/Specifications/ArticleSpecifications.cs`
  — `ArticleByStatusSpecification`, `ArticleByCategorySpecification`, `ArticleSearchSpecification`.

The status/category specs are directly reusable for the popular query. The ordering is *not*
expressible as a `Specification` (a spec is a `bool` predicate), so the score ordering is
implemented in the new query builder (see `03-query-and-scoring.md`).

### DTO

`src/Modules/Content/Content/Application/Shared/DTOs/ArticleSummaryDto.cs`

```csharp
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
    int BookmarkCount
) : AuditableDto;
```

The DTO **already carries all four engagement counters and `PublishedAt`** — the popular
endpoint reuses it unchanged (the frontend sidebar already renders `ArticleSummaryDto`).

### Mapper extension

`src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

```csharp
public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
    this IReadOnlyList<ArticleEntity> entities,
    IMapper mapper,
    IFileRepository fileRepository,
    CancellationToken ct = default
)
```

Resolves `CoverImageUrl` from the associated `FileEntity`. The popular handler reuses this
exact extension.

### Endpoint

`src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetPublishedArticles/V1/PublicGetPublishedArticlesEndpointV1.cs`

```csharp
RouteGroupBuilder group = app.MapApiVersionGroup(1)
    .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}")
    .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

group.MapGet("/", async (IDispatcher dispatcher, int pageIndex = 0, int pageSize = 10, ...) => { ... })
    .WithName(...)
    .AllowAnonymous()
    .RequireRateLimiting(RateLimitPolicies.ContentBrowsing)
    .Produces<PublicGetPublishedArticlesResponse>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status429TooManyRequests);
```

`GET /api/v1/public/articles/` — `AllowAnonymous`, `ContentBrowsing` rate limiting. The
popular endpoint sits in the **same route group** (`articles`) at the `popular` sub-path.

---

## 2. Popularity Signals Available on the Article

`src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs`

Four persisted, non-negative integer counters:

```csharp
public int LikeCount { get; private set; }
public int CommentCount { get; private set; }
public int ShareCount { get; private set; }
public int BookmarkCount { get; private set; }
```

Persistence config (`ArticleConfiguration.cs`) — all four are `int`, non-null, default 0:

```csharp
builder.Property(x => x.LikeCount).HasDefaultValue(0).IsRequired();
builder.Property(x => x.CommentCount).HasDefaultValue(0).IsRequired();
builder.Property(x => x.ShareCount).HasDefaultValue(0).IsRequired();
builder.Property(x => x.BookmarkCount).HasDefaultValue(0).IsRequired();
```

Publishing / ranking fields on the same entity:

```csharp
public EnumContentStatus Status { get; private set; }
public DateTimeOffset? PublishedAt { get; private set; }   // null until Publish()
public Guid CategoryId { get; private set; }
public string Title { get; private set; }
public string Slug { get; private set; }
```

### Counter mutations (these move the ranking → they bust the cache)

The counters are maintained by domain methods, called from the interactions slice
(`Application/Interactions/UseCases/Public/Commands/...`):

| Signal | Handler | Entity method |
|--------|---------|---------------|
| Like +1 | `LikeArticle/PublicLikeArticleHandler.cs` | `IncrementLikeCount()` |
| Like −1 | `UnlikeArticle/PublicUnlikeArticleHandler.cs` | `DecrementLikeCount()` |
| Comment +1 | `AddArticleComment/PublicAddArticleCommentHandler.cs` | `IncrementCommentCount()` |
| Comment −1 | `DeleteArticleComment/PublicDeleteArticleCommentHandler.cs` | `DecrementCommentCount()` |
| Share +1 | `ShareArticle/PublicShareArticleHandler.cs` | `IncrementShareCount()` |
| Bookmark +1 | `BookmarkArticle/PublicBookmarkArticleHandler.cs` | `IncrementBookmarkCount()` |
| Bookmark −1 | `UnbookmarkArticle/PublicUnbookmarkArticleHandler.cs` | `DecrementBookmarkCount()` |

Decrements clamp at 0 (`Math.Max(0, x - 1)`). There is no share decrement.

### Status enum

`src/Modules/Content/Content/Domain/Enums/EnumContentStatus.cs` — values: `Draft`,
`PendingPayment`, `PendingReview`, `Approved`, `Published`, `Rejected`, `Archived`. Only
`Published` articles are eligible for the popular endpoint.

---

## 3. Promotion Feed — how exclusion is done today

`src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetArticlePromotionFeed/PublicGetArticlePromotionFeedHandler.cs`

The homepage promotion feed dedups internally using a `HashSet<Guid> usedIds` and passes it
as `excludeIds` to `articleRepository.GetGossipFallbackAsync(...)`:

```csharp
Task<IReadOnlyList<ArticleEntity>> GetGossipFallbackAsync(
    Guid gossipCategoryId,
    int limit,
    IEnumerable<Guid> excludeIds,
    CancellationToken cancellationToken = default
);
```

So the repository layer already has a precedent for excluding article ids via a filter.
There is **no `excludeId` query parameter** exposed on any endpoint today — the popular
endpoint introduces the first one, translated to a `WHERE a.Id != excludeId` in the builder.
`GetPromotedArticles` (`.../GetPromotedArticles/...`) has no exclusion at all.

---

## 4. The GetPopularTags Slice — the CANONICAL caching pattern to mirror

This is the slice the new endpoint must mirror for its **endpoint shape + caching +
invalidation**. It lives under
`Application/Lookup/UseCases/Public/Queries/GetPopularTags/`.

### Endpoint

`.../GetPopularTags/V1/PublicGetPopularTagsEndpointV1.cs` —
`GET /api/v1/public/tags/popular`, `AllowAnonymous`,
`RequireRateLimiting(RateLimitPolicies.ContentBrowsing)`, reads `int? limit` from the query
string, dispatches `PublicGetPopularTagsQuery`.

### Handler with IMemoryCache + eviction token

`.../GetPopularTags/PublicGetPopularTagsHandler.cs`

```csharp
public class PublicGetPopularTagsHandler(
    ILookupRepository lookupRepository,
    IMemoryCache cache,
    IPopularTagsCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularTagsQuery, PublicGetPopularTagsResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<PublicGetPopularTagsResult> Handle(
        PublicGetPopularTagsQuery query,
        CancellationToken cancellationToken
    )
    {
        string limitPart = query.Limit?.ToString() ?? "all";
        string contentTypePart = query.ContentType?.ToString().ToLowerInvariant() ?? "null";
        string cacheKey = $"popular_tags_{limitPart}_{contentTypePart}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<TagDto>? cached) && cached is not null)
        {
            return new PublicGetPopularTagsResult(Tags: cached);
        }

        IReadOnlyList<TagEntity> tags = await lookupRepository.GetPopularTagsAsync(
            limit: query.Limit,
            contentType: query.ContentType,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<TagDto> dtoList = tags.ToTagDtos(mapper);

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularTagsResult(Tags: dtoList);
    }
}
```

Each `(limit, contentType)` combination is its own cache entry; **all** entries share one
eviction token, so any mutation evicts them all at once.

### The invalidator

Interface: `src/Modules/Content/Content/Application/Shared/Cache/IPopularTagsCacheInvalidator.cs`

```csharp
public interface IPopularTagsCacheInvalidator
{
    CancellationToken GetEvictionToken();
    void Invalidate();
}
```

Implementation: `src/Modules/Content/Content/Infrastructure/Cache/PopularTagsCacheInvalidator.cs`

```csharp
public sealed class PopularTagsCacheInvalidator : IPopularTagsCacheInvalidator
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _cts = new();

    public CancellationToken GetEvictionToken()
    {
        lock (_lock) { return _cts.Token; }
    }

    public void Invalidate()
    {
        CancellationTokenSource old;
        lock (_lock)
        {
            old = _cts;
            _cts = new CancellationTokenSource();
        }
        old.Cancel();
        old.Dispose();
    }
}
```

Registered as a singleton in `ContentModule.cs`:

```csharp
services.AddSingleton<IPopularTagsCacheInvalidator, PopularTagsCacheInvalidator>();
```

### Where the tags invalidator is called

After `CommitAsync`, in the handlers that change the tag graph:

- `AdminCreateTagHandler`, `AdminUpdateTagHandler`, `AdminDeleteTagHandler`
- `AdminUpdateArticleTagsHandler`, `AdminUpdateVideoTagsHandler`

Pattern:

```csharp
await unitOfWork.CommitAsync(cancellationToken);
cacheInvalidator.Invalidate();
```

### The tags query builder

`src/Modules/Content/Content/Application/Lookup/Builders/PopularTagsQueryBuilder.cs` — fluent
`WithContentType / WithLimit / Build(context)`, computes usage counts, orders
`OrderByDescending(count).ThenBy(name)`, applies `.Take(limit)`. Called from
`LookupRepository.GetPopularTagsAsync`. The new `PopularArticlesQueryBuilder` mirrors this
exactly (see `03-query-and-scoring.md`).
