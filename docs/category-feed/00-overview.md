# Category Feed (Pinned Categories) — Overview

## Context

The homepage content feed is split into **sections**, where each section is a **category** and displays the **latest published content** belonging to it. The video feed shows up to **8 latest videos** per section; an article feed (planned) will reuse the same mechanism.

A super admin curates which categories appear in the feed by **pinning** them. The set of pinned categories is **capped** — at most **5 categories per content type** may be pinned at once. When a 6th category is pinned, the **oldest pinned category is automatically unpinned** (FIFO eviction), so the feed never grows beyond the cap without manual cleanup.

### Why "pinned to feed" and not "featured"

The obvious word, *featured*, is already taken: `is_featured` / `featured_until` were the original promotion columns on `videos` and `articles`, since **renamed to `IsPromoted` / `PromotedUntil`**. Reusing "featured" for a category-level concept would collide with that history and read ambiguously next to `IsPromoted` (paid promotion) and `IsExclusive` (the single hero show). **Pinning** captures the actual intent precisely: an admin deliberately places a category into the feed, and there is a limited number of slots.

### What "pinned" means on the frontend

The video feed (`GET /api/v1/public/videos/feed`) returns an **ordered list of sections**. Each section carries:

- the **category** metadata (name, slug, optional poster), and
- up to **8 latest published videos** in that category.

The new frontend renders one block per section — a section header plus a row/grid of video cards. Empty sections (a pinned category with zero published videos) are **omitted** from the response so the UI never renders a blank block.

A later `GET /api/v1/public/articles/feed` will reuse the exact same `PinnedToFeedAt` flag on article categories — no new entity work is needed for it.

## The new field

This feature adds **one** field to `CategoryEntity`:

| Field            | Type              | Description                                                                                                                                                  |
|------------------|-------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `PinnedToFeedAt` | `DateTimeOffset?` | When the category was pinned to the feed. `null` means it is **not** pinned. The timestamp doubles as the **FIFO eviction key** (oldest is unpinned first).  |

A computed, non-mapped convenience property exposes the boolean view:

```csharp
[NotMapped]
public bool IsPinnedToFeed => PinnedToFeedAt is not null;
```

### Why a timestamp instead of a plain `bool`

The original idea was a boolean `ShowInTheFeed`. A boolean alone cannot answer **"which pinned category is the oldest?"**, which is exactly what the FIFO eviction rule requires. Storing `PinnedToFeedAt`:

- encodes presence (`PinnedToFeedAt is not null` ⇔ in the feed),
- gives a deterministic eviction order (unpin `MIN(PinnedToFeedAt)`),
- needs no second column to stay in sync.

The DTO still exposes a clean `IsPinnedToFeed` boolean for clients that only care about the flag.

## How it differs from existing category / content flags

| Flag             | Lives on  | Cardinality                        | Meaning                                                            |
|------------------|-----------|------------------------------------|-------------------------------------------------------------------|
| `IsExclusive`    | category  | exactly **one** (mutex)            | The single hero show featured after the promotion feed.           |
| `IsGossip`       | category  | exactly **one** (mutex)            | The article category used as homepage fallback / gossip strip.    |
| `IsPinnedToFeed` | category  | up to **5 per type** (capped FIFO) | Categories surfaced as sections in the content feed.              |
| `IsPromoted`     | video/article | per-item, time-boxed           | Paid promotion (formerly named `is_featured`).                    |

## Decisions baked into this spec

These are the non-obvious choices made here. Each is called out where it appears and is cheap to change before implementation:

1. **Cap is per content type** (5 video categories *and* 5 article categories), not 5 globally. This is what makes the future article feed work cleanly.
2. **Eviction is automatic FIFO** — pinning a 6th category silently unpins the oldest (mirrors the `IsExclusive` "unset previous" pattern, but with capacity 5 instead of 1). No error is thrown.
3. **Only the `Video` content type can be pinned today** (the only feed that exists). The guard rejects `Article` / `Short` / `Custom` with `ContentTypeNotFeedable`. `Article` becomes eligible when the article feed lands — the `PinnedToFeedAt` field and per-content-type cap already support it.
4. **Section order** = `PinnedToFeedAt` **descending** (most recently pinned first). Swap to ascending, or introduce an explicit `FeedPosition` column, if manual ordering is ever needed.
5. **Empty sections are omitted** from the feed response.
6. **Videos per section** = up to 8 latest **published**, ordered by `PublishedAt` desc with `CreatedAt` as the tie-breaker/fallback. Constant: `MaxVideosPerFeedSection = 8`.
7. **Eligibility gate** — a category can only be pinned if it has at least **4 published videos** (so no section ever looks empty/thin). Enforced at pin time. Constant: `MinVideosToPinToFeed = 4`. Both constants live in `EditorialFeedConstants`; the future article feed gets its own parallel pair.

## Scope

| Doc | Contents |
| --- | -------- |
| [01-domain-entity.md](01-domain-entity.md) | `PinnedToFeedAt` field, `IsPinnedToFeed` computed property, `PinToFeed()` / `UnpinFromFeed()` methods, constants |
| [02-ef-configuration.md](02-ef-configuration.md) | EF property config and index |
| [03-repository-and-specification.md](03-repository-and-specification.md) | `PinnedToFeedCategorySpecification`, repository methods (list pinned, latest videos by category) |
| [04-admin-pin-category.md](04-admin-pin-category.md) | Admin `pin-to-feed` / `unpin-from-feed` endpoints + FIFO eviction handler |
| [05-public-video-feed-query.md](05-public-video-feed-query.md) | Public `GET /videos/feed` query, grouped result DTOs, endpoint |
| [06-dto-and-mapper.md](06-dto-and-mapper.md) | `CategoryDto` additions and feed section DTO |
| [07-error-messages.md](07-error-messages.md) | New error keys and i18n resources |
| [08-ef-migration.md](08-ef-migration.md) | Migration command and expected schema changes |
| [09-tests.md](09-tests.md) | Test plan across entity, handler, query, mapper, endpoint, and specification |
| [10-file-inventory.md](10-file-inventory.md) | Complete list of files to create or modify |
