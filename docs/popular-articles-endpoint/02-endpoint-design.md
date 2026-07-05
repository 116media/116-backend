# Endpoint Design

The new public endpoint mirrors `GetPopularTags` for its endpoint shape and its caching +
invalidation, and reuses `ArticleSummaryDto` from the published-articles slice.

---

## Route

```
GET /api/v1/public/articles/popular?limit=&categoryId=&excludeId=
```

It lives in the **same route group** as the published-articles endpoint
(`{ContentConstants.Public}/{EditorialRouteConstants.Articles}` = `public/articles`), at a
`popular` sub-path — exactly how `GET /api/v1/public/tags/popular` sits under the tags group.

Add the route segment constant to `EditorialRouteConstants.cs`:

```csharp
/// <summary>
/// Route segment for retrieving popularity-ranked editorial entities.
/// Example: /api/v1/public/articles/popular.
/// </summary>
public const string Popular = "popular";
```

### Query parameters

| Param | Type | Default | Meaning |
|-------|------|---------|---------|
| `limit` | `int` | `5` | Max articles to return. Clamped to `1..50`. Sidebar uses ~5. |
| `categoryId` | `Guid?` | `null` | Optional — restrict to one category. |
| `excludeId` | `Guid?` | `null` | Optional — drop this article id (the one being viewed on the detail page). |

`limit` has a small default because the primary consumer is the detail-page sidebar. Unlike
the published list, this endpoint is **not paginated** — popularity sidebars want a small
top-N, and an unbounded skip/take over a computed score is both unnecessary and harder to
cache. A flat `IReadOnlyList<ArticleSummaryDto>` is returned, matching the popular-tags shape.

---

## Query record + result

`src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetPopularArticles/PublicGetPopularArticlesQuery.cs`

```csharp
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Query for retrieving the most popular published articles, ranked by a weighted
/// engagement score.
/// </summary>
/// <param name="Limit">
/// Maximum number of articles to return. Clamped to a small range by the validator.
/// </param>
/// <param name="CategoryId">
/// Optional category filter. When supplied, only articles in that category are ranked.
/// </param>
/// <param name="ExcludeId">
/// Optional article identifier to omit from the result. Used by the article-detail
/// sidebar to drop the article currently being viewed.
/// </param>
public record PublicGetPopularArticlesQuery(int Limit, Guid? CategoryId, Guid? ExcludeId)
    : IQuery<PublicGetPopularArticlesResult>;

/// <summary>
/// Result of the <see cref="PublicGetPopularArticlesQuery" /> containing the ranked
/// article summaries.
/// </summary>
/// <param name="Articles">The popular articles ordered by engagement score descending.</param>
public record PublicGetPopularArticlesResult(IReadOnlyList<ArticleSummaryDto> Articles);
```

---

## Handler (with caching)

`.../GetPopularArticles/PublicGetPopularArticlesHandler.cs`

Structure is a 1:1 mirror of `PublicGetPopularTagsHandler`: try the cache, on miss hit the
repository, map to DTOs, store with an absolute-expiration TTL and the shared eviction token.

```csharp
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMemoryCache cache,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularArticlesQuery, PublicGetPopularArticlesResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public async Task<PublicGetPopularArticlesResult> Handle(
        PublicGetPopularArticlesQuery query,
        CancellationToken cancellationToken
    )
    {
        string categoryPart = query.CategoryId?.ToString() ?? "all";
        string excludePart = query.ExcludeId?.ToString() ?? "none";
        string cacheKey = $"popular_articles_{query.Limit}_{categoryPart}_{excludePart}";

        if (cache.TryGetValue(cacheKey, out IReadOnlyList<ArticleSummaryDto>? cached) && cached is not null)
        {
            return new PublicGetPopularArticlesResult(Articles: cached);
        }

        IReadOnlyList<ArticleEntity> articles = await articleRepository.GetPopularArticlesAsync(
            limit: query.Limit,
            categoryId: query.CategoryId,
            excludeId: query.ExcludeId,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<ArticleSummaryDto> dtoList = await articles.ToArticleSummaryDtosAsync(
            mapper,
            fileRepository,
            cancellationToken
        );

        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheTtl)
            .AddExpirationToken(new CancellationChangeToken(cacheInvalidator.GetEvictionToken()));

        cache.Set(cacheKey, dtoList, options);

        return new PublicGetPopularArticlesResult(Articles: dtoList);
    }
}
```

### Cache key

`popular_articles_{limit}_{categoryId|all}_{excludeId|none}`. Every distinct
`(limit, categoryId, excludeId)` combination is its own entry. `excludeId` is part of the key
because two detail pages request different exclusions; including it keeps entries correct.
Cardinality is bounded in practice — the sidebar uses a fixed `limit` (~5) and a small set of
categories, and `excludeId` is one per currently-trafficked article.

> Alternative considered: **omit `excludeId` from the cache key** — cache the unfiltered
> top-`limit+1`, then drop `excludeId` in the handler after the cache read. This shrinks
> cache cardinality to `(limit, categoryId)` at the cost of a tiny post-filter and requesting
> one extra row from the DB. This is the recommended optimization for high article traffic;
> both approaches are documented in `06-caching-and-rollout.md`. The spec ships the simpler
> key-includes-`excludeId` form first.

All entries share one eviction token from `IPopularArticlesCacheInvalidator`, so a single
`Invalidate()` clears every combination — identical to the popular-tags design.

---

## Validator

`.../GetPopularArticles/PublicGetPopularArticlesValidator.cs` — clamp is enforced at the
endpoint (see below); the validator guards the dispatched query. Per the module's validator
conventions there is no inline rule: the range check lives in a shared
`EditorialValidation.ValidPopularArticlesLimit` extension whose bounds come from
`PopularArticlesLimits` and whose message is localized via the `ArticleErrorMessage`
resources (`PopularLimitOutOfRange`, en + fr):

```csharp
public class PublicGetPopularArticlesValidator : AbstractValidator<PublicGetPopularArticlesQuery>
{
    public PublicGetPopularArticlesValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Limit).ValidPopularArticlesLimit(i18n.Article.Msg);
    }
}
```

---

## Response + endpoint

`.../GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1.cs`

```csharp
public record PublicGetPopularArticlesResponse(IReadOnlyList<ArticleSummaryDto> Articles);

public class PublicGetPopularArticlesEndpointV1 : ICarterModule
{
    private const int DefaultLimit = 5;
    private const int MaxLimit = 50;

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Articles}/{EditorialRouteConstants.Popular}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Articles}");

        group
            .MapGet(
                "/",
                async (
                    IDispatcher dispatcher,
                    int limit = DefaultLimit,
                    Guid? categoryId = null,
                    Guid? excludeId = null
                ) =>
                {
                    int safeLimit = Math.Clamp(limit, 1, MaxLimit);

                    var query = new PublicGetPopularArticlesQuery(
                        Limit: safeLimit,
                        CategoryId: categoryId,
                        ExcludeId: excludeId
                    );

                    PublicGetPopularArticlesResult result = await dispatcher.Send(request: query);

                    var response = new PublicGetPopularArticlesResponse(Articles: result.Articles);
                    return Results.Ok(response);
                }
            )
            .WithName(endpointName: PublicGetPopularArticlesMetaField.PublicGetPopularArticles.Name)
            .WithSummary(summary: PublicGetPopularArticlesMetaField.PublicGetPopularArticles.Summary)
            .WithDescription(description: PublicGetPopularArticlesMetaField.PublicGetPopularArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPopularArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
```

| Concern | Value | Same as popular-tags? |
|---------|-------|-----------------------|
| Auth | `AllowAnonymous()` | Yes |
| Rate limit | `RateLimitPolicies.ContentBrowsing` | Yes |
| API version | `MapApiVersionGroup(1)` | Yes |
| Caching | `IMemoryCache` + eviction token | Yes |
| Response | flat `IReadOnlyList<...>` wrapped in a response record | Yes |

`PublicGetPopularArticlesMetaField.cs` provides `Name` / `Summary` / `Description` constants,
same convention as `PublicGetPopularTagsMetaField`.

---

## Cache invalidator — reuse vs new

**Recommendation: a new `IPopularArticlesCacheInvalidator`, not reuse of the tags one.**

- Reusing `IPopularTagsCacheInvalidator` would couple two unrelated caches: every tag-graph
  change would needlessly evict the popular-articles cache, and every engagement change would
  needlessly evict the popular-tags cache. They have different invalidation triggers.
- The invalidator is a trivial, stateless (bar the token) singleton — duplicating it costs a
  few lines and keeps eviction precise.

The new invalidator is identical in shape to the tags one (see `specs/03-cache-invalidator.md`).
It is invalidated on the mutations that change engagement counts or publish state — see
`06-caching-and-rollout.md` for the full list.
