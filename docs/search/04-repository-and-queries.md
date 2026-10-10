# Repository and Queries

## Checklist

- [ ] `ISearchRepository` in `Content.Application/Shared/Repositories/`
- [ ] `SearchRepository` in `Content.Infrastructure/Repositories/`
- [ ] Registered in `ContentModule`
- [ ] Mock added to `Content.TestData/Mocks/Repositories/`
- [ ] Integration tests resolve it from DI

---

## Why a dedicated repository

The existing repositories are per-aggregate and return entities. Search needs
*scored projections across two aggregates*, ordered by a value that only exists in
the query. Putting that on `IArticleRepository` would make it return a shape no other
caller wants, and would leave the cross-type union homeless.

`ISearchRepository` reads from both tables and owns no aggregate. That is acceptable
for a read-only query service and mirrors how `ICategoryDtoService` already spans
repositories.

---

## Port

```csharp
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Read-only full-text search across the public content types.
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Returns the ranked published articles matching the query, newest first on ties.
    /// </summary>
    /// <param name="query">The reader's query, already trimmed.</param>
    /// <param name="skip">Rows to skip.</param>
    /// <param name="take">Rows to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The matching hits, highest score first.</returns>
    Task<IReadOnlyList<SearchHitDto>> SearchArticlesAsync(
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the ranked published videos matching the query, newest first on ties.
    /// </summary>
    /// <param name="query">The reader's query, already trimmed.</param>
    /// <param name="skip">Rows to skip.</param>
    /// <param name="take">Rows to return.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The matching hits, highest score first.</returns>
    Task<IReadOnlyList<SearchHitDto>> SearchVideosAsync(
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the matching published articles and videos for the query.
    /// </summary>
    /// <param name="query">The reader's query, already trimmed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The counts per scope.</returns>
    Task<SearchCountsDto> CountAsync(string query, CancellationToken cancellationToken = default);
}
```

---

## Implementation

`EF.Functions.WebSearchToTsQuery` and `Matches` map to `websearch_to_tsquery` and `@@`.
`ts_rank_cd` has no EF mapping, so it is declared as a DbFunction on the context.

### The rank DbFunction

```csharp
// Content.Infrastructure/Persistence/ContentDbContext.cs

/// <summary>
/// Maps PostgreSQL's ts_rank_cd for use in LINQ projections.
/// </summary>
/// <param name="vector">The indexed search vector.</param>
/// <param name="query">The parsed tsquery.</param>
/// <returns>The cover-density relevance score.</returns>
[DbFunction("ts_rank_cd", IsBuiltIn = true)]
public static double RankCd(NpgsqlTsVector vector, NpgsqlTsQuery query) =>
    throw new NotSupportedException("Translated by the provider; never called in memory.");
```

The throw is deliberate: calling it client-side is a bug, and a silent `0` would hide it.

### Repository

```csharp
using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Helpers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Full-text search over the published articles and videos, ranked by cover density.
/// </summary>
/// <param name="context">The Content database context.</param>
public class SearchRepository(ContentDbContext context) : ISearchRepository
{
    private const string Config = "french";

    /// <inheritdoc />
    public async Task<IReadOnlyList<SearchHitDto>> SearchArticlesAsync(
        string query,
        int skip,
        int take,
        CancellationToken cancellationToken = default
    )
    {
        NpgsqlTsQuery parsed = EF.Functions.WebSearchToTsQuery(Config, query);

        return await context
            .Articles.AsNoTracking()
            .Where(article =>
                article.Status == EnumContentStatus.Published && article.SearchVector.Matches(parsed)
            )
            .Select(article => new
            {
                article.Id,
                article.Slug,
                article.Title,
                article.Headline,
                article.CategoryId,
                article.PublishedAt,
                Score = ContentDbContext.RankCd(article.SearchVector, parsed),
            })
            .OrderByDescending(row => row.Score)
            .ThenByDescending(row => row.PublishedAt)
            .Skip(skip)
            .Take(take)
            .Select(row => new SearchHitDto(
                Type: EnumCoreContentType.Article,
                Id: row.Id,
                Slug: row.Slug,
                Title: row.Title,
                Excerpt: row.Headline,
                ImageUrl: null,
                CategoryName: string.Empty,
                PublishedAt: row.PublishedAt,
                Score: row.Score,
                RatingAverage: null,
                DurationSeconds: null
            ))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SearchCountsDto> CountAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        NpgsqlTsQuery parsed = EF.Functions.WebSearchToTsQuery(Config, query);

        int articles = await context.Articles.CountAsync(
            article =>
                article.Status == EnumContentStatus.Published && article.SearchVector.Matches(parsed),
            cancellationToken
        );

        int videos = await context.Videos.CountAsync(
            video => video.Status == EnumContentStatus.Published && video.SearchVector.Matches(parsed),
            cancellationToken
        );

        return new SearchCountsDto(All: articles + videos, Articles: articles, Videos: videos);
    }
}
```

`SearchVideosAsync` is the same shape over `context.Videos`, projecting
`EnumCoreContentType.Video`, `video.Description` as the excerpt, and carrying
`RatingAverage` and `DurationSeconds`.

### Two deliberate gaps in the projection

`ImageUrl` is `null` and `CategoryName` is empty in the repository. Both are resolved by
the handler, because:

- **Images** live in the Storage module behind `IFileStorageService` and are resolved in
  one batch for the whole page, the `files` pattern already used by `VideoDtoFactory`.
  Resolving per row here would be an N+1 across a module boundary.
- **Category names** come from `ICategoryRepository.GetByIdsAsync`, also batched.

The repository therefore returns the row's `CategoryId` through the anonymous
projection and the handler fills the display fields. Keeping the DTO flat means the
handler rewrites the record with `with` rather than mapping a second shape.

---

## Excerpt

`ExcerptHelper.Truncate` is applied in the **handler**, not in the LINQ projection:
calling it inside `Select` would not translate and would silently pull the whole column
into memory. Articles project `Headline`, which is already short; videos project
`Description`, which is not, so truncation matters there.

---

## Registration

```csharp
// Content.Infrastructure/ContentModule.cs
services.AddScoped<ISearchRepository, SearchRepository>();
```

---

## The trigram fallback (phase 4)

Not implemented at launch. When it is, it belongs here as a second method rather than a
branch inside the first, so the handler decides when to call it:

```csharp
Task<IReadOnlyList<SearchHitDto>> SearchArticlesByTrigramAsync(
    string query, int take, CancellationToken cancellationToken = default);
```

The handler calls it only when the FTS arm returns fewer than 5 rows, per
`../../../../docs/search/04-relevance-and-ranking.md`.
