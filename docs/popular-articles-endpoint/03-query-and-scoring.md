# Query Builder and Scoring

The ranking is the heart of this feature. This document defines the weighted score, why it is
computed in the SQL `ORDER BY`, and the `PopularArticlesQueryBuilder` that mirrors
`PopularTagsQueryBuilder`.

---

## Scoring constants

`src/Modules/Content/Content/Application/Editorial/Constants/PopularArticlesScoring.cs`

```csharp
namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Weight constants for the weighted engagement score used to rank popular articles.
/// The score is computed as a linear combination of the article's persisted engagement
/// counters. Weights are ordinal (like &gt; comment &gt; share &gt; bookmark) and are tuned as a
/// product decision — retuning here changes the ranking everywhere without touching handler
/// or query-builder logic.
/// </summary>
public static class PopularArticlesScoring
{
    /// <summary>
    /// Weight applied to <c>LikeCount</c>. Highest weight: a like is the most direct
    /// expression of reader approval and the primary popularity signal.
    /// </summary>
    public const int LikeWeight = 4;

    /// <summary>
    /// Weight applied to <c>CommentCount</c>. High weight: commenting is high-effort and
    /// signals discussion around the article.
    /// </summary>
    public const int CommentWeight = 3;

    /// <summary>
    /// Weight applied to <c>ShareCount</c>. Medium weight: a share redistributes the
    /// article to other readers.
    /// </summary>
    public const int ShareWeight = 2;

    /// <summary>
    /// Weight applied to <c>BookmarkCount</c>. Baseline weight: a bookmark is private
    /// intent to return, meaningful but non-amplifying.
    /// </summary>
    public const int BookmarkWeight = 1;
}
```

Resulting score:

```
score = LikeWeight     * LikeCount
      + CommentWeight  * CommentCount
      + ShareWeight    * ShareCount
      + BookmarkWeight * BookmarkCount
```

All operands are non-negative `int`; the whole expression is a non-negative `int`.

---

## Where the score is computed — DB, not memory

**Decision: compute the score inside the EF `IQueryable` so PostgreSQL evaluates it in the
`ORDER BY`, then `Take(limit)` in the database.** Rationale:

- **Correctness of top-N:** to return the top `limit` by score, the ranking must be applied
  *before* `Take`. Fetching all published articles into memory to sort them is unbounded and
  defeats the purpose of a cheap sidebar endpoint.
- **EF-translatable:** the score is a pure linear integer expression over columns
  (`w1*LikeCount + w2*CommentCount + ...`). EF Core translates this arithmetic directly into
  a SQL `ORDER BY` — no client evaluation, no raw SQL needed. This is exactly how
  `PopularTagsQueryBuilder` orders by a computed count expression.
- **Consistency with the existing pattern:** `PopularTagsQueryBuilder` already projects a
  computed `totalCount` and orders by it server-side. The articles builder does the same with
  the weighted score.

Constants are captured into locals before building the expression tree so EF sees plain
arithmetic over columns (not static field access), which keeps the translation clean.

---

## Ordering expression

```
ORDER BY (LikeWeight     * a.LikeCount
        + CommentWeight  * a.CommentCount
        + ShareWeight    * a.ShareCount
        + BookmarkWeight * a.BookmarkCount) DESC,
         a.PublishedAt DESC
```

With the recommended weights (`4/3/2/1`) this is:

```
ORDER BY (4*like_count + 3*comment_count + 2*share_count + 1*bookmark_count) DESC,
         published_at DESC
```

`PublishedAt` is the tie-breaker (fresher wins among equal scores). Because only `Published`
articles are ranked and `Publish()` sets `PublishedAt`, it is non-null for every candidate
row — so the tie-break is deterministic.

---

## Filter composition

Three filters compose independently of the ordering:

| Filter | Rule | Always applied? |
|--------|------|-----------------|
| Published only | `a.Status == EnumContentStatus.Published` | Yes |
| Category | `a.CategoryId == categoryId` | Only when `categoryId` supplied |
| Exclude | `a.Id != excludeId` | Only when `excludeId` supplied |

The status and category predicates reuse the existing `ArticleByStatusSpecification` and
`ArticleByCategorySpecification`. The `excludeId` predicate is applied as a plain `Where` in
the builder (there is no existing single-id-exclusion spec, and one predicate does not warrant
a new spec class — but a `ArticleExcludeByIdSpecification` is an equally valid choice if the
team prefers all predicates to be specs).

---

## PopularArticlesQueryBuilder

`src/Modules/Content/Content/Application/Editorial/Builders/PopularArticlesQueryBuilder.cs`

Mirrors `PopularTagsQueryBuilder`: fluent `With*` setters, a `Build(context)` that returns the
ordered, filtered, limited `IQueryable<ArticleEntity>`.

```csharp
using _116.Content.Application.Editorial.Builders.Contracts;
using _116.Content.Application.Editorial.Constants;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Application.Editorial.Builders;

/// <summary>
/// Builder for constructing the popular-articles query. Ranks published articles by a
/// weighted engagement score computed in the database <c>ORDER BY</c>, tie-broken by
/// publish date. Mirrors <see cref="PopularTagsQueryBuilder" /> in shape.
/// </summary>
public class PopularArticlesQueryBuilder : IPopularArticlesQueryBuilder
{
    private Guid? _categoryId;
    private Guid? _excludeId;
    private int? _limit;

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithCategory(Guid? categoryId)
    {
        _categoryId = categoryId;
        return this;
    }

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithExcludeId(Guid? excludeId)
    {
        _excludeId = excludeId;
        return this;
    }

    /// <inheritdoc />
    public IPopularArticlesQueryBuilder WithLimit(int? limit)
    {
        _limit = limit;
        return this;
    }

    /// <inheritdoc />
    public IQueryable<ArticleEntity> Build(ContentDbContext context)
    {
        const int likeWeight = PopularArticlesScoring.LikeWeight;
        const int commentWeight = PopularArticlesScoring.CommentWeight;
        const int shareWeight = PopularArticlesScoring.ShareWeight;
        const int bookmarkWeight = PopularArticlesScoring.BookmarkWeight;

        IQueryable<ArticleEntity> query = context
            .Articles.Include(a => a.Category)
            .Where(a => a.Status == EnumContentStatus.Published);

        if (_categoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == _categoryId.Value);
        }

        if (_excludeId.HasValue)
        {
            query = query.Where(a => a.Id != _excludeId.Value);
        }

        query = query
            .OrderByDescending(a =>
                (likeWeight * a.LikeCount)
                + (commentWeight * a.CommentCount)
                + (shareWeight * a.ShareCount)
                + (bookmarkWeight * a.BookmarkCount)
            )
            .ThenByDescending(a => a.PublishedAt);

        if (_limit.HasValue)
        {
            query = query.Take(_limit.Value);
        }

        return query;
    }
}
```

Builder contract interface:

`src/Modules/Content/Content/Application/Editorial/Builders/Contracts/IPopularArticlesQueryBuilder.cs`

```csharp
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;

namespace _116.Content.Application.Editorial.Builders.Contracts;

/// <summary>
/// Interface for building the popular-articles query using the builder pattern.
/// Encapsulates the weighted-score ordering and the published/category/exclude filters.
/// </summary>
public interface IPopularArticlesQueryBuilder
{
    /// <summary>
    /// Restricts ranking to a single category. When not called, all categories are ranked.
    /// </summary>
    IPopularArticlesQueryBuilder WithCategory(Guid? categoryId);

    /// <summary>
    /// Omits a specific article id from the result (the article being viewed on a detail page).
    /// </summary>
    IPopularArticlesQueryBuilder WithExcludeId(Guid? excludeId);

    /// <summary>
    /// Limits the number of ranked articles returned.
    /// </summary>
    IPopularArticlesQueryBuilder WithLimit(int? limit);

    /// <summary>
    /// Builds the ordered, filtered, optionally limited query.
    /// </summary>
    IQueryable<ArticleEntity> Build(ContentDbContext context);
}
```

---

## Repository port method

`src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs` — add:

```csharp
/// <summary>
/// Retrieves the most popular published articles, ranked by a weighted engagement score
/// (like, comment, share, bookmark) and tie-broken by publish date descending.
/// </summary>
/// <param name="limit">Maximum number of articles to return.</param>
/// <param name="categoryId">Optional category filter.</param>
/// <param name="excludeId">Optional article id to omit from the result.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>The ranked list of article entities.</returns>
Task<IReadOnlyList<ArticleEntity>> GetPopularArticlesAsync(
    int limit,
    Guid? categoryId,
    Guid? excludeId,
    CancellationToken cancellationToken = default
);
```

Implementation in `ArticleRepository.cs` (mirrors `LookupRepository.GetPopularTagsAsync`):

```csharp
/// <inheritdoc />
public async Task<IReadOnlyList<ArticleEntity>> GetPopularArticlesAsync(
    int limit,
    Guid? categoryId,
    Guid? excludeId,
    CancellationToken cancellationToken = default
)
{
    IQueryable<ArticleEntity> query = new PopularArticlesQueryBuilder()
        .WithCategory(categoryId)
        .WithExcludeId(excludeId)
        .WithLimit(limit)
        .Build(context);

    return await query.ToListAsync(cancellationToken);
}
```

---

## Why not a Specification for ordering

The existing `Specification<ArticleEntity>` abstraction expresses a `bool` predicate
(`ToExpression()` returns `Expression<Func<ArticleEntity, bool>>`). Ordering by a computed
score is not a predicate, so it cannot live in a spec — this is exactly why the popular-tags
slice uses a dedicated *query builder* rather than the specification pipeline. The
popular-articles slice follows the same reasoning: filters can be specs, ordering must be a
builder.
