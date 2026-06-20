# 02 — EF Configuration

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/CategoryConfiguration.cs`

## New Property Configuration

Add after the `IsExclusive` block:

```csharp
builder.Property(x => x.PinnedToFeedAt);

builder.HasIndex(x => x.PinnedToFeedAt).HasFilter("pinned_to_feed_at IS NOT NULL");
```

- `PinnedToFeedAt` maps to a nullable `timestamptz` column (`pinned_to_feed_at`) via the project's snake_case naming convention. No explicit `HasColumnName` is needed — the convention produces `pinned_to_feed_at`.
- The index is a **partial (filtered)** index over only the pinned rows. It is **not unique** — unlike `is_gossip_fallback` and `is_exclusive` (which are mutexes), the feed allows up to 5 pinned categories per content type. The cap is enforced at the application layer (see [04-admin-pin-category.md](04-admin-pin-category.md)); the index only accelerates the "list pinned categories" query.

## What is NOT configured

- `IsPinnedToFeed` is a `[NotMapped]` computed property — EF ignores it entirely. Do **not** add a property config or index for it.
- No database-level cap enforcement. PostgreSQL cannot express "at most 5 rows per content type where `pinned_to_feed_at is not null`" with a simple constraint, so the FIFO cap lives in the handler. This matches how the codebase already handles multi-row business rules.

## Existing Pattern Reference

For comparison, the existing single-row mutex flags use **unique** filtered indexes:

```csharp
builder.HasIndex(x => x.IsGossip).IsUnique().HasFilter("is_gossip_fallback = true");
builder.HasIndex(x => x.IsExclusive).IsUnique().HasFilter("is_exclusive = true");
```

`PinnedToFeedAt`'s index intentionally drops `.IsUnique()` because the feed is a capped set, not a mutex.
