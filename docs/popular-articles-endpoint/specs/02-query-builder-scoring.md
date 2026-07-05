# Spec 02 — Scoring Constants, Query Builder, Repository Method

No dependencies. Implement this first.

---

## 1. Scoring constants

**File:** `src/Modules/Content/Content/Application/Editorial/Constants/PopularArticlesScoring.cs`

```csharp
namespace _116.Content.Application.Editorial.Constants;

/// <summary>
/// Weight constants for the weighted engagement score used to rank popular articles.
/// The score is a linear combination of the article's persisted engagement counters.
/// Weights are ordinal (like &gt; comment &gt; share &gt; bookmark) and are a tunable product
/// decision — retuning here changes the ranking everywhere without touching handler or
/// query-builder logic.
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

---

## 2. Query builder contract

**File:** `src/Modules/Content/Content/Application/Editorial/Builders/Contracts/IPopularArticlesQueryBuilder.cs`

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

## 3. Query builder

**File:** `src/Modules/Content/Content/Application/Editorial/Builders/PopularArticlesQueryBuilder.cs`

Mirrors `PopularTagsQueryBuilder`. Filters are applied first, then the weighted-score
`ORDER BY`, then `Take`. Constants are lifted into `const` locals so EF sees plain column
arithmetic in the expression tree.

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
/// weighted engagement score computed in the database <c>ORDER BY</c>, tie-broken by publish
/// date descending. Mirrors <see cref="PopularTagsQueryBuilder" /> in shape.
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

> The builder is instantiated directly in the repository (`new PopularArticlesQueryBuilder()`),
> exactly like `LookupRepository` does `new PopularTagsQueryBuilder()` — no DI registration is
> required for the builder. The `IPopularArticlesQueryBuilder` interface exists for
> consistency and unit-testability, matching `IPopularTagsQueryBuilder`.

---

## 4. Repository port method

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/IArticleRepository.cs`

Add:

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

---

## 5. Repository implementation

**File:** `src/Modules/Content/Content/Infrastructure/Repositories/ArticleRepository.cs`

Add (mirrors `LookupRepository.GetPopularTagsAsync`):

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

Ensure the `using` for `_116.Content.Application.Editorial.Builders` is present in
`ArticleRepository.cs` (same style as `LookupRepository`'s import of its builders namespace).

---

## Tasks

- [x] Create `PopularArticlesScoring.cs` (4 weight constants)
- [x] Create `Contracts/IPopularArticlesQueryBuilder.cs`
- [x] Create `PopularArticlesQueryBuilder.cs` (filters + weighted `ORDER BY` + `Take`)
- [x] Add `GetPopularArticlesAsync` to `IArticleRepository`
- [x] Implement `GetPopularArticlesAsync` in `ArticleRepository` via the builder
- [ ] Confirm EF translates the score expression to SQL (no client-eval warning) — inspect the
      generated SQL once when running the app locally (the ordering integration test would also
      fail loudly if EF could not translate the expression)
