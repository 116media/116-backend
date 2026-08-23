# Stage 15 — Domain model hardening

Closes **[03 §2]** (High), **[03 §7]** (High), **[03 §8]**, **[03 §9]**, **[03 §10]**,
**[03 §11]**, **[03 §12]**, **[03 §13]**, **[04 §3]**, **[04 §11]**, **[04 §12]** / **[06 §14]**,
**[06 §16]**. The findings behind "everything is an aggregate root, no structure" that the
restructure stage does not own — the *semantic* model, fixed before the files move.

Verified in the current tree:

- **53 entity-typed navigation properties** in the Content domain (the audit's 64 predates
  Stages 6–8), many crossing aggregate boundaries: `ArticleEntity.Customer`
  (`CustomerEntity?`), `ArticleEntity.PromotionLevel`, `LyricsEntity.Video`, and their
  siblings on every content aggregate.
- `LyricsRevisionEntity.Accept(Guid?)` has **no status guard**: calling it twice re-sets the
  decision and raises `LyricsRevisionDecidedEvent` again — re-runnable and re-notifying
  `[03 §7]`. `Reject` is the same shape.
- `ArticleRepository.Update` is `context.Articles.Update(article)` — the whole row goes
  `Modified`, so every commit rewrites every column including `body` `[04 §3]`.

> Draft — finalized against the tree Stage 14 lands on. Deliberately **before** Stage 18's
> restructure: semantic changes hide badly inside a whole-repo file move.

---

## Decisions

| # | Question | Options weighed | Decision |
| --- | --- | --- | --- |
| D1 | Cross-aggregate navigations | keep object references, or IDs at the boundary | **IDs at the boundary, navigations within.** `Article → Images/Tags` stays (one aggregate); `Article → Customer/PromotionLevel` becomes id-only — the FK columns already exist, so **no migration**: the change is deleting the navigation, its `Include`, and re-pointing the mapper at a repository lookup the handler already batches (Stage 9's shapes). Done per aggregate, Commerce first (fewest navigations), Content last. |
| D2 | Strongly-typed IDs | wrap the 105 Guids, or reject with reasons | **Reject, recorded here.** The cost (every signature, EF converter, DTO, test builder — thousands of lines) buys compile-time protection against cross-assigning ids, but the top sources of that bug class — cross-aggregate navs (D1) and check-then-act guards (Stage 6) — are closed by cheaper means. Revisit only if an id-swap bug actually ships. `[03 §8]` closes as *decided-against*. |
| D3 | Review guards | ad-hoc ifs, or the Stage 6 pattern | **Stage 6's pattern:** idempotent transitions return `bool`, invalid transitions throw coded rule exceptions, events raised only on actual transition. |
| D4 | `Update()` semantics | keep attach-Update everywhere, or tracked mutation | **Tracked mutation on load-then-mutate paths.** Post-Stage 9 the write paths are explicitly `AsTracking`; a loaded entity's `SaveChanges` diffs columns. `Update()` (attach) remains only where the entity genuinely arrives detached — after this stage that is nowhere in Content, and the method is deleted per repository as its callers convert. |
| D5 | Specifications | make them carry include/sort/page, or retire the layer | **Retire.** 136 single-use predicate-only types whose includes/sorts are re-hand-written per call site is ceremony without leverage `[04 §12]` / `[06 §14]`; the query-builder pattern already in Application is the honest shape. Specs inline into their single call sites; the base class stays in Shared for Identity until Stage 18. |
| D6 | `[06 §16]` unused `IMapper` | sweep now | **Verify first — likely already closed.** Stage 7's CS9113 cleanup removed unread primary-ctor params and the build holds at 0 warnings, which would flag an injected-never-read `mapper`. Re-census at finalization; sweep only what remains. |

---

## Checklist

- [ ] 15.1 — Navigation census commit (the 53, classified within/across aggregate)
- [ ] 15.2 — Cross-aggregate navs → id-only: Commerce, Lookup, then Editorial aggregates
- [ ] 15.3 — Review-workflow guards on `LyricsRevisionEntity`, `LyricsSubmissionEntity`, `LyricsTranslationRevisionEntity`
- [ ] 15.4 — The three missing state-change events + `LyricsDeletedEvent` `[03 §10]`
- [ ] 15.5 — `HasLyrics` derived; the three maintaining handlers stop writing it `[03 §11]`
- [ ] 15.6 — Load-then-mutate paths drop `Update()`; attach-Update deleted per repository `[04 §3]`
- [ ] 15.7 — Content specifications inlined; `ApplySpecification` call sites collapse `[04 §12]`
- [ ] 15.8 — D2/D6 verified and recorded
- [ ] 15.9 — Verify (build 0/0, csharpier, unit, integration; builder reflection hacks gone `[03 §9]`)

---

## Part A — Aggregate boundaries

Representative — `ArticleEntity.Customer`. Today:

```csharp
/// <summary>
/// The customer who commissioned this article, or null for free content.
/// </summary>
public CustomerEntity? Customer { get; private set; }
```

The FK (`CustomerId`) already exists beside it. The navigation, its EF configuration line, and
every `.Include(a => a.Customer)` are deleted; the two mappers that read
`article.Customer.Name` take the name from a batched customer lookup instead:

```csharp
// In the admin detail handler (the only consumer of CustomerName):
CustomerEntity? customer = article.CustomerId.HasValue
    ? await customerRepository.GetByIdAsync(article.CustomerId.Value, cancellationToken)
    : null;
```

Each removed navigation is also a removed `Include` — reconcile with Stage 9's split-query list
so no cartesian fix gets re-litigated. Where a navigation is *within* the aggregate
(`Article.Images`, `Article.Tags`, `Lyrics` on its own children), it stays.

`[03 §9]` falls out here: the test builders' reflection hacks exist to satisfy navigations that
no longer exist; delete them with the navs.

## Part B — Review-workflow guards

`LyricsRevisionEntity.Accept` today sets `Status = Accepted` and raises the event
unconditionally. After (Stage 6/7 house shape):

```csharp
/// <summary>
/// Accepts this revision. Idempotent: a revision already accepted reports false and raises
/// nothing; a revision already rejected cannot be flipped.
/// </summary>
/// <param name="decidedByUserId">The moderator, or null when auto-accepted by vote threshold.</param>
/// <returns><c>true</c> if the revision transitioned; <c>false</c> if already accepted.</returns>
public bool Accept(Guid? decidedByUserId)
{
    if (Status == EnumRevisionStatus.Accepted)
    {
        return false;
    }

    if (Status == EnumRevisionStatus.Rejected)
    {
        throw new ContentRuleException(ContentRuleCodes.RevisionAlreadyDecided);
    }

    Status = EnumRevisionStatus.Accepted;
    DecidedByUserId = decidedByUserId;

    AddDomainEvent(
        new LyricsRevisionDecidedEvent(
            RevisionId: Id,
            LyricsId: LyricsId,
            ProposedByUserId: ProposedByUserId,
            Accepted: true,
            ByModerator: decidedByUserId.HasValue
        )
    );

    return true;
}
```

`Reject` mirrors it; `AdminDecideLyricsRevisionHandler` / `AdminDecideTranslationRevisionHandler`
translate the `false` into the existing `AlreadyDecided` problem instead of silently
re-notifying. `RevisionAlreadyDecided` joins `ContentRuleCodes` + the revision catalog map.

## Part C — Derived state and write semantics

**`HasLyrics` `[03 §11]`** — today `AdminCreateLyricsHandler`, `AdminUpdateLyricsHandler` and
`AdminDeleteLyricsHandler` all remember to flip `video.MarkHasLyrics()` /
`UnmarkHasLyrics()`. The flag's readers move to the lyrics table:

```csharp
// VideoRepository — the one place the fact is computed from its source of truth.
public Task<bool> HasPublishedLyricsAsync(Guid videoId, CancellationToken cancellationToken = default)
{
    return context.Lyrics.AnyAsync(
        l => l.VideoId == videoId && l.Status == EnumContentStatus.Published,
        cancellationToken
    );
}
```

The column, the two mutators, and the three maintenance sites are deleted (column dropped in
this stage's migration — generated, unapplied). If the feed queries need it hot, a filtered
`EXISTS` subquery serves; measure before denormalizing again.

**`Update()` `[04 §3]`** — with Stage 9's `.AsTracking()` explicit on write paths, the
load-mutate handlers' `repository.Update(entity)` line is deleted; `SaveChanges` writes only the
changed columns. The 19 handlers Stage 9 made explicit keep their `Update()` **until** their
load converts to tracked, then lose it too. When a repository's `Update` has no callers left,
the method goes.

## Part D — Specifications

`ArticleByIdSpecification`, `ArticleCommentByArticleIdSpecification` and the other ~136
single-use types inline into their call sites:

```csharp
// Before
var specification = new ArticleByIdSpecification(id: id);
return await context.Articles.ApplySpecification(specification: specification)…

// After — the predicate says the same thing without the type
return await context.Articles.Where(a => a.Id == id)…
```

Multi-use predicates (the handful of user-role specs in Identity) are out of scope until
Stage 18. The `docs/testing` rule that specs are covered "through the repository method that
uses them" becomes vacuously satisfied — the repository tests already assert the behaviour.

---

## Tests

- **Unit:** every new guard transition (accept-after-accept false, accept-after-reject throws,
  event raised exactly once — assert `DomainEvents.Count` after double-call); the
  `HasPublishedLyricsAsync` predicate against draft/published/deleted lyrics (integration,
  repository).
- **Integration — the re-notify regression:** decide a revision twice over HTTP → second call
  409s and **no second notification row/outbox email** exists. This is the `[03 §7]` proof and
  it fails today.
- **Integration — column clobber regression:** load an article on the tracked path, change only
  the title, commit; assert (via SQL logging or a concurrent body edit) the body column was not
  in the UPDATE. Fails before D4's change.
- The existing suites are the no-op proof for D1: removing navigations must not change any
  response body.

---

## Rollout

`HasLyrics` column drop is the only schema change; migration generated, unapplied. D1 lands
aggregate-by-aggregate — each series independently shippable and revertible.

---

## Verification

1. Build/format/unit/integration green.
2. Navigation census: cross-aggregate entity-typed navigations in Content Domain → 0.
3. `grep -rn "MarkHasLyrics\|UnmarkHasLyrics" src/` → empty.
4. `grep -rn "ApplySpecification" src/Modules/Content` → empty.
5. Builder reflection: `grep -rn "GetProperty\|SetValue" tests/Fixtures/Builders` → only the
   engagement-counter arrangement helper remains.

---

**PR:** `refactor(domain): aggregate boundaries, review guards, events and repository shape`
