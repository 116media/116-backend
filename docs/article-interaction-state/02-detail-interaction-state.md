# Phase 1 — Detail Interaction State

Add per-user `IsLiked` / `IsBookmarked` to `ArticleDetailDto` for the single-article page
(`GET /api/v1/public/articles/{slug}`). Anonymous readers get `false` for both; authenticated
readers get the true value from the join tables.

Full C# with `## Tasks` checklist is in [specs/01-detail-flags.md](specs/01-detail-flags.md).
This document is the design narrative.

---

## 1. DTO — add two flags to `ArticleDetailDto`

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/ArticleDetailDto.cs`

Append two boolean parameters to the record. They default to `false`, so the write-side call
sites that build the DTO for admin flows (and any test that does not care) keep compiling
without change, and an anonymous read naturally yields `false`.

```csharp
/// <param name="IsLiked">
/// Whether the current authenticated user has liked this article.
/// False for anonymous requests and for users who have not liked it.
/// </param>
/// <param name="IsBookmarked">
/// Whether the current authenticated user has bookmarked this article.
/// False for anonymous requests and for users who have not bookmarked it.
/// </param>
public record ArticleDetailDto(
    // ... existing parameters unchanged ...
    Guid? CustomerId = null,
    string? CustomerName = null,
    Guid? OrderItemId = null,
    AuthorDto? Author = null,
    bool IsLiked = false,
    bool IsBookmarked = false
) : AuditableDto;
```

> Keep `IsLiked` / `IsBookmarked` as the **last** optional parameters so every existing
> positional construction of the record stays valid.

---

## 2. Repository — reuse the existing existence methods

No new repository methods are needed for Phase 1. `IArticleRepository` already exposes:

```csharp
Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);
Task<bool> HasBookmarkedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);
```

These are the same methods the like/bookmark **write** handlers use as their idempotency
guard, so their correctness is already covered. Phase 1 only reads them.

> The task brief suggested method names `HasUserLikedArticleAsync(userId, articleId)` /
> `HasUserBookmarkedArticleAsync(...)`. The repository already ships equivalents named
> `HasLikedAsync` / `HasBookmarkedAsync` — **reuse them** rather than add redundant aliases.

---

## 3. Mapper — thread the flags through `ToArticleDetailDtoAsync`

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/ArticleMapper.cs`

The cleanest seam is to let the caller (the handler) pass the two resolved booleans into the
extension method, so the mapper stays free of repository/user concerns beyond what it already
has:

```csharp
public static async Task<ArticleDetailDto> ToArticleDetailDtoAsync(
    this ArticleEntity entity,
    IMapper mapper,
    IFileRepository fileRepository,
    CancellationToken ct = default,
    bool isLiked = false,
    bool isBookmarked = false
)
{
    string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);

    return new ArticleDetailDto(
        // ... existing arguments unchanged ...
        entity.CustomerId,
        entity.Customer != null ? entity.Customer.FullName : null,
        entity.OrderItemId
    )
    {
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy,
        IsLiked = isLiked,
        IsBookmarked = isBookmarked,
    };
}
```

Setting `IsLiked` / `IsBookmarked` in the object initializer keeps them independent of the
positional argument list and reads clearly alongside the auditable fields.

---

## 4. Query + handler — resolve the optional user, set the flags

**Query** — add an optional `Guid? CurrentUserId` to the query record:

```csharp
/// <param name="Slug">The URL-safe slug of the article to retrieve.</param>
/// <param name="CurrentUserId">
/// The authenticated caller's id, or null for an anonymous request. When null,
/// per-user interaction flags resolve to false.
/// </param>
public record PublicGetArticleBySlugQuery(string Slug, Guid? CurrentUserId = null)
    : IQuery<PublicGetArticleBySlugResult>;
```

**Handler** — `PublicGetArticleBySlugHandler`:

```csharp
public async Task<PublicGetArticleBySlugResult> Handle(
    PublicGetArticleBySlugQuery query,
    CancellationToken cancellationToken
)
{
    ArticleEntity? article = await articleRepository.GetBySlugAsync(query.Slug, cancellationToken);

    if (article is null || article.Status != EnumContentStatus.Published)
    {
        throw i18n.Article.NotFound(Guid.Empty);
    }

    bool isLiked = false;
    bool isBookmarked = false;

    if (query.CurrentUserId is Guid userId)
    {
        isLiked = await articleRepository.HasLikedAsync(userId, article.Id, cancellationToken);
        isBookmarked = await articleRepository.HasBookmarkedAsync(userId, article.Id, cancellationToken);
    }

    var dto = await article.ToArticleDetailDtoAsync(
        mapper,
        fileRepository,
        cancellationToken,
        isLiked: isLiked,
        isBookmarked: isBookmarked
    );

    return new PublicGetArticleBySlugResult(Article: dto);
}
```

Anonymous request → `CurrentUserId` is `null` → the `if` is skipped → both flags stay
`false` → **zero extra queries** for anonymous traffic. Authenticated request → two indexed
`AnyAsync` existence checks against the unique `(user_id, article_id)` index. Cheap and exact.

---

## 5. Endpoint — resolve the user with the optional-auth pattern

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1.cs`

Adopt the same pattern as `PublicShareVideoEndpointV1`: add `ClaimsPrincipal user` and
`IClaimsProvider claimsProvider` to the handler lambda, resolve `Guid? userId` when
authenticated, and pass it into the query.

```csharp
group
    .MapGet(
        "/{slug}",
        async (
            string slug,
            ClaimsPrincipal user,
            IClaimsProvider claimsProvider,
            IDispatcher dispatcher
        ) =>
        {
            Guid? userId = null;
            if (user.Identity?.IsAuthenticated == true)
            {
                userId = claimsProvider.GetUserIdFromClaims(user: user);
            }

            var query = new PublicGetArticleBySlugQuery(Slug: slug, CurrentUserId: userId);
            PublicGetArticleBySlugResult result = await dispatcher.Send(request: query);

            var response = new PublicGetArticleBySlugResponse(Article: result.Article);
            return Results.Ok(response);
        }
    )
    .WithName(endpointName: PublicGetArticleBySlugMetaField.PublicGetArticleBySlug.Name)
    // ... existing WithSummary / WithDescription / AllowAnonymous / rate limiting / Produces unchanged ...
    ;
```

The endpoint stays `.AllowAnonymous()`. The JWT middleware still populates
`ClaimsPrincipal user` when a valid bearer token is present; when it is not, `IsAuthenticated`
is `false` and `userId` stays `null`.

---

## Files changed (Phase 1)

| File | Change |
|------|--------|
| `Content/Application/Shared/DTOs/ArticleDetailDto.cs` | Add `bool IsLiked = false`, `bool IsBookmarked = false` (last optional params) |
| `Content/Application/Shared/Mappers/ArticleMapper.cs` | Thread `isLiked` / `isBookmarked` into `ToArticleDetailDtoAsync` |
| `.../GetArticleBySlug/PublicGetArticleBySlugQuery.cs` | Add `Guid? CurrentUserId = null` |
| `.../GetArticleBySlug/PublicGetArticleBySlugHandler.cs` | Resolve flags when `CurrentUserId` is set |
| `.../GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1.cs` | Resolve optional user, pass into query |

No new repository methods. No EF migration.
