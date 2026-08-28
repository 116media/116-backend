# 04 — Content Infrastructure & Data

Scope: `src/Modules/Content/Content/Infrastructure` (repositories, `ContentDbContext`, 55
EF configurations, background jobs, cache), the specification pattern, and `Shared`
interceptors.

The good news up front: there is **no client-side query evaluation anywhere** —
`ToExpression().Compile()` appears once (on a single materialized entity), `AsEnumerable()`
zero times, and every specification (including correlated-subquery ones) translates to SQL.
The problems are lost-update races on engagement counters, cartesian-product includes,
column-rewriting updates, a missing soft-delete filter, missing indexes, and a data layer
that assumes a single app instance.

---

## 4.1 Engagement counters are lost-update read-modify-write races with zero concurrency control

**Severity: Critical**

**Where:** no entity carries a concurrency token (`grep RowVersion|IsConcurrencyToken|xmin`
→ 0). 21 in-memory counter mutators (`ArticleEntity.cs:616` `IncrementLikeCount() =>
LikeCount++`). The engagement handlers load → mutate → save
(`ArticleEngagementHandler.cs:35-79`) and run **post-commit in a detached DI scope**, so
they are genuinely concurrent across requests.

**Problem/why.** Two users liking the same article both read `LikeCount = 100`, both write
`101`. The `article_likes` rows are correct (unique index); the displayed counter is
permanently short. On a short-video feed at scale (`ShortVideoEntity.ViewCount`, view events
at scroll rate) the counter under-counts by roughly the concurrency factor, with no
reconciliation. `DecrementLikeCount` clamps at 0, so an unlike racing a like can permanently
zero a real count.

**Solution.** Do not add `RowVersion` (retry loops on a hot counter are worse). Replace the
read-modify-write with a single atomic statement — the pattern already exists at
`ShortVideoRepository.cs:362`:

```csharp
await context.Articles.Where(a => a.Id == id)
    .ExecuteUpdateAsync(s => s.SetProperty(a => a.LikeCount, a => a.LikeCount + delta), ct);
```

Add `ApplyEngagementDeltaAsync` to the four content repositories, rewrite the five
engagement handlers to call it (dropping the `GetByIdAsync` + `Update` + `CommitAsync`
triple), clamp with `GREATEST(0, …)` in SQL. This also fixes §4.15 as a side effect. Ship
Article first, verify counter monotonicity under load, then the rest.

---

## 4.2 Every "like" request runs the heaviest article query twice, and each run is a cartesian product

**Severity: Critical**

**Where:** `ArticleRepository.GetByIdAsync` (`:54`) joins two collections (`Images`, `Tags`)
in one query with no `AsSplitQuery` — and `AsSplitQuery` appears 4 times in the codebase,
all in Identity, zero in Content. `PublicLikeArticleHandler.cs:27` calls the 5-include
`GetByIdOrThrowAsync` purely as an existence check, discarding the result; the post-commit
engagement handler then runs the same query again. `ContentOrderRepository.cs:62-75` is
worse — a collection inside a collection.

**Problem/why.** Rows × `article_images` × `article_tags`: an article with 12 images and 8
tags returns 96 duplicated rows, each carrying the full `body` TEXT column, materialized then
de-duplicated by EF. The like path pays this twice per request.

**Solution.** Add `Task<bool> ExistsAsync(Guid id, CancellationToken)` (`AnyAsync`) and use
it in the six interaction handlers instead of the 5-include load. Append `.AsSplitQuery()` to
the 3 `ArticleRepository`, 3 `VideoRepository`, and `ContentOrderRepository` multi-collection
methods (a one-line safe change — ship first). Delete the redundant `GetByIdAsync` in the
engagement handler as part of §4.1.

---

## 4.3 Repository `Update()` rewrites every column of every row, including article bodies

**Severity: High**

**Where:** 19 `DbSet.Update(entity)` implementations, 91 call sites. Every read path already
returns tracked entities (only 7 `AsNoTracking` in all of Content — §4.5).

**Problem/why.** `DbSet.Update` marks *every* scalar `Modified`. A single like issues `UPDATE
articles SET title=…, body=…, … WHERE id=…` — rewriting the whole `body` TEXT column and
re-indexing `ix_articles_slug/title/status` on every engagement event. It also makes
concurrent writes destructive: an editor saving a headline while a visitor likes the article
has one write clobber all columns of the other.

**Solution.** Since reads already track, `Update()` is unnecessary — change the 19
implementations to attach only when detached (`if (Entry(e).State == Detached)
Attach(e).State = Modified`). Better long-term: delete the `Update` methods and rely on the
unit of work. Apply to the hot four (Article/Video/Lyrics/ShortVideo) first; verify UPDATE
statements shrink via EF logging.

---

## 4.4 No global query filter for soft delete — `IsDeleted` is filtered by hand, and several paths forget

**Severity: High**

**Where:** `grep HasQueryFilter` → 0. Soft-deletable: `ArticleCommentEntity`, `FileEntity`,
`RoleEntity`, `PermissionEntity`. 104 hand-written `IsDeleted` references. Two confirmed
omissions: `ArticleCommentByArticleIdSpecification` (`ArticleRepository.cs:205`) has no
`!IsDeleted` term, so `GetCommentsAsync`'s `totalCount` counts tombstones;
`GetCommentByIdAsync` (both overloads) omits it, so `PublicEditArticleCommentHandler` lets a
user edit a tombstoned comment.

**Problem/why.** `GET .../comments` returns a `totalCount` including deleted comments, so the
pagination footer disagrees with `articles.comment_count` (decremented on delete) — "24
comments" with 19 visible and a phantom last page. Every new query against a soft-deletable
table is one omission from leaking deleted data.

**Solution.** Add `HasQueryFilter(c => !c.IsDeleted)` in `ContentDbContext`/`CoreDbContext`
for the four types. Add `.IgnoreQueryFilters()` to the deliberate tombstone-rendering path
(`GetCommentsAsync` needs tombstones for threading — it renders `Body = null`), and *not* to
`GetCommentByIdAsync`. Remove redundant manual predicates once the filter exists.

---

## 4.5 Read paths track by default — only 7 of ~154 Content query terminations use `AsNoTracking`

**Severity: High**

**Where:** ~159 query terminations in Content repositories, 7 use `AsNoTracking` (all in
ShortVideo/Video). Public feed reads (`GetAllAsync`, `GetBySlugAsync`, `GetPopularArticlesAsync`,
comment/reply/bookmark/shared reads, the Lookup reads) all track. The popular-articles query
builders return tracked queries feeding a 10-minute cache.

**Problem/why.** Every public feed request builds change-tracking snapshots for every row —
a full second copy of every scalar including `articles.body` — as pure garbage. Tracked
entities are also live in the shared per-request `ContentDbContext`: an accidental
`CommitAsync` later in the scope writes back mapper mutations (the `MarkRemoved()` calls show
entities are mutated freely).

**Solution.** Set the default once, in `BaseModule.ConfigureDbContextOptions`:
`.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)`. Then add `.AsTracking()` to the
~25 methods whose results are mutated and committed (every method reached from a handler that
calls `repository.Update`). Flip the default in a branch, run the integration suite — writes
fail loudly where `AsTracking` is missing, which is exactly how you find them.

---

## 4.6 Missing indexes for the hottest ordering and cleanup predicates

**Severity: High**

**Where:** cross-referencing `Where`/`OrderBy` columns against the 61 `HasIndex`
declarations. The promoted-articles homepage query filters `is_promoted AND status
='Published'` and orders by `published_at DESC` with only a `status` index. `VideoRepository
.GetAllAsync` has no usable index at all. `GetLatestPublishedByCategoryAsync` orders by
`COALESCE(published_at, created_at)` — unindexable as written. The short-video view-event
cleanup filters `NOT is_counted AND created_at < cutoff` against a `(short_video_id,
dedup_key, created_at)` index (wrong leading column). `GetTagByNameAsync` does
`Name.ToLower() == name.ToLower()` — non-sargable, seq scan, called in a loop.

**Problem/why.** The promoted homepage bitmap-scans `articles` on `status` (~70% Published,
barely selective) then full-sorts by `published_at`, on every uncached request. The nightly
view-event cleanup seq-scans the fastest-growing table. `GetTagByNameAsync` is N sequential
scans per tag-update request.

**Solution.** One migration `AddContentReadIndexes` adding composite/filtered indexes:
`(Status, PublishedAt desc)`, `(CategoryId, Status, PublishedAt)`, a filtered
`is_promoted = true` index, `(Status, PublishedAt)` on videos, a `is_counted = false`
filtered index on view events. Rewrite `OrderBy(v => v.PublishedAt ?? v.CreatedAt)` to
`ThenBy` so the composite serves it; rewrite `GetTagByNameAsync` to `EF.Functions.ILike`
with a `LOWER(name)` expression index (or fold names on write like `ArtistEntity.NameFolded`).
Generate the migration with `CREATE INDEX CONCURRENTLY` — plain `CREATE INDEX` on
`articles`/`videos` at startup locks writes for the build.

---

## 4.7 Multi-step writes are not atomic — 14 files commit more than once, no explicit transaction in Content or Core

**Severity: High** · shared with [03 §1/§4](03-content-domain.md), [06 §7](06-content-application.md), [02 §5](02-module-boundaries.md).

**Where:** `ContentUnitOfWork`/`CoreUnitOfWork` are bare `SaveChangesAsync` passthroughs with
no `BeginTransaction`. `grep BeginTransaction src` → 5 hits, none in Content/Core. 14 files
call `CommitAsync` more than once. `FileRepository` self-commits 7 times.

**Problem/why.** Each extra commit is a partial-failure window with no rollback.
`AdminUploadArticleImageHandler` commits the `files` row then the `article_images` row
separately — a crash between leaves a Cloudinary asset and a `files` row referenced by
nothing. `AdminSetExclusiveCategoryHandler` clears the old exclusive category then sets the
new one in two commits; a failure between leaves the site with **zero** exclusive categories.
`OrderPaidEffectsHandler` on a 3-item order can leave the customer paid-for-three,
got-two.

**Solution.**
1. Collapse the trivially-mergeable double commits first (8 handlers — pure deletion of the
   first commit, since the entities are all tracked by one context).
2. Add `ExecuteInTransactionAsync` (over `CreateExecutionStrategy`) to the UoW for the
   handlers that raise no domain events (`AdminSetExclusiveCategoryHandler`,
   `AdminUploadArticleImageHandler`).
3. For handlers that raise domain events (`OrderPaidEffectsHandler`), the real fix is the
   domain-event outbox from [01 §1.7](01-composition-root-and-shared-kernel.md) — Mailer's
   `OutboxEmailDispatcherJob` is the pattern to port.

---

## 4.8 Cache is in-process and background jobs have no distributed lock — the app cannot run more than one instance

**Severity: High**

**Where:** `Program.cs:51` `AddMemoryCache`; `grep IDistributedCache|AddStackExchangeRedis`
→ 0. `CacheInvalidator` invalidates via a process-local `CancellationTokenSource`, registered
singleton. Quartz is configured with no persistent store and no clustering (default
RAMJobStore); `[DisallowConcurrentExecution]` is scheduler-local only.

**Problem/why.** Deploy a second pod and the system is immediately incorrect. (a) A like on
pod A invalidates pod A's cache token; pod B serves a stale popular list for 10 minutes, so
the same user refreshing gets different counts per pod. (b) Both pods' schedulers fire
`AbandonedDraftCleanupJob` at the same instant, both delete the same drafts, both dispatch
the deletion event → the Cloudinary asset is purged twice. (c) Both run
`ShortVideoViewEventCleanupJob`, doubling the scan on the largest table.

**Solution.** Cache: `AddStackExchangeRedisCache` + `IDistributedCache`; replace the
token-based mass-eviction with a Redis version key (`INCR popular_articles_version` folded
into the cache key). Jobs: `q.UsePersistentStore(...).UseClustering()` — Quartz's row-level
cluster lock makes `[DisallowConcurrentExecution]` cluster-wide. Minimum viable alternative:
a Postgres advisory lock at the top of each job's `Execute`. Do the job fix first — job
duplication corrupts data; cache staleness only annoys.

---

## 4.9 Seeding is unguarded at startup, and the `ContentTypeSeeder` short-circuit permanently withholds the `Lyrics` content type

**Severity: High** · overlaps [01 §1.12/§1.13](01-composition-root-and-shared-kernel.md).

**Where:** `ContentTypeSeeder.cs:39` — `bool alreadySeeded = await
context.ContentTypes.AnyAsync(); if (alreadySeeded) return;`. No migration inserts content
types (`grep InsertData` → 0). Lyrics landed in a July-2026 migration, long after the initial
seed. Seeding runs at startup, blocked (`GetAwaiter().GetResult()`), no lock, rethrows on any
exception.

**Problem/why.** Two bugs. (1) Any environment seeded before Lyrics is missing the `Lyrics`
content-type row forever — the feature is silently unavailable and fails as a
`NotFoundException` on content-type lookup, not a clear diagnostic. (2) On a fresh
multi-instance deploy, two pods both see `AnyAsync == false`, both insert, the unique index
rejects the loser → that pod crashes on startup and CrashLoopBackOffs.

**Solution.** Make the seeder idempotent per-row (diff `ContentTypeNames` against existing,
seed only the missing) — apply the same to `VisitorRoleSeeder`/`SuperAdminSeeder`. Wrap
seeding in a Postgres advisory lock so one instance seeds. Move seeding out of the request
pipeline into an `IHostedService`.

---

## 4.10 Migrations apply automatically at startup, including 60+ destructive `DropColumn`/`DropTable` operations

**Severity: High** · overlaps [01 §1.12](01-composition-root-and-shared-kernel.md).

**Where:** 42 migrations; `UseMigration` runs `MigrateAsync().GetAwaiter().GetResult()`,
gated only on `!Testing` (so Production migrates on every boot). 22 of 30 Content migrations
contain destructive DDL in `Up()`. No `CREATE INDEX CONCURRENTLY` anywhere.

**Problem/why.** A rolling deploy runs new-version pods beside old ones. The new pod boots and
runs `DropColumn("meta_keywords", "lyrics")`; the old pods still serving traffic 500 on every
`SELECT` of that column. An index-building migration on `articles` takes a write-blocking lock
during startup, before ready. A failed migration takes the pod down (a schema problem becomes
an outage).

**Solution.** Split migration from startup — add a `--migrate` CLI switch, set
`EnableMigrations = false` for non-Development, run migration as a pre-deploy job. Adopt
expand/contract for destructive changes (a column drop ships one release after reads stop).
Hand-edit index migrations to `CREATE INDEX CONCURRENTLY` with `suppressTransaction: true`.

---

## 4.11 Repositories are per-table CRUD wrappers, not per-aggregate — and `FileRepository` performs external HTTP I/O

**Severity: Medium**

**Where:** 22 repositories; `ArticleRepository` is 765 lines / 46 methods / 7 tables;
`LookupRepository` is 4 unrelated aggregates in one class. `FileRepository` takes `IFileService`
(Cloudinary) and `IImageColorService` and does upload-then-save with no compensation. Return
types are inconsistent (entities, projections, tuples, dictionaries, sets). Query builders in
`Application/*/Builders/` take `ContentDbContext` and return `IQueryable` — the dependency runs
the wrong way.

**Problem/why.** `FileRepository.UploadAndStoreImageFileAsync` uploads to Cloudinary *then*
saves; if the save throws, the asset is orphaned with no reconciliation. Because the repository
owns its own `SaveChangesAsync`, callers can't compose the file write with their domain write
(§4.7). `ArticleRepository` at 46 methods is the change-amplifier: the like path reused the
5-include load because the right narrow method didn't exist and adding one to a 765-line class
is unappealing.

**Solution.** Split `ArticleRepository` along aggregate lines (`IArticleRepository`,
`IArticleInteractionRepository`, `IArticleCommentRepository` — all share one context, so the
UoW still commits them together). Split `LookupRepository` into four. Move the upload
orchestration out of `FileRepository` into a `FileUploadService`, DB-row-first with
compensation on failure; delete `IFileRepository.SaveChangesAsync`. Move the query builders
into `Infrastructure/Queries/` so Application stops referencing `ContentDbContext`.

---

## 4.12 Specifications are predicate-only — every include, sort, and page is re-hand-written per call site

**Severity: Medium** · see also [06 §14](06-content-application.md).

**Where:** `ISpecification<T>` is one expression plus an in-memory evaluator — no `Includes`,
`OrderBy`, or paging. `ArticleRepository` repeats the same 5-`Include` block three times and
has 9 hand-rolled `CountAsync` + `OrderBy…Skip…Take` blocks; 29 across the Content repositories.
(Confirmed clean: no client-side evaluation anywhere.)

**Problem/why.** An omission surface, not a correctness bug. Ordering lives at the call site,
so "newest article" is `created_at` in one method, `published_at` in two others — nobody can
reconcile it, and the missing indexes (§4.6) exist because no single place declares the sort.
Includes at the call site mean `GetBySlugAsync` omits `Customer` that `GetByIdAsync` has, so
public-by-slug and admin-by-id responses differ in a way no type expresses. 29 hand-copied
paging blocks are 29 off-by-one chances.

**Solution.** Extend `ISpecification<T>` with `Includes`, `OrderBy`, `AsSplitQuery`,
`AsNoTracking` (defaults on the base so all 21 spec files compile unchanged); extend
`ApplySpecification` to apply them; add a `ToPagedListAsync` helper that owns the `CountAsync`
+ `Skip/Take` pair once. Migrate the 29 blocks starting with `ArticleRepository`.

---

## 4.13 Two N+1 loops on live read paths, and two more on admin writes

**Severity: Medium** · overlaps [06 §4](06-content-application.md), [02 §8](02-module-boundaries.md).

**Where:** `PublicGetArtistsHandler.cs:37-48` does one `fileRepository.GetByIdAsync` per
artist row — despite a batched `GetStorageUrlsByIdsAsync` existing.
`PublicGetVideoFeedHandler.cs:55` does one query per pinned category (each the unindexed
`COALESCE` sort). `AdminUpdateArticleTagsHandler`/`AdminUpdateVideoTagsHandler` do one
non-sargable `GetTagByNameAsync` per tag.

**Problem/why.** `GET .../artists?pageSize=50` issues up to 50 sequential round-trips to
`core.files` on top of the directory query — ~100ms of pure latency, growing with page size.

**Solution.** `PublicGetArtistsHandler`: collect the avatar ids and call
`GetStorageUrlsByIdsAsync` once (the shape `PublicGetVideoFeedHandler` already uses). Add
`GetTagsByNamesAsync` (single `Where(Contains)`) and rewrite the two tag handlers.
`PublicGetVideoFeedHandler`: one windowed query, or accept the loop once §4.6's index lands.

---

## 4.14 `DbContextPool` used with no resilience, no command timeout, no pool-size cap

**Severity: Medium**

**Where:** `BaseModule.cs` uses `AddDbContextPool` for all four modules; `UseNpgsql` has no
`EnableRetryOnFailure`, no `CommandTimeout`, no `Maximum Pool Size` in the connection string
(`grep EnableRetryOnFailure|CommandTimeout` → 0).

**Problem/why.** No retry means a single transient Postgres blip — a managed-DB failover, a
`57P01` during maintenance — surfaces as a 500. No `CommandTimeout` means the 30s Npgsql
default applies to every statement including the unindexed queries of §4.6; a slow scan holds
a pooled connection 30s and with enough of them the pool exhausts and the app stalls. No
`Maximum Pool Size` means N pods × 100 connections can exceed `max_connections`.

**Solution.** In `ConfigureDbContextOptions`: `EnableRetryOnFailure(3, 5s)` and
`CommandTimeout(15)`. **Sequencing:** `EnableRetryOnFailure` forbids user transactions outside
`strategy.ExecuteAsync`, which breaks `SuperAdminSeeder` and `OutboxEmailDispatcherJob` — wrap
those in the same change, and build §4.7's `ExecuteInTransactionAsync` on the execution
strategy from the start. Add `Maximum Pool Size`/`Timeout` to the connection string, sized as
`max_connections / expected_pods`.

---

## 4.15 Post-commit engagement handlers overwrite the editorial audit trail on content rows

**Severity: Medium** · fixed as a side effect of §4.1.

**Where:** `AuditableEntityInterceptor` stamps `UpdatedBy`/`UpdatedAt` on every `Modified`
entity from `ICurrentActor`, and `IHttpContextAccessor` (AsyncLocal) flows into the fresh
post-commit scope. `ArticleEngagementHandler` marks the whole article `Modified` (§4.3) and
commits inside that flowed context.

**Problem/why.** A visitor liking an article sets `articles.updated_by = <visitor id>` and
`updated_at = now`. The editorial audit trail — who last edited this article — is destroyed by
every like, bookmark, comment, and share. On a popular article the "last modified by" column
always shows whichever visitor most recently tapped the heart. (The interceptor itself is
correct — background jobs correctly get `System`, one timestamp per save. The problem is what
the engagement handlers ask it to stamp.)

**Solution.** §4.1 fixes this: `ExecuteUpdateAsync` bypasses the change tracker, so the
interceptor never sees the counter update. Same work, both bugs.

---

## 4.16 `CountAsync` + page query on every list endpoint, and several genuinely unbounded reads

**Severity: Medium**

**Where:** 29 `CountAsync` + `Skip` pairs as two round-trips. Unbounded reads with no `Take`:
`GetPromotedAsync`, `GetActivePromotedBySpotAsync`, `GetAbandonedDraftsAsync` (with all images
loaded at once), and the sharp one — `GetCommentedArticlesAsync` loads **every comment the
user ever wrote** on the paged articles just to take `.First()` per group.

**Problem/why.** `GetCommentedArticlesAsync` is O(user's total comment history) on a paginated
endpoint — a user with 500 comments materializes all 500 rows (each with `body`) to pick 20.
`GetAbandonedDraftsAsync` loads every abandoned draft + images into one list. `GetPromotedAsync`
returning every promoted article turns a pricing mistake into a 5,000-row homepage response.

**Solution.** Rewrite `GetCommentedArticlesAsync` to project the latest comment id per article
in SQL, then fetch ≤ pageSize comments. Add explicit `Take` caps to the feed methods.
Batch-load abandoned drafts. Strip includes before `CountAsync`
(`ContentOrderRepository.cs:105`). Cache `GetAvailableLettersAsync`.

---

## What is done well here

- **No client-side evaluation anywhere** — `ToExpression().Compile()` once (on a materialized
  entity), `AsEnumerable()` zero times. Every specification translates to SQL; all paging is
  `Skip/Take` before `ToListAsync`.
- **`DispatchDomainEventsInterceptor` is unusually careful** — events collected at
  `SavingChanges`, buffered per-context, discarded on failure/cancel, dispatched post-save in
  a fresh scope with `CancellationToken.None`, every handler failure caught and logged. The
  transaction caveat is documented honestly.
- **`AuditableEntityInterceptor` captures one timestamp per save**, distinguishes `Anonymous`
  from `System`, handles owned-entity changes.
- **Aggregate-projection queries avoid N+1 in the hard places** —
  `ArtistRepository.GetPublicDirectoryAsync` computes a four-source count as correlated
  subqueries in one statement; batched reads use early returns on empty input.
- **The short-video feed uses keyset paging, not offset** — ordered on `FeedRank ^ seed` with
  a unique rank, a strict total order that never drifts across pages, and correctly
  `AsNoTracking`.
- **Partial and filtered indexes are used deliberately** — unique-singleton category flags,
  `user_id IS NOT NULL` on artists, unique filename among active files, covering share indexes
  with matching sort direction.
- **`ExecuteDeleteAsync` is used correctly** for the bulk view-event prune.
- **The Mailer module already has a working outbox** — the pattern Content needs for §4.7
  exists in-repo to port rather than invent.
