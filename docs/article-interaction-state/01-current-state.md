# Current State

Everything that exists today around article likes, bookmarks, the read DTOs, and the
current-user accessor. All paths are relative to `apps/backend`.

---

## 1. How likes and bookmarks are stored

### Join entities

Both are aggregates with a single row per `(user, article)` pair. A row exists while the
interaction is active; it is **created on like/bookmark and removed on unlike/unbookmark** —
never updated.

**`ArticleLikeEntity`** — `src/Modules/Content/Content/Domain/Entities/ArticleLikeEntity.cs`

| Member | Type | Notes |
|--------|------|-------|
| `Id` | `Guid` | Primary key (from `Aggregate<Guid>`) |
| `UserId` | `Guid` | Identity user UUID; no FK to the identity schema by design |
| `ArticleId` | `Guid` | FK to `ArticleEntity` |
| `Article` | `ArticleEntity` | Navigation (`null!`) |
| `Create(Guid id, Guid userId, Guid articleId)` | factory | Sets `CreatedAt = DateTime.UtcNow` |

**`ArticleBookmarkEntity`** — `src/Modules/Content/Content/Domain/Entities/ArticleBookmarkEntity.cs`

Identical shape: `Id`, `UserId`, `ArticleId`, `Article`, and
`Create(Guid id, Guid userId, Guid articleId)`.

### EF configurations

**`ArticleLikeConfiguration`** — `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleLikeConfiguration.cs`

- Table `article_likes`, schema `content`.
- **Unique index on `(UserId, ArticleId)`** — one like per user per article.
- Index on `ArticleId`.
- FK `ArticleId → ArticleEntity.Id`, `OnDelete(Cascade)`.

**`ArticleBookmarkConfiguration`** — `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleBookmarkConfiguration.cs`

- Table `article_bookmarks`, schema `content`, same unique `(UserId, ArticleId)` index and
  `ArticleId` index, same cascade FK.

The `content.article_likes` / `content.article_bookmarks` tables and their unique indexes
already exist, so the read-path work in this feature needs **no EF migration**.

### DbContext sets

`ContentDbContext` exposes `ArticleLikes` and `ArticleBookmarks` (used verbatim by the
repository below).

---

## 2. How counts are maintained

The counters are **denormalized on `ArticleEntity`**
(`src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs`) and adjusted in the command
handlers, not via DB triggers or COUNT queries:

```csharp
public int LikeCount { get; private set; }       // "Cached like count. Incremented/decremented by interaction events."
public int BookmarkCount { get; private set; }    // "Cached bookmark count. Incremented/decremented by interaction events."

public void IncrementLikeCount() => LikeCount++;
public void DecrementLikeCount() => LikeCount = Math.Max(0, LikeCount - 1);
public void IncrementBookmarkCount() => BookmarkCount++;
public void DecrementBookmarkCount() => BookmarkCount = Math.Max(0, BookmarkCount - 1);
```

These counters are the **global** figures already on the DTOs. They are orthogonal to the
per-user flags this feature adds: a count of `5` says nothing about whether *this* reader is
one of the 5.

---

## 3. The interaction command handlers

All under `src/Modules/Content/Content/Application/Interactions/UseCases/Public/Commands/`.
Each command carries `(Guid ArticleId, Guid UserId)`; each handler verifies the article,
checks the existence guard, mutates the join set, adjusts the counter, and commits.

| Command | Handler | Repo methods used |
|---------|---------|-------------------|
| `LikeArticle/PublicLikeArticleCommand` | `PublicLikeArticleHandler` | `HasLikedAsync`, `AddLikeAsync`, `IncrementLikeCount`, `Update` |
| `UnlikeArticle/PublicUnlikeArticleCommand` | `PublicUnlikeArticleHandler` | `HasLikedAsync`, `RemoveLikeAsync`, `DecrementLikeCount`, `Update` |
| `BookmarkArticle/PublicBookmarkArticleCommand` | `PublicBookmarkArticleHandler` | `HasBookmarkedAsync`, `AddBookmarkAsync`, `IncrementBookmarkCount`, `Update` |
| `UnbookmarkArticle/PublicUnbookmarkArticleCommand` | `PublicUnbookmarkArticleHandler` | `HasBookmarkedAsync`, `RemoveBookmarkAsync`, `DecrementBookmarkCount`, `Update` |

Note that `HasLikedAsync` / `HasBookmarkedAsync` **already exist** — the write path uses them
as its idempotency guard. Phase 1 reuses them directly. Only Phase 2 needs new **batch**
methods.

---

## 4. The article repository

**Interface** — `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`
(`: IRepository<ArticleEntity>`).

Existing like/bookmark methods (verbatim):

```csharp
Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);
Task AddLikeAsync(ArticleLikeEntity like, CancellationToken cancellationToken = default);
Task RemoveLikeAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

Task<bool> HasBookmarkedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);
Task AddBookmarkAsync(ArticleBookmarkEntity bookmark, CancellationToken cancellationToken = default);
Task RemoveBookmarkAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

Task<(List<ArticleEntity> Articles, int TotalCount)> GetBookmarkedArticlesAsync(
    Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
```

**Implementation** — `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`.
`HasLikedAsync` / `HasBookmarkedAsync` apply a spec and call `AnyAsync`:

```csharp
public async Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
{
    var specification = new ArticleLikeByUserAndArticleSpecification(userId: userId, articleId: articleId);
    return await context.ArticleLikes.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
}
```

**Specifications** — `src/Modules/Content/Content/Application/Editorial/Specifications/ArticleSpecifications.cs`:
`ArticleLikeByUserAndArticleSpecification`, `ArticleBookmarkByUserAndArticleSpecification`,
`ArticleBookmarkByUserIdSpecification`.

> **Gap:** there is **no** existing method to answer "which of these N article ids has user X
> liked/bookmarked" in a single query. Phase 2 adds `GetLikedArticleIdsAsync` /
> `GetBookmarkedArticleIdsAsync`.

---

## 5. Where the DTOs are built

### DTO types

- **`ArticleDetailDto`** — `src/Modules/Content/Content/Application/Shared/DTOs/ArticleDetailDto.cs`,
  a `record : AuditableDto`. Carries `LikeCount`, `CommentCount`, `ShareCount`,
  `BookmarkCount` — **no `IsLiked` / `IsBookmarked`**.
- **`ArticleSummaryDto`** — `src/Modules/Content/Content/Application/Shared/DTOs/ArticleSummaryDto.cs`,
  a `record : AuditableDto`. Same counters — **no per-user flags**.

### Mapper

Both are built by plain C# extension methods in
`src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs` (Mapster is used
only for `Images`, `Tags`, and comments — the article-to-DTO mappings are deliberately hand
written to avoid a `PromotionLevel` auto-flatten NPE):

```csharp
public static async Task<ArticleDetailDto> ToArticleDetailDtoAsync(
    this ArticleEntity entity, IMapper mapper, IFileRepository fileRepository, CancellationToken ct = default);

public static async Task<ArticleSummaryDto> ToArticleSummaryDtoAsync(
    this ArticleEntity entity, IMapper mapper, IFileRepository fileRepository, CancellationToken ct = default);

public static async Task<IReadOnlyList<ArticleSummaryDto>> ToArticleSummaryDtosAsync(
    this IReadOnlyList<ArticleEntity> entities, IMapper mapper, IFileRepository fileRepository, CancellationToken ct = default);
```

These signatures are the seam this feature extends — the per-user flags get threaded through
them.

### Read handlers and endpoints

All under `src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/`.
Every endpoint is `.AllowAnonymous()` with `RateLimitPolicies.ContentBrowsing`. **None are
cached today.**

| Query | Handler | Endpoint route | DTO / shape |
|-------|---------|----------------|-------------|
| `GetArticleBySlug/PublicGetArticleBySlugQuery(string Slug)` | `PublicGetArticleBySlugHandler` | `GET /api/v1/public/articles/{slug}` | `ArticleDetailDto` (single) |
| `GetPublishedArticles/...Query(PaginatedRequest, Search, CategoryId, TagSlug)` | `PublicGetPublishedArticlesHandler` | `GET /api/v1/public/articles/` | `PaginatedResult<ArticleSummaryDto>` |
| `GetPromotedArticles/...Query()` | `PublicGetPromotedArticlesHandler` | `GET /api/v1/public/articles/promoted` | `IReadOnlyList<ArticleSummaryDto>` |
| `GetArticlePromotionFeed/...Query(int StripSize)` | `PublicGetArticlePromotionFeedHandler` | `GET /api/v1/public/articles/promotion/feed` | 3 spots + `IReadOnlyList<ArticleSummaryDto> GossipStrip` |

The get-by-slug handler body today:

```csharp
public async Task<PublicGetArticleBySlugResult> Handle(
    PublicGetArticleBySlugQuery query, CancellationToken cancellationToken)
{
    ArticleEntity? article = await articleRepository.GetBySlugAsync(query.Slug, cancellationToken);

    if (article is null || article.Status != EnumContentStatus.Published)
    {
        throw i18n.Article.NotFound(Guid.Empty);
    }

    var dto = await article.ToArticleDetailDtoAsync(mapper, fileRepository, cancellationToken);
    return new PublicGetArticleBySlugResult(Article: dto);
}
```

The get-by-slug endpoint currently injects only `IDispatcher` — it does **not** read the
`ClaimsPrincipal`. Wiring the optional user is part of Phase 1 (doc 02).

---

## 6. The current-user accessor

### `ICurrentActor`

`src/Shared/Shared/Application/Services/ICurrentActor.cs`:

```csharp
public interface ICurrentActor
{
    /// <summary>
    /// The user identifier extracted from the JWT NameIdentifier claim,
    /// or null when the request is unauthenticated or there is no HTTP context.
    /// </summary>
    string? UserId { get; }

    bool IsAuthenticated { get; }
    bool HasHttpContext { get; }
}
```

`UserId` is a **`string?`** (the raw `ClaimTypes.NameIdentifier` value), not a `Guid`.
Implementation `HttpCurrentActor`
(`src/Shared/Shared/Infrastructure/Services/HttpCurrentActor.cs`) reads it via
`IHttpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)` and is
registered `TryAddSingleton` in `BaseModule`.

### `IClaimsProvider` — the cross-module resolver

`src/Modules/Identity/Identity.Contracts/Application/IClaimsProvider.cs`:

```csharp
public interface IClaimsProvider
{
    /// <summary>
    /// Resolves the authenticated user's ID from the given claims principal.
    /// </summary>
    Guid GetUserIdFromClaims(ClaimsPrincipal user);   // throws when the claim is missing/unparseable

    Guid GetSessionIdFromClaims(ClaimsPrincipal user);
}
```

Implemented by `AuthRepository`, registered `AddScoped<IClaimsProvider, AuthRepository>()`.
It returns a `Guid` and **throws** on a missing claim — so it must only be called when the
request is already known to be authenticated.

### The optional-auth pattern the codebase uses

For an `.AllowAnonymous()` endpoint that still wants the user when present, the codebase
resolves the id **at the endpoint** and passes a `Guid?` into the command/query record. Real
example — `PublicShareVideoEndpointV1`
(`.../Interactions/UseCases/Public/Commands/ShareVideo/V1/PublicShareVideoEndpointV1.cs`):

```csharp
async (string id, ClaimsPrincipal user, IClaimsProvider claimsProvider, IDispatcher dispatcher) =>
{
    Guid videoId = Guid.Parse(id);
    Guid? userId = null;

    if (user.Identity?.IsAuthenticated == true)
    {
        userId = claimsProvider.GetUserIdFromClaims(user: user);
    }

    var command = new PublicShareVideoCommand(VideoId: videoId, UserId: userId);
    // ...
}
```

This feature adopts the **same pattern**: the get-by-slug and feed endpoints gain a
`ClaimsPrincipal user` + `IClaimsProvider claimsProvider` parameter, resolve `Guid? userId`,
and pass it into the query record. Anonymous → `null` → flags `false`. (`ICurrentActor` is
the alternative — inject it in the handler — but it returns a `string?` that would need
parsing, and the endpoint-resolution pattern is what the sibling public interaction
endpoints already use, so we match it.)
