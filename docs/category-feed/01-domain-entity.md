# 01 — Domain Entity Changes

**File:** `src/Modules/Content/Content/Domain/Entities/CategoryEntity.cs`

## New Property

Add after `IsExclusive`:

```csharp
/// <summary>
/// The moment this category was pinned to the content feed, or null if it is not pinned.
/// A non-null value means the category appears as a section in its content-type feed
/// (video feed, and later the article feed). The timestamp also serves as the FIFO
/// eviction key: when the per-content-type cap is exceeded, the category with the
/// oldest PinnedToFeedAt is unpinned first.
/// </summary>
public DateTimeOffset? PinnedToFeedAt { get; private set; }
```

## New Computed Property

`IsPinnedToFeed` is a read-only convenience view over `PinnedToFeedAt`. It is **not mapped** to a column — EF reads/writes `PinnedToFeedAt` only. Add the `System.ComponentModel.DataAnnotations.Schema` using if not already present.

```csharp
/// <summary>
/// Whether this category is currently pinned to the content feed.
/// Derived from <see cref="PinnedToFeedAt" /> — true when a pin timestamp is set.
/// </summary>
[NotMapped]
public bool IsPinnedToFeed => PinnedToFeedAt is not null;
```

> Specifications and EF `Where(...)` clauses must filter on `PinnedToFeedAt != null`, **not** on `IsPinnedToFeed` — a `[NotMapped]` property cannot be translated to SQL.

## Method Changes

`PinnedToFeedAt` is **not** set in `Create()` or `Update()` — pinning is an explicit curation action handled by its own use case (see [04-admin-pin-category.md](04-admin-pin-category.md)), exactly like `IsExclusive` is toggled via `SetExclusive()` rather than at create time.

### New method: `PinToFeed()`

```csharp
/// <summary>
/// Pins this category to the content feed, stamping the current time.
/// The handler is responsible for enforcing the per-content-type cap and unpinning
/// the oldest pinned category when the cap would be exceeded.
/// Re-pinning an already-pinned category refreshes its timestamp (moving it to the
/// front of the FIFO queue).
/// </summary>
public void PinToFeed()
{
    PinnedToFeedAt = DateTimeOffset.UtcNow;
}
```

### New method: `UnpinFromFeed()`

```csharp
/// <summary>
/// Removes this category from the content feed.
/// Called by the handler directly (unpin endpoint) or as part of FIFO eviction
/// when the per-content-type cap is exceeded.
/// </summary>
/// <returns>True if the category was pinned and is now removed, false if it was not pinned.</returns>
public bool UnpinFromFeed()
{
    if (PinnedToFeedAt is null)
    {
        return false;
    }

    PinnedToFeedAt = null;
    return true;
}
```

## Constants

The two tunable numbers for this feature live in constants, not magic numbers.

### Per-content-type cap

**File:** `src/Modules/Content/Content/Application/Catalog/Constants/CatalogFeedConstants.cs` (new)

```csharp
namespace _116.Content.Application.Catalog.Constants;

/// <summary>
/// Constants governing how categories are curated into the content feed.
/// </summary>
public static class CatalogFeedConstants
{
    /// <summary>
    /// Maximum number of categories that may be pinned to the feed at once, per content type.
    /// Pinning a category beyond this limit unpins the oldest pinned category of the same
    /// content type (FIFO).
    /// </summary>
    public const int MaxPinnedCategoriesPerContentType = 5;
}
```

### Feed video bounds

**File:** `src/Modules/Content/Content/Application/Editorial/Constants/EditorialFeedConstants.cs` (existing — add two constants)

```csharp
/// <summary>
/// Maximum number of latest published videos shown in a single feed section
/// (one pinned category) in the video feed.
/// </summary>
public const int MaxVideosPerFeedSection = 8;

/// <summary>
/// Minimum number of published videos a category must have before it can be
/// pinned to the video feed. A category below this threshold cannot be pinned,
/// so no section ever renders with too few videos to look intentional.
/// </summary>
public const int MinVideosToPinToFeed = 4;
```

> Invariant: `MinVideosToPinToFeed <= MaxVideosPerFeedSection`. The minimum is an
> **eligibility gate enforced at pin time** (see [04-admin-pin-category.md](04-admin-pin-category.md));
> the maximum is a **display cap applied when building the feed** (see
> [05-public-video-feed-query.md](05-public-video-feed-query.md)).
>
> These two constants are video-specific. The future article feed gets its own parallel
> pair (e.g. `MaxArticlesPerFeedSection` / `MinArticlesToPinToFeed`).

No change to `ContentConstants` — `PinnedToFeedAt` is a nullable timestamp (no length constraint).
