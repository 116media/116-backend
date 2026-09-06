# Stage 9 — Query performance

Closes **[04 §2]** (Critical), **[04 §4]** (High), **[04 §5]** (High), **[04 §6]** (High) and
**[04 §13]** / **[06 §4]** (Medium). Five findings that all describe the same thing from different
angles: the read paths do far more work than the data they return justifies.

This is the widest stage so far. Unlike Stage 8, where one change fixed everything, these five are
independent, and **two of them can corrupt behaviour silently if shipped carelessly**. The order in
Part A is not cosmetic — it runs cheapest-and-safest first so that the two dangerous changes land
against a suite that is already green for other reasons.

> **Depends on Stage 8** (merge order). [9.2](#92-existsasync-on-the-interaction-paths) deletes the
> aggregate loads that Stage 8's handlers stopped needing, and [9.6](#96-the-no-tracking-default)
> touches the same repositories.
>
> **Behavioural change is deliberate in one place**: a tombstoned comment stops being a valid
> target for the six comment-by-id endpoints ([9.4](#94-global-soft-delete-filters)) — edit,
> admin/public delete, like, unlike, and add-reply all 404 on it now. The threaded listing (and
> its `totalCount`) deliberately keeps tombstones. Everything else must be invisible.

---

## The finding that changes the plan

The audit proposes flipping the global tracking default and says writes will "fail loudly where
`AsTracking` is missing, which is exactly how you find them." **That is not true in this codebase,
and following it as written would ship silent data loss.**

`DbContext.Update(entity)` attaches an untracked entity and marks it modified, and `Remove()` and
`Add()` behave the same way — so the ~130 handlers that call one of them keep working under
`NoTracking`. But **19 handlers mutate a loaded entity and call `CommitAsync()` with no persisting
call at all**, relying purely on the change tracker:

```csharp
// AdminActivateCategoryHandler — representative of all 19
CategoryEntity category = await categoryRepository.GetByIdOrThrowAsync(id, cancellationToken);

bool activated = category.Activate();
if (!activated)
{
    throw i18n.Category.AlreadyActive();
}

await unitOfWork.CommitAsync(cancellationToken);   // <- the only thing that persists Activate()
```

Under a `NoTracking` default this becomes a no-op. No exception, no failed assertion: `SaveChanges`
finds nothing dirty and returns `0`. The handler then re-reads the row and returns **HTTP 200 with a
DTO showing the unchanged state**. An integration test that asserts the response DTO would pass.

The full list, all Admin state transitions on taxonomy and commerce reference data:

| Aggregate | Handlers |
| --- | --- |
| Category | `ActivateCategory`, `DeactivateCategory`, `UpdateCategory`, `SetExclusiveCategory`, `PinCategoryToFeed`, `UnpinCategoryFromFeed` |
| ContentType | `ActivateContentType`, `DeactivateContentType`, `UpdateContentType` |
| PricingTier | `ActivatePricingTier`, `DeactivatePricingTier`, `UpdatePricingTier` |
| PromotionLevel | `ActivatePromotionLevel`, `DeactivatePromotionLevel`, `UpdatePromotionLevel` |
| Package | `ActivatePackage`, `DeactivatePackage` |
| Tag / Customer | `UpdateTag`, `UpdateCustomer` |

So the tracking flip is gated: **make those 19 writes explicit first** ([9.5](#95-make-the-implicit-writes-explicit)),
and only then change the default ([9.6](#96-the-no-tracking-default)).

> The soft-delete handlers (`DeleteArticle`, `DeleteVideo`, `DeleteShortVideo`, `DeleteTag`) look
> identical — `entity.MarkDeleted()` then commit — but each calls `repository.Remove(...)`, which
> attaches. They are safe, and they are the reason the detection rule below has to recognise
> `Remove`/`Add` as persisting calls rather than looking for `Update` alone.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Tracking default | flip globally and let tests find the gaps, or annotate reads one by one | **Flip globally — but only after 9.5.** Per-query `AsNoTracking` on ~150 terminations is the same edit count with no end state: the next new query still tracks by default. The flip is correct; the audit's *sequencing* is not, because 19 writes fail silently rather than loudly. |
| D2 | How to find the tracking-dependent writes | run the suite and look for red, or find them statically | **Statically.** "Run it and see" cannot find a silent no-op. The rule is mechanical: a handler that mutates a loaded entity and never calls a persisting repository method depends on the tracker. It returns exactly 19 files today and is re-run in Verification as a regression guard. |
| D3 | Existence checks | keep `GetByIdOrThrowAsync`, or an existence guard | **`ExistsOrThrowAsync`.** 18 interaction handlers `await` a 5-include aggregate load and discard the result; the guard is an `AnyAsync` index probe that throws the same `NotFoundException` the load threw. Handlers never construct exceptions inline — the throw site (and therefore the localized wire format) stays behind the repository's `*OrThrowAsync` convention, exactly like `GetByIdOrThrowAsync`. |
| D4 | Soft delete | global query filter, or keep the 104 hand-written predicates | **Global filter** on the 4 soft-deletable types. A hand-written predicate is correct only until someone writes the 105th query; two are already wrong today. |
| D5 | The comment-threading path | drop tombstones, or keep rendering them | **Keep them** — `GetCommentsAsync` needs tombstones so replies keep their parent, and renders `Body = null`. It gets `IgnoreQueryFilters()`; `GetCommentByIdAsync` deliberately does **not**, which is what stops editing a deleted comment. |
| D6 | Index creation | plain `CREATE INDEX` in a migration, or `CONCURRENTLY` | **`CONCURRENTLY`**, hand-written into the migration with `suppressTransaction: true`. A plain build on `articles`/`videos` takes an `ACCESS EXCLUSIVE` lock; with §4.10 still open, migrations run at startup, so that lock lands during a deploy. |
| D7 | `GetTagByNameAsync` | `EF.Functions.ILike`, or a folded column | **`ILike` + a `LOWER(name)` expression index.** A folded column is the better long-term shape (`ArtistEntity.NameFolded` already does it) but needs a migration, a backfill and a write-path change on every tag writer — out of scope here. |
| D8 | The N+1 mappers | new batch methods, or re-point the existing signatures | **Re-point the existing `ToXxxDtosAsync` signatures** at a `BuildDtosAsync` ported from `ShortVideoMapper`. The 21 call sites then need no edit, which keeps this the smallest diff that removes the loop. |
| D9 | Scope of one PR | one PR, or split the risky half | **One PR, strict internal order, two gates.** The five findings share the same repositories and the same test surface; splitting doubles the review of the same files. If review pressure demands a split, cut between 9.4 and 9.5 — everything before is additive, everything after changes write semantics. |

---

## Checklist

- [ ] 9.1 — `.AsSplitQuery()` on the multi-collection reads
- [ ] 9.2 — `ExistsAsync` on the 4 content repositories; 18 interaction handlers stop loading aggregates
- [ ] 9.3 — `AddContentReadIndexes` migration (`CONCURRENTLY`) + the two query rewrites
- [ ] 9.4 — Global soft-delete filters + `IgnoreQueryFilters` on the tombstone path
- [ ] 9.5 — **GATE** — make the 19 implicit writes explicit; detection script returns 0
- [ ] 9.6 — **GATE** — flip the tracking default; `AsTracking()` where results are mutated
- [ ] 9.7 — Batch the N+1 file lookups in the 6 mappers and `PublicGetArtistsHandler`
- [ ] 9.8 — `GetTagsByNamesAsync` + rewrite the two tag handlers
- [ ] 9.9 — Tests: the two behavioural changes, and a no-op-write regression test
- [ ] 9.10 — Verify (clean build 0/0, csharpier, unit green, integration green, static queries clean)
- [ ] 9.11 — Identity's miniature instances: the 2 discarded role loads + the bulk-permission N+1

---

## Part A — The mechanism

### 9.1 Split the multi-collection reads

`AsSplitQuery` appears 9 times in the codebase, all in Identity, **zero in Content**. Three methods
join two or more collections in one statement:

| Method | Collections joined |
| --- | --- |
| `ArticleRepository.GetByIdAsync` / `GetByIdOrThrowAsync` / `GetBySlugAsync` | `Images` × `Tags` |
| `ContentOrderRepository.GetByIdWithItemsAsync` | `Items` and a collection inside it |
| `PackageRepository.GetByIdWithSlotsAsync` / `…OrThrowAsync` | `Slots` and a collection inside it |

An article with 12 images and 8 tags returns 96 rows, each carrying the full `body` TEXT column,
materialized and de-duplicated in memory. Append one line:

```csharp
return await context
    .Articles.ApplySpecification(specification: specification)
    .Include(a => a.Category)
    .Include(a => a.Images)
    .Include(a => a.Tags)
        .ThenInclude(t => t.Tag)
    .Include(a => a.Customer)
    .Include(a => a.PromotionLevel)
    .AsSplitQuery()
    .FirstOrDefaultAsync(cancellationToken);
```

`VideoRepository`'s three equivalents include only **one** collection (`Tags`) next to three
references, so they produce no cartesian product and are left alone — the audit's "3 VideoRepository
methods" over-counts.

> Split queries are not free: N statements, and no single-snapshot guarantee across them. Both are
> acceptable for a read of one aggregate; do not apply this reflexively to single-collection reads.

### 9.2 `ExistsAsync` on the interaction paths

18 handlers in `Application/Interactions` do this:

```csharp
await articleRepository.GetByIdOrThrowAsync(id: command.ArticleId, cancellationToken: cancellationToken);
```

— the result is never assigned. It is an existence check paying for a 5-include load, on the
hottest public write paths (7 article, 6 short-video, 3 lyrics, 2 video).

Add to each of the four content repository interfaces:

```csharp
/// <summary>
/// Throws when no article row exists, without materializing the aggregate.
/// </summary>
/// <param name="articleId">The article identifier.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
Task ExistsOrThrowAsync(Guid articleId, CancellationToken cancellationToken = default);
```

```csharp
/// <inheritdoc />
public async Task ExistsOrThrowAsync(Guid articleId, CancellationToken cancellationToken = default)
{
    bool exists = await context.Articles.AnyAsync(a => a.Id == articleId, cancellationToken);

    if (!exists)
    {
        throw new NotFoundException(nameof(ArticleEntity), articleId);
    }
}
```

The wire format must not move. Today the discarded load throws from
`FirstDefaultOrThrowAsync` — a `NotFoundException`, and the endpoint tests pin it:

```csharp
await response.ShouldBeProblem<NotFoundException>(
    HttpStatusCode.NotFound,
    Localized<SharedExceptionMessage>(m => m.EntityNotFound("Article"))
);
```

The guard throws **the same `NotFoundException`, not an i18n rule exception** — a
`ContentRuleException` here would change the problem `Title` and the message, and every
missing-entity endpoint test would fail. Handlers stay one line and construct nothing:

```csharp
await articleRepository.ExistsOrThrowAsync(articleId: command.ArticleId, cancellationToken: cancellationToken);
```

The `NotFoundExceptionHandler` strategy localizes the response from `EntityName` through
`SharedExceptionMessage.EntityNotFound` per request culture — the throw site only has to carry
the right entity name, which `nameof(ArticleEntity)` does identically to the old load path.

### 9.3 Read indexes

`articles` carries only `(status)` — barely selective at ~70% `Published` — so the promoted
homepage bitmap-scans it and then full-sorts by `published_at` on every uncached request. `videos`
has `(artist_id, status)` and nothing serving its default listing. The short-video view-event
cleanup filters `NOT is_counted AND created_at < cutoff` against an index whose leading column is
`short_video_id`.

One migration, `AddContentReadIndexes`, hand-edited to `CONCURRENTLY`:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(
        """
        CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_status_published_at
            ON content.articles (status, published_at DESC);
        """,
        suppressTransaction: true
    );

    migrationBuilder.Sql(
        """
        CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_articles_promoted_published_at
            ON content.articles (published_at DESC)
            WHERE is_promoted = true;
        """,
        suppressTransaction: true
    );
    // …(category_id, status, published_at) on articles
    // …(status, published_at DESC) on videos
    // …filtered (created_at) WHERE is_counted = false on short_video_view_events
    // …LOWER(name) on tags
}
```

`suppressTransaction: true` is mandatory — `CREATE INDEX CONCURRENTLY` cannot run inside a
transaction block, and EF wraps migrations in one. Declare the same indexes in the entity
configurations so the model and the database agree; scaffold, then replace the generated
`CreateIndex` calls with the raw SQL above.

Two queries cannot use an index as written:

```csharp
// Unindexable: the expression is COALESCE(published_at, created_at).
.OrderByDescending(v => v.PublishedAt ?? v.CreatedAt)

// Serves from (status, published_at DESC), with created_at only breaking ties.
.OrderByDescending(v => v.PublishedAt)
.ThenByDescending(v => v.CreatedAt)
```

This is a **visible ordering change** for rows where `published_at IS NULL`: they now sort as
`NULL` (**first** under `DESC` in PostgreSQL — `NULLS FIRST` is the default) instead of falling
back to `created_at`. As built this is moot: `published_at` exists since the initial migration and
`Publish()` always stamps it, so no such row can exist without a manual edit. Confirm that is
acceptable for `GetLatestPublishedByCategoryAsync` and the two `VideoRepository` sites before
shipping — if it is not, the fallback needs a stored generated column instead.

```csharp
// LookupRepository:203 stays as written — Npgsql translates it to LOWER(name) = LOWER(@p),
// which the LOWER(name) expression index serves. ILike was considered and rejected: its
// pattern semantics turn a tag named "rock%" into a prefix match, silently changing equality.
t => t.Name.ToLower() == name.ToLower()
```

### 9.4 Global soft-delete filters

`grep HasQueryFilter` → **0**, against 4 soft-deletable types and 104 hand-written `IsDeleted`
predicates. Two are already wrong:

- `ArticleCommentByArticleIdSpecification` has no `!IsDeleted` term, so `GetCommentsAsync`'s
  `totalCount` counts tombstones — the footer says "24 comments" over 19 visible rows, with a
  phantom last page, and it disagrees with `articles.comment_count`.
- `GetCommentByIdAsync` (both overloads) omits it, so `PublicEditArticleCommentHandler` will edit a
  deleted comment.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema(ContentConstants.SchemaName);
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    modelBuilder.Entity<ArticleCommentEntity>().HasQueryFilter(c => !c.IsDeleted);

    base.OnModelCreating(modelBuilder);
}
```

`CoreDbContext` gets `FileEntity`, `IdentityDbContext` gets `RoleEntity` and `PermissionEntity`.

Then exactly one opt-out, on the threading path that renders tombstones as `Body = null`:

```csharp
IQueryable<ArticleCommentEntity> query = context
    .ArticleComments.IgnoreQueryFilters()
    .ApplySpecification(specification: specification);
```

> A query filter applies to `Include`d navigations too, so a deleted comment silently vanishes from
> any parent's reply collection — that is the intent here, but it is the part most likely to
> surprise. Remove the now-redundant manual `!IsDeleted` predicates only after the suite is green,
> as a separate commit, so a regression bisects cleanly.

### 9.5 Make the implicit writes explicit

**Gate.** Before the default changes, the 19 handlers from the table above stop depending on the
tracker:

```csharp
bool activated = category.Activate();
if (!activated)
{
    throw i18n.Category.AlreadyActive();
}

categoryRepository.Update(category: category);          // <- added
await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
```

This is correct on its own merits and is a no-op behaviourally while tracking is still on, so it can
land and be verified before anything else moves.

The detection cannot be a plain `grep -L`: the persisting call is named `Update`, `Remove`, `Add`,
`Replace…`, `Set…` or `Save…` depending on the repository, and a pattern loose enough to catch them
all also matches `GetImagesByArticleIdAsync`. The gate is the script below — save it as
`scripts/find-tracking-writes.py`; it passes when it prints `0`.

```python
import re, subprocess

files = [f for f in subprocess.run(
    ["grep", "-rl", "CommitAsync", "src/Modules/Content/Content/Application"],
    capture_output=True, text=True).stdout.split() if f.endswith("Handler.cs")]

LOADED = re.compile(r"^\s*(?:var|[A-Z]\w*(?:Entity)?)\s+(\w+)\s*=\s*await\s+\w*[Rr]epository\.(Get\w+)", re.M)
PERSIST = re.compile(r"[Rr]epository\.(Update|Replace|Set|Save|Remove|Add|Delete)\w*\(|ExecuteUpdate|ExecuteDelete")

offenders = []
for path in files:
    source = open(path).read()
    mutated = any(
        not call.startswith(("Get", "To", "Has", "Is", "Can", "Try"))
        for var in {m.group(1) for m in LOADED.finditer(source)}
        for call in re.findall(rf"\b{var}\.([A-Z]\w*)\s*\(", source)
    )
    if mutated and not PERSIST.search(source):
        offenders.append(path)

print(len(offenders))
for path in offenders:
    print("  ", path)
```

It reports **19** on the tree this stage starts from, and must report **0** before 9.6 begins.

### 9.6 The no-tracking default

**Gate: 9.5 is green.** Only 7 of ~154 Content query terminations use `AsNoTracking`, all in
Video/ShortVideo. Every public feed request builds a change-tracking snapshot — a second copy of
every scalar, including `articles.body` — as pure garbage.

```csharp
private static void ConfigureDbContextOptions(
    IServiceProvider serviceProvider,
    DbContextOptionsBuilder options,
    string connectionString
)
{
    options.AddInterceptors(serviceProvider.GetServices<ISaveChangesInterceptor>());

    options
        .UseNpgsql(connectionString)
        .UseSnakeCaseNamingConvention()
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
}
```

Then `.AsTracking()` on the repository methods whose results are mutated and committed — every
method reached from a handler that calls `Update`, plus the aggregate loads behind the 19 from 9.5.
Work from the write side: for each handler that commits, follow its `Get*` call and annotate that
method. Delete the 7 now-redundant `AsNoTracking()` calls in the same pass.

> `Update()` covers most of this, which is why the suite alone is not a sufficient signal. Trust
> 9.5's static check, not a green run.

---

## Part B — The N+1 loops

### 9.7 Batch the file lookups

`ShortVideoMapper.BuildDtosAsync` already does this correctly: collect ids into a set, one
`GetByIdsAsync`, map in memory — two round-trips regardless of page size. `CategoryMapper`,
`PlaylistMapper` and `VideoMapper` batch as well; `AlbumMapper`, `ArticleMapper`, `ArtistMapper` and
`LyricsMapper` still do one `fileRepository.GetByIdAsync` per entity, and `VideoMapper`'s N+1
overload is still used at 8 sites while its batch overload is used at 2.

`GET /admin/articles?pageSize=100` is one article query plus 100 sequential file round-trips, and
with §6.2 (client-controlled `pageSize`) still open that factor is unbounded. Port `BuildDtosAsync`
into the remaining mappers, re-point the existing `ToXxxDtosAsync` signatures at it — the 21 call
sites need no edit — then delete the single-entity-in-a-loop overloads.

`PublicGetArtistsHandler` loops `fileRepository.GetByIdAsync` per row despite
`GetStorageUrlsByIdsAsync` existing:

```csharp
IReadOnlyDictionary<Guid, string> avatarUrls = await fileRepository.GetStorageUrlsByIdsAsync(
    ids: rows.Where(r => r.AvatarFileId.HasValue).Select(r => r.AvatarFileId!.Value).ToHashSet(),
    cancellationToken: cancellationToken
);
```

### 9.8 Batch the tag lookups

`AdminUpdateArticleTagsHandler` and `AdminUpdateVideoTagsHandler` call the non-sargable
`GetTagByNameAsync` once per tag — N sequential sequential scans per request. Add
`GetTagsByNamesAsync` (one `Where(… Contains …)`, `ILike`-folded per 9.3) and rewrite both handlers
to a single call.

### 9.11 Identity's miniature instances

The same census run against Identity (see the as-built notes) found the patterns exist there
only in miniature — nothing on a hot path, but two are worth closing while the idiom is fresh:

- `AdminRemovePermissionFromRoleHandler` and `AdminBulkUpdateRolePermissionsHandler` both
  `await` a role load purely as an existence check — the bulk handler loads the role **with its
  permissions collection** and discards it. Same fix as 9.2: `ExistsByIdAsync` beside the
  existing `ExistsByNameAsync`, handler throws the same `NotFoundException(nameof(RoleEntity), …)`
  the discarded load threw.
- The bulk handler's removal loop is an N+1: one `GetByRoleAndPermissionAsync` per removed
  permission. One `GetByRoleAndPermissionIdsAsync` (a single `Contains` query) feeds the same
  `Delete` calls.

Identity's tracking default is untouched (0 tracker-dependent writes, but the flip buys nothing
on single-row lookups); `BulkUpdateRolePermissions` stays tracked.

---

## Tests

- **Unit** — `ExistsAsync`, `GetTagsByNamesAsync` and the mapper batch methods get handler-level
  tests with mocked repositories. The 19 handlers from 9.5 get an assertion that `Update` was
  called; that is the unit-level guard against the tracking regression.
- **Integration — the deliberate behavioural change** (fails before this stage):
  - A tombstoned comment cannot be edited: soft-delete it, `PUT` it, assert `404`
    (`EditArticleComment_OnASoftDeletedComment_ReturnsNotFound`, proven failing on `develop` with
    `200 OK`). The listing keeps tombstones — no `totalCount` test exists because that behaviour
    deliberately did not change (see the as-built notes).
- **Integration — the silent-write regression.** For a representative handler from each of the 19's
  aggregates (Category, Package, PromotionLevel, ContentType, PricingTier), assert the change is
  **persisted in the database**, not merely reflected in the response DTO. Several of those handlers
  re-read the row after committing, so a response-only assertion passes against a no-op write. This
  is the test that would have caught the audit's mistake.
- **Integration — no-op proofs.** `AsSplitQuery`, `ExistsAsync`, the indexes and the mapper batching
  must not move a single response body. The existing endpoint suites are the assertion; any diff
  there is a bug in this stage, not a test to update.

> `AsSplitQuery` and the query filters both change what a single `Include` returns. Run the full
> integration suite after 9.1 and again after 9.4 rather than once at the end — those two are the
> ones that fail far from where they were introduced.

---

## Rollout

9.1, 9.2, 9.7 and 9.8 are safe to deploy on their own and can ship first if this stage gets split.

9.3 needs the migration applied out of band (`CONCURRENTLY`, and §4.10 still applies migrations at
startup — apply it manually before the deploy that carries it, or the concurrent build races the
startup migrator). Index creation on `articles`/`videos` is online but not free; run it off-peak.

9.6 is the one to watch after deploy: a missing `AsTracking()` shows up as a write that returns 200
and changes nothing. Watch the Admin state-transition endpoints (Category/Package/ContentType
activate-deactivate) for the first day, since those are the 19.

No API contract moves. The response changes are the six comment-by-id endpoints 404ing on
tombstones (including the delete pair losing idempotency); comment `totalCount` and the thread
listing are unchanged, and the `ThenBy` ordering change cannot surface on real data.

---

## As built (deviations from this spec)

Recorded during implementation on `perf-query-performance`:

- **D7 executed differently.** `EF.Functions.ILike` was written, then reverted: ILike's pattern
  semantics turn a tag named `rock%` into a prefix match. `GetTagByNameAsync` keeps its
  `ToLower() == ToLower()` equality — Npgsql translates it to `LOWER(name) = LOWER(@p)`, which
  the new `ix_tags_name_lower` expression index serves.
- **9.4 narrowed to two of the four types.** `ArticleCommentEntity` and `FileEntity` carry the
  global filter. `RoleEntity`/`PermissionEntity` deliberately do not: Identity's role surface is
  built around tri-state `IsDeleted` filtering (list-deleted, restore flows), so a global filter
  there would need `IgnoreQueryFilters()` on nearly every read — inverted cost. Their manual
  predicates are complete today; both audit-confirmed omissions were on the comment paths.
- **The comment `totalCount` "bug" was self-consistent behaviour.** `ArticleCommentDto` documents
  the tombstone contract (deleted comments render with `Body = null` for thread continuity), so
  `GetCommentsAsync` keeps returning tombstones (via `IgnoreQueryFilters()`) and its `totalCount`
  keeps counting them — list and footer agree. The real §4.4 fix is `GetCommentByIdAsync` going
  filtered: editing a tombstoned comment now 404s (regression-proven: 200 on `develop`).
- **9.6 flipped Content only, not `BaseModule` globally.** A global flip would silently break the
  Mailer outbox dispatcher (loads → mutates → saves with no attach call) and Identity's
  tracker-dependent writes. `ModuleOptions.UseNoTrackingByDefault` carries the flag —
  `OnConfiguring` is not an option because pooled contexts reject it. Other modules opt in after
  their own 9.5-style sweep.
- **The tracked set is 44 methods, not ~25**: the 35 handler-facing loads whose results are
  mutated, plus 9 repository-internal load-to-remove methods (`RemoveLikeAsync` and siblings) the
  handler-level census cannot see.
- **The migration gained two items**: the `LOWER(name)` expression index (no model
  representation), and a concurrent drop of `ix_articles_category_id`, which the new
  `(category_id, status, published_at)` composite makes redundant.
- **9.11 added after an Identity census.** Cartesian products: 0 (Identity already used
  `AsSplitQuery` everywhere it matters). Tracker-dependent writes: 0. Discarded loads: 2, both
  cold admin role commands — fixed. N+1: 1, the bounded bulk-permission loop — fixed.
- **The comment filter's surface is six endpoints, not one.** Every `GetCommentByIdAsync`
  consumer now 404s on a tombstoned comment: edit (the planned fix), admin delete, public delete,
  like-comment, unlike-comment, and add-reply. The last five are the same bug class (interacting
  with a deleted comment) fixed for free — but note the delete pair **stopped being idempotent**:
  deleting an already-deleted comment returned `200 IsSuccess:true` before and returns `404` now.
- **Case-collision edge guarded:** `GetTagsByNamesAsync` keys its result by lower-cased name;
  `tags.name` is unique case-*sensitively*, so a database already containing `"Rock"` and
  `"rock"` would have made a plain `ToDictionary` throw where the old per-name `FirstOrDefault`
  picked one arbitrarily. The method groups by folded name and keeps the first per group, matching
  the old behaviour on dirty data instead of crashing.
- **The first full integration run failed 28 tests and exposed three census gaps**, all closed:
  - *Argument-flow loads.* The mutation census caught `entity.Mutate()` but not entities **passed
    to** persisting calls (`RemoveSlot(slot)`, `RemoveTag(tag)`) — including overloads
    (`GetSlotByIdAsync(slotId, packageId)`) and cross-file factory flows (`payment` loaded in
    `OrderPaymentFactory`, mutated in `AdminVerifyPaymentFactory`). Six more methods tracked.
  - *Identity-map fixup dependencies.* `AddItemTierFactory` reads `item.Tiers` from a method with
    **no Include** — it only ever worked because the earlier tracked order load fixed the
    collection up. Untracked, `Tiers` was empty and the duplicate-tier conflict check passed
    silently. `GetItemByIdAsync` is tracked with a comment naming the dependency.
  - *Test contexts.* `CreateDbContext<T>` in both test bases now sets `TrackAll`: test
    arrangements (backdated timestamps, counter seeds, retroactive links) mutate loaded entities
    and would silently no-op under the module default — the exact 9.5 failure mode, in the tests.
    Assertions that read soft-deleted rows switched from `FindAsync` to
    `IgnoreQueryFilters().FirstOrDefaultAsync` (10 sites).
  Five census false positives (`Concat`/`Sum`/guard methods flagged as mutations) were reverted so
  the public feed queries stay untracked.
- **The silent-write integration tests already existed** — the admin endpoint suites assert
  persisted state through a fresh `DbContext` (`IsCategoryActiveAsync` etc.), so 9.9's planned
  regression tests were already in place; the 19 handlers' unit suites cover the rest.

---

## Verification

1. `dotnet build --no-incremental` — 0 warnings, 0 errors.
2. `dotnet csharpier check .`
3. `dotnet test tests/Unit` — green.
4. `dotnet test tests/Integration` — green (run locally).
5. The 9.5 gate returns nothing:
   ```bash
   grep -rl "CommitAsync" src/Modules/Content/Content/Application --include='*Handler.cs' \
     | xargs grep -L -E "\.(Update|Add|AddAsync|Remove|RemoveRange)\(|ExecuteUpdate|ExecuteDelete"
   ```
6. No interaction handler still loads an aggregate to discard it:
   ```bash
   grep -rn "^\s*await \w*[Rr]epository\.GetByIdOrThrowAsync" \
     src/Modules/Content/Content/Application/Interactions
   ```
7. The mappers no longer do per-entity file lookups:
   ```bash
   grep -rn "fileRepository.GetByIdAsync" src/Modules/Content/Content/Application/Shared/Mappers
   ```
8. Confirm the indexes are actually used — `EXPLAIN` the promoted-homepage query against a seeded
   database and check for an index scan, not `Seq Scan` + `Sort`. An index that the planner ignores
   is a migration that bought nothing.

---

**PR:** `perf(content): split queries, no-tracking reads, read indexes and batch lookups`
