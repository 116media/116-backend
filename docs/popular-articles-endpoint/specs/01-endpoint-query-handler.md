# Spec 01 — Endpoint, Query, Handler, Response, Caching

Depends on spec 02 (`GetPopularArticlesAsync` on the repository) and spec 03
(`IPopularArticlesCacheInvalidator`).

---

## 1. Route segment constant

**File:** `src/Modules/Content/Content/Application/Editorial/Constants/EditorialRouteConstants.cs`

Add:

```csharp
/// <summary>
/// Route segment for retrieving popularity-ranked editorial entities.
/// Example: /api/v1/public/articles/popular.
/// </summary>
public const string Popular = "popular";
```

---

## 2. Query record + result

**File:** `.../UseCases/Public/Queries/GetPopularArticles/PublicGetPopularArticlesQuery.cs`

```csharp
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Query for retrieving the most popular published articles, ranked by a weighted
/// engagement score.
/// </summary>
/// <param name="Limit">
/// Maximum number of articles to return. Validated to a small inclusive range.
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

## 3. Validator

**File:** `.../GetPopularArticles/PublicGetPopularArticlesValidator.cs`

No inline rules — the range check is a shared `EditorialValidation` extension with a
localized message, matching the module's validator conventions:

```csharp
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Validators;
using FluentValidation;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Validator for the <see cref="PublicGetPopularArticlesQuery" /> ensuring the limit stays
/// within the accepted range.
/// </summary>
public class PublicGetPopularArticlesValidator : AbstractValidator<PublicGetPopularArticlesQuery>
{
    /// <summary>
    /// Initializes a new instance of <see cref="PublicGetPopularArticlesValidator" /> with the specified error message provider.
    /// </summary>
    /// <param name="i18n">Content module i18n facade.</param>
    public PublicGetPopularArticlesValidator(ContentI18n i18n)
    {
        RuleFor(x => x.Limit).ValidPopularArticlesLimit(i18n.Article.Msg);
    }
}
```

Supporting pieces (all in `Application/Shared`):

- `Validators/EditorialValidation.cs` — `ValidPopularArticlesLimit<T>` extension applying
  `InclusiveBetween(PopularArticlesLimits.MinLimit, PopularArticlesLimits.MaxLimit)` with
  the localized message.
- `Errors/Messages/ArticleErrorMessage.cs` — `PopularLimitOutOfRange(int min, int max)`.
- `Errors/Messages/ArticleErrorMessage.resx` / `.en.resx` / `.fr.resx` — the
  `PopularLimitOutOfRange` resource key (`{0}` = min, `{1}` = max).

---

## 4. Meta field

**File:** `.../GetPopularArticles/PublicGetPopularArticlesMetaField.cs`

Mirror `PublicGetPopularTagsMetaField`. Provide `Name`, `Summary`, `Description` constants:

```csharp
namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// OpenAPI metadata for the public get-popular-articles endpoint.
/// </summary>
public static class PublicGetPopularArticlesMetaField
{
    /// <summary>
    /// Metadata for the get-popular-articles operation.
    /// </summary>
    public static class PublicGetPopularArticles
    {
        /// <summary>
        /// The unique route name of the endpoint.
        /// </summary>
        public const string Name = "PublicGetPopularArticles";

        /// <summary>
        /// The short OpenAPI summary line.
        /// </summary>
        public const string Summary = "Get popular articles";

        /// <summary>
        /// The long OpenAPI description.
        /// </summary>
        public const string Description =
            "Returns published articles ranked by a weighted engagement score "
            + "(likes, comments, shares, bookmarks), tie-broken by publish date. "
            + "Supports an optional category filter and an optional article id to exclude. "
            + "Results are cached in-process for a short window.";
    }
}
```

---

## 5. Handler (with caching)

**File:** `.../GetPopularArticles/PublicGetPopularArticlesHandler.cs`

Mirrors `PublicGetPopularTagsHandler` exactly (cache try → repo → map → set with token).

```csharp
using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles;

/// <summary>
/// Handles the <see cref="PublicGetPopularArticlesQuery" /> to retrieve the most popular
/// published articles ranked by a weighted engagement score.
/// </summary>
/// <remarks>
/// Results are cached in-process for <see cref="CacheTtl" /> to avoid running the ranking
/// query on every request. Each distinct combination of limit, category, and excluded id
/// produces its own cache entry; all entries share the eviction token supplied by
/// <see cref="IPopularArticlesCacheInvalidator" />, so any engagement or publish-state
/// mutation cancels the token and evicts every entry at once.
/// </remarks>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="fileRepository">Repository for resolving cover image URLs.</param>
/// <param name="cache">In-process memory cache.</param>
/// <param name="cacheInvalidator">Token source used to evict all popular-articles entries on mutation.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetPopularArticlesHandler(
    IArticleRepository articleRepository,
    IFileRepository fileRepository,
    IMemoryCache cache,
    IPopularArticlesCacheInvalidator cacheInvalidator,
    IMapper mapper
) : IQueryHandler<PublicGetPopularArticlesQuery, PublicGetPopularArticlesResult>
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    /// <inheritdoc />
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

---

## 6. Response + endpoint

**File:** `.../GetPopularArticles/V1/PublicGetPopularArticlesEndpointV1.cs`

```csharp
using _116.BuildingBlocks.Constants.RateLimit;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Constants;
using _116.Shared.Application.Extensions;
using _116.Shared.Contracts.Application.CQRS;
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetPopularArticles.V1;

/// <summary>
/// Response model for listing popular articles.
/// </summary>
/// <param name="Articles">The articles ordered by engagement score descending.</param>
public record PublicGetPopularArticlesResponse(IReadOnlyList<ArticleSummaryDto> Articles);

/// <summary>
/// Defines the public get-popular-articles endpoint. Returns published articles ranked by a
/// weighted engagement score, cached for a short window.
/// </summary>
public class PublicGetPopularArticlesEndpointV1 : ICarterModule
{
    private const int DefaultLimit = 5;
    private const int MaxLimit = 50;

    /// <summary>
    /// Configures the popular-articles retrieval route within the API pipeline.
    /// Maps the <c>GET /api/v1/public/articles/popular</c> endpoint.
    /// </summary>
    /// <param name="app">The route builder used to register API endpoints.</param>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup(
                $"{ContentConstants.Public}/{EditorialRouteConstants.Articles}/{EditorialRouteConstants.Popular}"
            )
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
            .WithName(endpointName: PublicGetPopularArticlesMetaField.GetPopularArticles.Name)
            .WithSummary(summary: PublicGetPopularArticlesMetaField.GetPopularArticles.Summary)
            .WithDescription(description: PublicGetPopularArticlesMetaField.GetPopularArticles.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetPopularArticlesResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
```

The `limit` is clamped in the endpoint (defense in depth); the validator additionally guards
the dispatched query. Handlers and validators are auto-discovered by the existing CQRS
registration (same as every other slice) — no manual handler registration needed.

---

## Tasks

- [x] Add `Popular` constant to `EditorialRouteConstants.cs`
- [x] Create `PublicGetPopularArticlesQuery.cs` (query + result records)
- [x] Create `PublicGetPopularArticlesValidator.cs`
- [x] Create `PublicGetPopularArticlesMetaField.cs` (as a `RouteMetadata`, matching `PublicGetPopularTagsMetaField`)
- [x] Create `PublicGetPopularArticlesHandler.cs` with `IMemoryCache` + eviction token
- [x] Create `V1/PublicGetPopularArticlesEndpointV1.cs` (response + Carter module)
- [x] Confirm the handler resolves `IPopularArticlesCacheInvalidator` (registered in spec 03)
- [x] Verify route lands at `GET /api/v1/public/articles/popular`, `AllowAnonymous`, `ContentBrowsing`
