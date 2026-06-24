# 03 — Repository and Specification Additions

## PinnedToFeedCategorySpecification

**File:** `src/Modules/Content/Content/Application/Catalog/Specifications/CategorySpecifications.cs`

Add after `ExclusiveCategorySpecification`:

```csharp
/// <summary>
/// Specification that matches active categories currently pinned to the content feed,
/// optionally narrowed to a single content type.
/// Note: filters on PinnedToFeedAt (the mapped column), not the [NotMapped] IsPinnedToFeed property.
/// </summary>
public class PinnedToFeedCategorySpecification(Guid? contentTypeId = null) : Specification<CategoryEntity>
{
    /// <inheritdoc />
    public override Expression<Func<CategoryEntity, bool>> ToExpression()
    {
        return category =>
            category.PinnedToFeedAt != null
            && category.IsActive
            && (contentTypeId == null || category.ContentTypeId == contentTypeId);
    }
}
```

## ICategoryRepository

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/ICategoryRepository.cs`

Add one method, following the `GetExclusiveCategoryAsync` pattern:

```csharp
/// <summary>
/// Retrieves all active categories currently pinned to the content feed, optionally
/// filtered to a single content type, ordered by PinnedToFeedAt descending (most recently
/// pinned first). Used by the public feed query and by the admin pin handler to enforce
/// the per-content-type cap and pick the FIFO eviction victim.
/// </summary>
/// <param name="contentTypeId">Optional content type filter, or null for all pinned categories.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>A read-only list of pinned category entities, newest first.</returns>
Task<IReadOnlyList<CategoryEntity>> GetPinnedToFeedCategoriesAsync(
    Guid? contentTypeId = null,
    CancellationToken cancellationToken = default
);
```

## CategoryRepository Implementation

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Repositories/CategoryRepository.cs`

```csharp
public async Task<IReadOnlyList<CategoryEntity>> GetPinnedToFeedCategoriesAsync(
    Guid? contentTypeId = null,
    CancellationToken cancellationToken = default
)
{
    var spec = new PinnedToFeedCategorySpecification(contentTypeId: contentTypeId);

    return await _context.Categories
        .Include(c => c.ContentType)
        .ApplySpecification(spec)
        .OrderByDescending(c => c.PinnedToFeedAt)
        .ToListAsync(cancellationToken);
}
```

> Match the exact `Include`/`ApplySpecification` style already used by the surrounding methods in `CategoryRepository` (e.g. `GetExclusiveCategoryAsync`). The handler that enforces the cap re-sorts ascending in memory to find the oldest — see [04](04-admin-pin-category.md).

## IVideoRepository — latest published by category

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/IVideoRepository.cs`

The feed needs the **latest N published videos for a single category**. The existing `GetAllAsync` orders by `CreatedAt` and is pagination-shaped; add a purpose-built method that orders by publish recency.

```csharp
/// <summary>
/// Retrieves the latest published videos for a single category, newest first.
/// Ordered by PublishedAt descending, falling back to CreatedAt for rows with a
/// null PublishedAt. Used to populate a category section in the content feed.
/// </summary>
/// <param name="categoryId">The category to fetch videos for.</param>
/// <param name="limit">The maximum number of videos to return.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>A read-only list of published video entities, newest first.</returns>
Task<IReadOnlyList<VideoEntity>> GetLatestPublishedByCategoryAsync(
    Guid categoryId,
    int limit,
    CancellationToken cancellationToken = default
);
```

### VideoRepository Implementation

**File:** `src/Modules/Content/Content/Infrastructure/Repositories/VideoRepository.cs`

```csharp
public async Task<IReadOnlyList<VideoEntity>> GetLatestPublishedByCategoryAsync(
    Guid categoryId,
    int limit,
    CancellationToken cancellationToken = default
)
{
    var spec = new VideoByStatusSpecification(EnumContentStatus.Published)
        .And(new VideoByCategorySpecification(categoryId));

    return await context.Videos
        .Include(v => v.Category)
        .ApplySpecification(spec)
        .OrderByDescending(v => v.PublishedAt ?? v.CreatedAt)
        .Take(limit)
        .ToListAsync(cancellationToken);
}
```

> `VideoByStatusSpecification` and `VideoByCategorySpecification` already exist in `VideoSpecifications.cs`; `.And(...)` is provided by the `Specification<T>` base. Confirm the spec-composition helper name matches the one used elsewhere in `VideoRepository` (the codebase also uses a `VideoQueryBuilder` for `GetAllAsync` — either composition style is acceptable here as long as it matches a nearby example).

## IVideoRepository — count published by category

The pin handler must check a category has at least `MinVideosToPinToFeed` published
videos before allowing it into the feed. Add a lightweight count method.

**File:** `src/Modules/Content/Content/Application/Shared/Repositories/IVideoRepository.cs`

```csharp
/// <summary>
/// Counts the published videos belonging to a single category.
/// Used by the pin handler to enforce the minimum-videos eligibility gate.
/// </summary>
/// <param name="categoryId">The category to count videos for.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>The number of published videos in the category.</returns>
Task<int> CountPublishedByCategoryAsync(
    Guid categoryId,
    CancellationToken cancellationToken = default
);
```

### VideoRepository Implementation (count)

**File:** `src/Modules/Content/Content/Infrastructure/Repositories/VideoRepository.cs`

```csharp
public async Task<int> CountPublishedByCategoryAsync(
    Guid categoryId,
    CancellationToken cancellationToken = default
)
{
    var spec = new VideoByStatusSpecification(EnumContentStatus.Published)
        .And(new VideoByCategorySpecification(categoryId));

    return await context.Videos
        .ApplySpecification(spec)
        .CountAsync(cancellationToken);
}
```

> No `Include` needed — this is a pure count. The pin handler compares the result against
> `EditorialFeedConstants.MinVideosToPinToFeed`.
