# Stage 8 — Atomic engagement counters & audit-trail integrity

Closes **[04 §1]** (Critical) and **[04 §15]** (Medium). Five post-commit handlers maintain the
denormalized engagement counters by loading the row, mutating it in memory, and saving it. They run
in a detached DI scope after the interaction row is already durable, so they are genuinely
concurrent across requests — and the read-modify-write loses updates.

The same `SaveChanges` also drags the audit-trail bug behind it: marking the whole content row
`Modified` lets `AuditableEntityInterceptor` stamp `UpdatedBy`/`UpdatedAt` from the *visitor* who
tapped the heart, destroying the editorial "last edited by".

One change fixes both: replace the load-mutate-save with a single set-based statement.

> **No logical dependency on Stage 7** — counters involve no rule codes or coded exceptions. The
> branch still stacks on Stage 7 for merge order only: [8.6](#86-retiring-the-in-memory-mutators)
> edits the five entity files Stage 7 rewrote, so branching from `develop` instead would conflict.
>
> **Behavioural change is the point.** Counters stop under-counting; content rows stop being
> restamped by visitors. No API contract moves.

---

## The bug in one paragraph

`ArticleEngagementHandler` loads the article, calls `article.IncrementLikeCount()` — which is
`LikeCount++` — then `Update()` + `CommitAsync()`. Two users liking the same article both read
`LikeCount = 100` and both write `101`. The `article_likes` rows are correct (unique index guards
them), so the rows and the counter disagree permanently, with no reconciliation job. No entity
carries a concurrency token: `grep -r "RowVersion\|IsConcurrencyToken\|xmin" src/` returns nothing.
`DecrementLikeCount` clamps with `Math.Max(0, …)` in memory, so an unlike racing a like can drive a
real count to zero and hold it there.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Concurrency control | add `RowVersion` + retry, or a set-based UPDATE | **Set-based UPDATE.** A retry loop on the hottest row in the system converts contention into latency and still needs the same SQL to be correct. `UPDATE … SET c = c + 1` is atomic in one round trip with no token, no retry, no extra column, no migration. |
| D2 | Where the statement lives | in the handler via `DbContext`, or on the repository | **Repository.** Handlers are Application-layer and must not hold a `DbContext`; `ExecuteUpdateAsync` is EF-specific. Each content repository gains `ApplyEngagementDeltaAsync`. |
| D3 | Clamping | clamp in C# before the call, or in SQL | **In SQL.** Clamping in C# needs the current value, which is the read we are removing. `Math.Max(0, x + delta)` translates to `GREATEST(0, …)` on Npgsql — verified in 8.2 before the sweep, because the whole stage rests on it. |
| D4 | One method per counter or one with a selector | `IncrementLikeAsync`/`DecrementLikeAsync`/… (≈20 methods), or one method taking the kind and delta | **One method, kind + delta.** The events already carry `(Kind, Delta)`; 20 near-identical repository methods would restate the switch the handler already performs. |
| D5 | The counter mutators on the entities | keep them, or delete them | **Delete the 20.** Their only callers are the five handlers ([8.6](#86-retiring-the-in-memory-mutators)). Leaving `LikeCount++` on the entity leaves the lost-update pattern one keystroke away for the next contributor. |
| D6 | `VideoEngagementHandler`'s `Rating` arm | leave it, or convert it | **Convert it.** It is not a delta — it recomputes the average from all ratings and sets two columns — so it is not a §4.1 race. But it still marks the row `Modified`, so it is a §4.15 audit-trail overwrite. It becomes `SetRatingAsync` ([8.5](#85-the-rating-arm-is-not-a-delta)). |
| D7 | The "row disappeared" log | drop it, or keep it | **Keep it, but make it mean one thing.** A single `0` would conflate "the row is gone" with "this entity has no counter for that kind" — the latter is routine (an article receives `View` events it does not count) and would log on every scroll. `ApplyEngagementDeltaAsync` returns `int?`: `null` for "no counter here" (silent), `0` for "row gone" (logged), `n` for applied. |
| D8 | Negative `Share`/`View` deltas | keep the old positive-only guard, or apply whatever arrives | **Apply whatever arrives.** The old switch matched `(Share, > 0)` and `(View, > 0)`, silently dropping a negative. Nothing emits one — `Share` and `View` are raised with `Delta: 1` at all six call sites — so this changes no behaviour today, and the SQL `GREATEST(0, …)` floors it if one ever appears. Recorded because it is a real widening, not an oversight. |

---

## Checklist

- [ ] 8.1 — `EnumEngagementKind` → column mapping lives in one place per repository
- [ ] 8.2 — Prove `Math.Max` translates to `GREATEST` before relying on it
- [ ] 8.3 — `ApplyEngagementDeltaAsync` on the 4 content repositories + the comment repository
- [ ] 8.4 — Rewrite the 5 engagement handlers to call it; drop `GetByIdAsync` + `Update` + `CommitAsync`
- [ ] 8.5 — `SetRatingAsync` for the video rating arm
- [ ] 8.6 — Delete the 20 in-memory counter mutators and their unit tests
- [ ] 8.7 — Integration test: the audit trail survives a like (the §4.15 proof)
- [ ] 8.8 — Integration test: N concurrent likes leave the counter at N (the §4.1 proof)
- [ ] 8.9 — Unit tests: handlers assert the repository call, not the entity mutation
- [ ] 8.10 — Verify (clean build 0/0, csharpier, unit green; run integration locally)
- [ ] 8.11 — Integration test: every kind moves its own column on all five repositories

---

## Part A — The mechanism

### 8.1 The counter map

The audit says the pattern "already exists at `ShortVideoRepository.cs:362`". **It does not** —
there is no `ExecuteUpdateAsync` anywhere in Content. The real precedent is in Identity, added in
an earlier stage:

```csharp
// AccountLockoutRepository — "so two concurrent failures cannot both read the same count"
await context.Users.Where(u => u.Id == userId)
    .ExecuteUpdateAsync(setters => setters.SetProperty(u => u.FailedLoginAttempts, u => u.FailedLoginAttempts + 1), ct);
```

Stage 8 brings that shape to Content. Each repository owns the mapping from
`EnumEngagementKind` to its own columns, because the columns differ per entity — articles have
`CommentCount`, short videos have `ViewCount`, videos have only `ShareCount`.

### 8.2 Prove the clamp translates first

D3 rests on `Math.Max(0, x + delta)` reaching PostgreSQL as `GREATEST(0, …)`. If Npgsql cannot
translate it inside `ExecuteUpdateAsync`, the whole stage needs a different clamp — a
`CASE WHEN`, or a raw `ExecuteSqlInterpolatedAsync`. **Verify this on one column before writing the
other nineteen**, by asserting the generated SQL in an integration test against the real database:

```csharp
await context.Articles.Where(a => a.Id == id)
    .ExecuteUpdateAsync(s => s.SetProperty(a => a.LikeCount, a => Math.Max(0, a.LikeCount + delta)), ct);
```

If it throws `InvalidOperationException: could not be translated`, stop and revisit D3 before
proceeding. Everything downstream assumes this works.

### 8.3 The repository method

```csharp
/// <summary>
/// Applies a signed delta to one engagement counter in a single statement, clamped at zero.
/// Set-based by design: two concurrent likes cannot both read the same value and write the
/// same increment, and the change tracker never sees the row, so the audit columns are left
/// as the last editorial write set them.
/// </summary>
/// <returns>The number of rows updated — <c>0</c> when the article no longer exists.</returns>
public async Task<int> ApplyEngagementDeltaAsync(
    Guid articleId,
    EnumEngagementKind kind,
    int delta,
    CancellationToken cancellationToken = default
) =>
    kind switch
    {
        EnumEngagementKind.Like => await BumpAsync(articleId, a => a.LikeCount, delta, cancellationToken),
        EnumEngagementKind.Bookmark => await BumpAsync(articleId, a => a.BookmarkCount, delta, cancellationToken),
        EnumEngagementKind.Comment => await BumpAsync(articleId, a => a.CommentCount, delta, cancellationToken),
        EnumEngagementKind.Share => await BumpAsync(articleId, a => a.ShareCount, delta, cancellationToken),
        _ => 0,
    };
```

An unsupported kind returns `0` rather than throwing: the handler's existing `default:` arm already
treats an unknown kind as "invalidate the cache and move on", and a post-commit handler must not
fail a request that has already succeeded.

### 8.4 The handler

Before — a load, a switch of 7 arms, a whole-row `Update`, a commit. After:

```csharp
public async Task Handle(ArticleEngagedEvent domainEvent, CancellationToken cancellationToken = default)
{
    int updated = await articleRepository.ApplyEngagementDeltaAsync(
        articleId: domainEvent.ArticleId,
        kind: domainEvent.Kind,
        delta: domainEvent.Delta,
        cancellationToken: cancellationToken
    );

    if (updated == 0)
    {
        logger.LogDebug(
            "Engagement counter skipped for article {ArticleId}: the article no longer exists.",
            domainEvent.ArticleId
        );
    }

    cacheInvalidator.Invalidate();
}
```

`IContentUnitOfWork` leaves the constructor — `ExecuteUpdateAsync` writes immediately and there is
nothing left to commit. The cache is invalidated unconditionally, as it is today on every path
including `default:`.

### 8.5 The rating arm is not a delta

`VideoEngagementHandler` handles `Rating` by loading every rating row, averaging, and calling
`video.UpdateRating(average, count)` — two columns set to computed values, not a counter bump. It
is not a lost-update race, but it *is* an audit-trail overwrite, so it moves to its own set-based
method:

```csharp
Task<int> SetRatingAsync(Guid videoId, decimal average, int count, CancellationToken ct);
```

The average is still computed in the handler from `GetAllRatingsForVideoAsync`. Two concurrent
raters can still interleave — the last writer wins with a value computed from a consistent read —
which is the behaviour today and is acceptable for a displayed average.

`VideoEntity.UpdateRating` **stays**: `PublicRateVideoHandler` also calls it.

---

## Part B — Cleanup

### 8.6 Retiring the in-memory mutators

Twenty `Increment*`/`Decrement*` methods across `ArticleEntity`, `LyricsEntity`, `VideoEntity`,
`ShortVideoEntity` and `ArticleCommentEntity` exist **only** for these five handlers — verified by
tracing every call site, not by a filtered grep. Once the handlers stop calling them they are dead,
and dead in a dangerous way: `LikeCount++` on a domain entity is an invitation to reintroduce the
race. They go, with their unit tests.

The `Count` properties keep their private setters for EF materialization.

---

## Tests

- **Unit** — the five handler test files assert the repository call and the cache invalidation
  instead of the entity mutation. A handler test can no longer assert a counter value, because the
  handler no longer computes one; that assertion moves to integration where the database applies it.
- **Unit — the 24 mutator tests are deleted, not ported.** They named the deleted methods
  (`IncrementLikeCount_ShouldIncrement` and friends). Arranged through a test helper they would
  arrange, act and assert entirely inside the test project while covering no production line.
- **Integration — the two proofs, both of which fail on `main`:**
  - **§4.15:** seed an article with a known `UpdatedBy`, like it as a visitor over HTTP, assert
    `updated_by` and `updated_at` are **unchanged** while `like_count` rose. This is the regression
    that no existing test catches.
  - **§4.1:** fire N concurrent likes from N distinct users at one article and assert
    `like_count == N`. Drive it **over HTTP**, one client per visitor — calling the new repository
    method directly can only ever prove Postgres works, since the method does not exist on `main`.
    Each visitor must be a **seeded account with its own session**: a hand-minted token for an
    unseeded user has no matching token-state row and is rejected with 401 before the endpoint
    runs. Pick N high enough (≥ 20) that the old read-modify-write fails reliably.
  - Unlike-below-zero: like once, unlike twice, assert the counter is `0` and not negative — the
    SQL clamp, which is where the clamp now lives.
- **Integration — the column-mapping matrix (8.11).** Every kind on every one of the five
  repositories, asserting the intended counter moved **and its siblings did not**, plus the `null`
  and `0` returns from D7. Mocked handler tests cannot catch a kind wired to the wrong column, and
  three of the five repositories had no other integration coverage.

Counter arrangement lives in `tests/Fixtures/Helpers/EngagementCounterExtensions.cs` as
`WithLikeCount(n)`-style setters. They state the count a test needs rather than replaying N
increments to reach it, and they are arrangement only — no test asserts against them.

---

## Rollout

No migration, no configuration, no client coordination. Counters begin recording correctly from
deploy; **existing drift is not repaired** by this stage. If the current values matter, a one-off
reconciliation (`UPDATE … SET like_count = (SELECT count(*) FROM article_likes …)`) is a separate,
optional follow-up — every counter has a rows-of-truth table behind it.

Ship `ArticleEntity` first, confirm counter monotonicity under load, then the remaining four.

---

## Verification

1. `dotnet build --no-incremental` — 0 warnings, 0 errors.
2. `dotnet csharpier check .`
3. `dotnet test tests/Unit` — green.
4. `dotnet test tests/Integration` — green (run locally).
5. Confirm the read-modify-write is gone:
   `grep -rn "Count++\|Count--" src/Modules/Content/Content/Domain/` returns nothing.
6. Confirm no handler commits any more:
   `grep -rn "CommitAsync" src/Modules/Content/Content/Application/Interactions/EventHandlers/`
   returns nothing.

**PR:** `fix(content): make engagement counters atomic and stop clobbering the audit trail`
