# Spec 03 — Cache Invalidator and Mutation Wiring

Mirrors `IPopularTagsCacheInvalidator` / `PopularTagsCacheInvalidator` exactly.

---

## 1. Invalidator interface

**File:** `src/Modules/Content/Content/Application/Shared/Cache/IPopularArticlesCacheInvalidator.cs`

```csharp
namespace _116.Content.Application.Shared.Cache;

/// <summary>
/// Provides a mechanism to invalidate the popular-articles cache when article engagement
/// counters or publish state change.
/// </summary>
/// <remarks>
/// The popular-articles query ranks published articles by a weighted engagement score and
/// caches the result in-process. Any operation that changes an engagement counter
/// (like, comment, share, bookmark) or an article's membership in the published set
/// (publish, archive, delete) must call <see cref="Invalidate" /> after committing, so the
/// next read reflects the updated ranking. The implementation uses a
/// <see cref="CancellationToken" /> registered as a
/// <see cref="Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions" /> expiration
/// token, so a single cancellation evicts every entry regardless of the limit, category, or
/// exclude-id used when the entry was stored.
/// </remarks>
public interface IPopularArticlesCacheInvalidator
{
    /// <summary>
    /// Returns a <see cref="CancellationToken" /> that cache entries should register as an
    /// expiration token. When <see cref="Invalidate" /> is called the token is cancelled,
    /// evicting all associated entries immediately.
    /// </summary>
    /// <returns>The current eviction <see cref="CancellationToken" />.</returns>
    CancellationToken GetEvictionToken();

    /// <summary>
    /// Cancels the current eviction token, immediately evicting every cached popular-articles
    /// entry, and prepares a fresh token for the next cache fill.
    /// </summary>
    void Invalidate();
}
```

---

## 2. Invalidator implementation

**File:** `src/Modules/Content/Content/Infrastructure/Cache/PopularArticlesCacheInvalidator.cs`

```csharp
using _116.Content.Application.Shared.Cache;

namespace _116.Content.Infrastructure.Cache;

/// <summary>
/// Singleton implementation of <see cref="IPopularArticlesCacheInvalidator" /> that uses a
/// <see cref="CancellationTokenSource" /> as an eviction token for
/// <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache" /> entries.
/// </summary>
/// <remarks>
/// Cache entries created by
/// <see cref="Application.Editorial.UseCases.Public.Queries.GetPopularArticles.PublicGetPopularArticlesHandler" />
/// register the token returned by <see cref="GetEvictionToken" /> as an expiration token.
/// Calling <see cref="Invalidate" /> cancels the source, evicting all registered entries
/// immediately — regardless of the limit, category, or exclude-id used when those entries
/// were stored. A new <see cref="CancellationTokenSource" /> is then created so subsequent
/// cache fills are not affected. All mutations are protected by a lock so the class is safe
/// to use as a singleton.
/// </remarks>
public sealed class PopularArticlesCacheInvalidator : IPopularArticlesCacheInvalidator
{
    private readonly Lock _lock = new();
    private CancellationTokenSource _cts = new();

    /// <inheritdoc />
    public CancellationToken GetEvictionToken()
    {
        lock (_lock)
        {
            return _cts.Token;
        }
    }

    /// <inheritdoc />
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

---

## 3. DI registration

**File:** `src/Modules/Content/Content/ContentModule.cs`

Next to the existing tags-invalidator registration (`services.AddSingleton<IPopularTagsCacheInvalidator, PopularTagsCacheInvalidator>();`),
add:

```csharp
services.AddSingleton<IPopularArticlesCacheInvalidator, PopularArticlesCacheInvalidator>();
```

Singleton, because the token state must be shared across all requests (same lifetime as the
tags invalidator).

---

## 4. Wire `Invalidate()` into mutation handlers

In each handler below, inject `IPopularArticlesCacheInvalidator cacheInvalidator` and call
`cacheInvalidator.Invalidate();` **after** the commit — the exact pattern the tag handlers use
(`await unitOfWork.CommitAsync(cancellationToken); cacheInvalidator.Invalidate();`). Use the
handler's existing commit call site (`CommitAsync` / `unitOfWork.CommitAsync` — match whatever
that handler already calls).

### Engagement mutations

| Handler | Path (under `.../Application/Interactions/UseCases/Public/Commands`) |
|---------|----------------------------------------------------------------------|
| `PublicLikeArticleHandler` | `LikeArticle/PublicLikeArticleHandler.cs` |
| `PublicUnlikeArticleHandler` | `UnlikeArticle/PublicUnlikeArticleHandler.cs` |
| `PublicAddArticleCommentHandler` | `AddArticleComment/PublicAddArticleCommentHandler.cs` |
| `PublicDeleteArticleCommentHandler` | `DeleteArticleComment/PublicDeleteArticleCommentHandler.cs` |
| `PublicShareArticleHandler` | `ShareArticle/PublicShareArticleHandler.cs` |
| `PublicBookmarkArticleHandler` | `BookmarkArticle/PublicBookmarkArticleHandler.cs` |
| `PublicUnbookmarkArticleHandler` | `UnbookmarkArticle/PublicUnbookmarkArticleHandler.cs` |

### Membership mutations (published set changes)

| Handler | Effect |
|---------|--------|
| `AdminPublishArticleHandler` | article enters the ranked set |
| `AdminArchiveArticleHandler` | published article leaves the ranked set |
| Any unpublish / hard-delete of a published article | leaves the ranked set |

If an admin edit path can move a **published** article between categories, invalidate there
too (category-scoped entries would otherwise show it in the wrong category until TTL).

Example diff shape (like handler):

```csharp
public class PublicLikeArticleHandler(
    IArticleRepository articleRepository,
    IContentUnitOfWork unitOfWork,
    IPopularArticlesCacheInvalidator cacheInvalidator   // <-- add
    /* existing deps */
) : ICommandHandler<PublicLikeArticleCommand /*, ...*/>
{
    public async Task<...> Handle(PublicLikeArticleCommand command, CancellationToken cancellationToken)
    {
        // ... load article, article.IncrementLikeCount(), update ...

        await unitOfWork.CommitAsync(cancellationToken);
        cacheInvalidator.Invalidate();   // <-- add, after commit

        // ... return result ...
    }
}
```

---

## Tasks

- [x] Create `IPopularArticlesCacheInvalidator.cs`
- [x] Create `PopularArticlesCacheInvalidator.cs` (lock + `CancellationTokenSource`)
- [x] Register singleton in `ContentModule.cs`
- [x] Inject + call `Invalidate()` after commit in the 7 engagement handlers
- [x] Inject + call `Invalidate()` after commit in publish/archive — no unpublish/delete path
      touches published articles (`AdminDeleteArticleHandler` only deletes Draft/Rejected)
- [x] No category-change path exists for published articles (`AdminUpdateArticleHandler`
      rejects Approved/Published/Archived), so no further invalidation site is needed
