# 03 — Content Domain Layer

Scope: `src/Modules/Content/Content/Domain` (49 entities, enums, events, value objects) and
the `Shared/Domain` base classes it builds on.

The pieces that are good are genuinely good — the promotion-window arithmetic, the
snapshot-price modelling, `ArtistEntity`. But the layer has three structural faults that
produce concrete correctness bugs: **everything is an aggregate root** (no child entities,
no consistency boundaries), **the editorial state machines are unguarded** (illegal
transitions are reachable), and **the domain takes a localization service as a method
parameter**, which is why so many invariants ended up in handlers instead.

---

## 3.1 Every persisted class is an aggregate root; there are zero child entities

**Severity: Critical** · root cause of §3.4 and part of §3.11.

**Where:** all 49 entity files are `: Aggregate<Guid>`; none is `: Entity<Guid>`.
`ContentDbContext` exposes a `DbSet` for all 49, including pure children
(`ContentOrderItemEntity`, `ContentItemTierEntity`, `ArticleTagEntity`).
`ContentOrderItemConfiguration.cs:26` cascades from the order — the persistence model says
"child", the domain model says "root".

**Problem/why.** Nothing owns a consistency boundary. `ContentOrderEntity.TotalAmountUsd` is
an invariant over `Items × Tiers`, but items and tiers are added through *their own*
repository entry points with no forced recalculation (see §3.4). Every class also carries a
`List<IDomainEvent>` and is scanned on every save, including 12 junction rows that never
raise anything.

**Solution.** Demote children to `Entity<Guid>` (already exists in `Shared/Domain`) and
delete their `DbSet`s so they are reachable only through their root:
- Tier 1 (pure junctions, no behaviour change): `ArticleTagEntity`, `VideoTagEntity`,
  `LyricsTagEntity`, `ArticleArtistEntity`, `PlaylistVideoEntity`, `PackageSlotEntity`,
  `ContentItemTierEntity`, `CategoryPricingEntity`, `ArticleImageEntity` — move any event
  raises (e.g. `ArticleTagEntity` raises `TagGraphChangedEvent`) onto the owning root first.
- Tier 2 (real children): `ContentOrderItemEntity`/`ContentPaymentEntity` → children of
  `ContentOrderEntity`; `ArtistSocialLinkEntity`/`StreamingLinkEntity` → children of
  `ArtistEntity`/`AlbumEntity`; the two vote entities → children of their revisions.
- Move `AddItemAsync`/`AddPaymentAsync` off the repository onto the root
  (`order.AddItem(...)`), so total recalculation cannot be skipped (§3.4).

---

## 3.2 64 navigation properties cross aggregate boundaries by object reference

**Severity: High** · overlaps [04 §2](04-content-infrastructure.md).

**Where:** 53 single-entity + 11 collection navigations, all pointing at other aggregate
roots — e.g. `LyricsEntity.Video/Customer/Category`, `VideoEntity.Shorts`,
`ArticleCommentEntity.Article`, and a full back-navigation on every interaction row
(`ArticleLikeEntity.Article` etc.). Worst query: `ContentOrderRepository.cs:64-75` — three
collection `Include`s in one statement with no `AsSplitQuery`.

**Problem/why.** Lazy loading is off, so the failure mode is silent row multiplication
(cartesian products) plus null-reference risk: a handler reads `category.ContentType.Name`
and only works because that repository call happened to `Include` it. Change the include set
anywhere and a handler NREs at runtime with no compile error.

**Solution.** Delete the back-references from the 12 interaction/junction entities (they are
write-only — no handler reads `like.Article`); keep the `Guid` FK and bind it with
`HasOne<T>().WithMany().HasForeignKey(x => x.ArticleId)`. Replace `VideoEntity.Shorts` with
a repository query. For read paths that need `Article + Category + Customer`, project
straight to the DTO in the repository rather than hydrating aggregates. Add `.AsSplitQuery()`
to the order query as an immediate mitigation.

---

## 3.3 Every editorial state transition is unguarded; the state machine lives in 9 handlers

**Severity: Critical**

**Where:** `ArticleEntity.Publish()` (`:445`) checks only `Status == Published` — it accepts
*any* other source state. Same for `Submit`/`Approve`/`Archive` and the Video/Lyrics twins.
The real guard is copy-pasted into 9 Admin handlers, e.g. `AdminPublishArticleHandler.cs:35`
(`if (Status != Approved) throw InvalidStatusTransition(...)`). The guard pair appears 26
times across 17 handlers.

**Problem/why.** Today each entity method has one caller, so the hole is latent — the second
caller ships the bug. Concretely: a paid article in `PendingPayment` can be published
without the customer paying; an `Archived` article can be silently republished with a fresh
`PublishedAt`, losing its original date; `Submit()` on a `Published` article un-publishes it
into the payment queue **with no `ArticleUnpublishedEvent`** (contrast `Reject`/`Archive`
which raise one), so the homepage cache and search index keep serving a page that is no
longer published.

**Solution.** One transition table, shared by all three content entities (identical for
Article/Video/Lyrics — put it in `Domain/Entities/ContentPublicationState.cs`):

```csharp
[Draft]          = [PendingPayment, PendingReview],
[PendingPayment] = [PendingReview, Rejected],
[PendingReview]  = [Approved, Rejected],
[Approved]       = [Published, Rejected, Archived],
[Published]      = [Archived, Rejected],
[Rejected]       = [PendingReview, Archived],
[Archived]       = [PendingReview],
```

Route every transition through `EnsureCanMove(Status, target, errors)`, make the methods
throw on an illegal source instead of returning `bool`, and delete the 18 duplicated guard
lines from the handlers. Keep `MarkPendingReview`'s idempotent `bool` return (it is
load-bearing for `OrderPaidEffectsHandler` event replay). Do `Publish` first (highest money/
SEO exposure).

---

## 3.4 Order total is not recalculated when an item is added — the payment snapshots a stale amount

**Severity: Critical** · a symptom of §3.1.

**Where:** `ContentOrderEntity.RecalculateTotalFromItems()` (`:121`) has 5 callers;
`AdminAddOrderItemFactory.cs:89-101` is **not** among them — it creates the item, commits,
and returns without recalculating. `AdminSubmitOrderFactory.cs:24-35` then freezes whatever
`TotalAmountUsd` holds into `ContentPaymentEntity`. Its guard is `Any` (one item with a
tier), not `All`.

**Problem/why.** Add item A → add tier to A (recalc fires) → add item B with a $200
promotion level and no tiers (**no recalc**) → submit. The total omits B's $200. The
customer is invoiced short, the payment is verified against the wrong amount, and
`OrderPaidEvent` still stamps B's promotion. Revenue lost silently, receipt wrong.

**Solution.** Immediate: add `order.RecalculateTotalFromItems()` + update before the commit
in `AdminAddOrderItemFactory`. Structural (with §3.1): make items children and move creation
into `ContentOrderEntity.AddItem(...)`, which calls `RecalculateTotalFromItems()` and then
make that method **private** so no call site can forget it. Tighten `AdminSubmitOrderFactory`
from `Any` to `All`.

---

## 3.5 `ContentPaymentEntity.Verify` accepts a payment with no proof attached

**Severity: High**

**Where:** `ContentPaymentEntity.Verify` (`:117`) guards the status but never checks
`PaymentProofFileId`/`PaymentMethod`. `AttachProof` (`:101`) has no status guard.

**Problem/why.** A `Pending` payment with `PaymentProofFileId == null` verifies cleanly,
flipping the whole order to `Paid`, raising `OrderPaidEvent`, and moving all commissioned
content to `PendingReview` — content goes live off an unevidenced payment, with no artefact
to dispute. Symmetrically, `AttachProof` can overwrite proof on an already-verified payment.

**Solution.** In `Verify`, throw `PaymentProofRequired()` when proof/method is null. In
`AttachProof`, throw `PaymentAlreadyDecided()` when status isn't `Pending`. Add the two error
methods (or, preferably, domain exceptions per §3.6).

---

## 3.6 The domain takes a localization service as a method parameter (the `errors` injection)

**Severity: High** · root cause of why §3.3/§3.5 guards ended up in handlers.

**Where:** 17 of 49 entity files carry `using _116.Content.Application.Shared.Errors` —
Domain depends on Application, unenforced because it is one assembly. 50 method signatures
take an `Errors` parameter (`VideoEntity.Publish(VideoErrors errors)`), and those classes
resolve to `IStringLocalizer`. So `LyricsEntity.Publish` transitively depends on
`Microsoft.Extensions.Localization` and the ambient `CultureInfo`.

**Problem/why.** Three costs: (a) the domain can't be constructed without a DI container —
every test builder opens with `TestErrorsFactory.Create...()`; (b) a rule and its wording
are welded together, so adding a guard is a domain change *plus* a new error method *plus*
three `.resx` edits *plus* a signature ripple — which is exactly why §3.3/§3.5 guards were
put in handlers instead; (c) a request-scoped culture concern cannot be evaluated in a
background job or replay.

**Solution.** Invert it. Domain throws typed, culture-free exceptions; the presentation edge
translates them.
1. Add `Domain/Exceptions/DomainRuleException.cs` carrying a stable `string Code`
   and structured args — no message string.
2. Rewrite guards as `throw new DomainRuleException(LyricsRules.SongTitleRequired)`;
   drop the `errors` parameter from all 50 signatures.
3. Add a `DomainRuleExceptionStrategy` that maps `Code` → `IStringLocalizer` →
   `ProblemDetails`. The `.resx` files stay, re-keyed by `Code` — no i18n coverage lost.
4. Pilot on `LyricsEntity` (12 guards, one file), then the other content entities, then split
   `Content.csproj` so the layering violation becomes a compile error (§[06 §14](06-content-application.md)).
   Application-layer errors (`NotFound(id)`, `SlugAlreadyExists`) are correctly placed — leave
   them.

---

## 3.7 Review-workflow entities have no status guard — decisions are re-runnable and re-notify

**Severity: High**

**Where:** `LyricsSubmissionEntity.Approve/Reject/RequestRevision` and
`LyricsRevisionEntity.Accept/Reject` (and the translation twins) set the terminal state with
no check on the current one. The compensating guard is applied inconsistently by callers:
`AdminApprove...`/`AdminReject...` guard it, but **`AdminDecideLyricsRevisionHandler.cs:33`
and `AdminDecideTranslationRevisionHandler.cs:34` do not**.

**Problem/why.** Two admins hitting *Accept* on an already-accepted revision both succeed,
each raising the decided-event → the proposer is notified twice. Worse, a moderator can
*Accept* a revision the community already **Rejected**, and the handler then overwrites the
canonical lyrics text with the rejected proposal — a content-integrity bug from a normal
admin endpoint.

**Solution.** Put the guard in the entity: `if (Status != Pending) throw
RevisionAlreadyDecided()` in `Accept`/`Reject` (and the three submission methods against
`Pending`). Remove the now-redundant handler guards.

---

## 3.8 Zero strongly-typed IDs: 105 raw `Guid` properties, all mutually assignable

**Severity: High**

**Where:** all 105 `Guid` id properties are bare. `LyricsEntity.CreatePaid(Guid id, Guid
customerId, Guid orderItemId, Guid categoryId, Guid? videoId, ..., Guid authorId, ...)` —
five consecutive positionally-interchangeable `Guid`s. Money is `decimal` with currency in
the property name (6 fields, non-negativity enforced in 2 of 6); slug is `string` in 7
entities (only `IsNullOrWhiteSpace` checked); language is `string`; URL is `string` with the
`https` rule explicitly documented as living in the validator, not the domain.

**Problem/why.** `CreatePaid(...)` with `customerId`/`orderItemId` transposed compiles, runs,
and writes a row whose FKs both resolve — surfacing months later as a broken commission
report. `ContentPaymentEntity.Create` accepts a negative amount. A slug with `/` or a space
silently produces a broken permalink.

**Solution.** Do not convert 105 at once — sequence by protection-per-blast-radius:
1. **`Money`** first (6 fields, highest value): `readonly record struct Money(decimal
   Amount, string Currency)` with a non-negative ctor guard, via `HasConversion`. Closes 4
   missing guards.
2. **`Slug`** (7 fields): validates `^[a-z0-9]+(-[a-z0-9]+)*$`; replaces 7 weak checks with
   one real invariant.
3. **`LanguageCode`** (3 fields): BCP-47 allowlist.
4. **IDs** — start with the 7 that actually collide in signatures (`ArticleId`, `VideoId`,
   `LyricsId`, `CustomerId`, `OrderItemId`, `CategoryId`, `PromotionLevelId`) as
   `readonly record struct`s with `HasConversion`; ~35 of the 105. Leave `AuthorId`/`UserId`
   as `Guid` — they are deliberately FK-free cross-schema references.

---

## 3.9 Test builders must use reflection for `PublishedAt` and 8 navigation properties

**Severity: Medium** · symptom of §3.2 and a missing clock seam.

**Where:** production uses no entity reflection; test fixtures need it in 28 files (21
`SetValue` calls) for `PublishedAt` and cross-aggregate navigations. `ArticleEntity.Publish`
hard-codes `PublishedAt = DateTimeOffset.UtcNow`; `ShortVideoEntity` uses
`Random.Shared.NextInt64` for `FeedRank`.

**Problem/why.** Two smells. (a) The domain reads the wall clock directly (30 direct
`UtcNow`/`Random` calls across `Domain/Entities/`), so a test needing deterministic
"latest-first" ordering — or a backfill preserving historical publish dates — *cannot* use
the domain. (b) The navigation reflection is direct evidence the 8 properties don't belong on
the aggregate (§3.2) — production never sets them either; EF does via `Include`.

**Solution.** Inject the clock — `TimeProvider` is already a first-class dependency
(`AuditableEntityInterceptor` uses it). Thread an instant through transitions
(`Publish(DateTimeOffset publishedAt)`, `StampPromotion(levelId, until, at)`), handlers pass
`timeProvider.GetUtcNow()`. `ShortVideoEntity.NewFeedRank()` takes the seed as a parameter.
The navigation reflection disappears with §3.2.

---

## 3.10 Three state changes raise no domain event; `LyricsEntity` has no delete event at all

**Severity: Medium**

**Where:** the mechanism is correct (past-tense records, dispatched post-save). Gaps:
`Submit()` can move `Published → PendingPayment` with no event (§3.3); `LyricsEntity.Publish`
raises `CommissionedContentPublishedEvent` but no `LyricsPublishedEvent` (Article/Video both
raise their own); **`LyricsEntity` has no `MarkDeleted()`** while Article/Video/ShortVideo/
Tag do, so `AdminDeleteLyricsHandler` calls `Remove` directly and `CoverImageFileId` leaks
its Cloudinary asset forever; `ReplaceLyricsText` rewrites canonical content with no event
(no cache invalidation / re-index); `ArticleCommentEntity.Edit` raises nothing and has no
`IsDeleted` guard.

**Problem/why.** Each gap is a stale-read bug in a cache/index the event pipeline exists to
keep fresh, plus one orphaned asset per deleted lyrics page.

**Solution.** Add `LyricsPublishedEvent`, `LyricsUnpublishedEvent`, `LyricsDeletedEvent`
(carrying `CoverImageFileId`), `LyricsTextReplacedEvent`, mirroring the article/video files.
Add `LyricsEntity.MarkDeleted()`; register the new events with the existing
`ContentAssetCleanupHandler`. The `Submit()` gap closes with §3.3's transition table.

---

## 3.11 `VideoEntity.HasLyrics` is a denormalized cross-aggregate flag maintained by four handlers

**Severity: Medium**

**Where:** `VideoEntity.HasLyrics` + `MarkHasLyrics()`/`UnmarkHasLyrics()`, written from 4
lyrics handlers. `LyricsConfiguration` has no unique index on `VideoId`, so many lyrics may
point at one video, but `UnmarkHasLyrics()` sets `false` unconditionally with no count check.

**Problem/why.** Two lyrics pages on one video, delete one → `HasLyrics` flips to `false`
while the second still exists → the video hides its lyrics tab for live content. The flag is
also written in the same transaction as a *different* root's mutation (two roots per
transaction).

**Solution.** Best: delete the column — derive it via `context.Lyrics.Any(l => l.VideoId ==
v.Id)` in a `VideoRepository` projection (read on one surface). If it must stay, replace the
four handler calls with a `LyricsLinked/UnlinkedFromVideoEvent` pair consumed by a handler
that recounts. If a video may only ever have one lyrics page, add the unique filtered index
and the ambiguity disappears — a product decision.

---

## 3.12 31 of 49 entities are anemic row-wrappers; behaviour concentrates in 3 files

**Severity: Medium**

**Where:** 197 public methods across 49 classes; 13 entities have exactly one (a static
`Create`), 18 more have two; `LyricsEntity`/`VideoEntity`/`ArticleEntity` hold 68 (35%). In a
12-handler sample, 9 carry a domain rule the entity does not — the whole read-time
view-counting algorithm lives in `PublicRecordLyricsViewHandler.cs:93`; the "exactly one
exclusive/gossip/default category" mutex is documented at `CategoryEntity.cs:70` as the
*handler's* responsibility.

**Problem/why.** Rules that should be portable are not. `SatisfiesReadTimeRule` is the
platform's definition of "a view happened" but lives where a second entry point can't reach
it. The `CategoryEntity` mutexes are enforced nowhere structural — nothing prevents two
exclusive categories, and the homepage then renders whichever the query returns first.

**Solution.** Pull down rules that are about one aggregate (`SatisfiesReadTimeRule` →
`LyricsEntity.CountsAsRead(...)`; transition legality → §3.3). Keep up rules that need a
repository (slug uniqueness, the pin FIFO cap). Move the three `CategoryEntity` mutexes to
the **database** as partial unique indexes (`HasIndex(x => x.IsExclusive).IsUnique()
.HasFilter("is_exclusive")`) — an invariant a comment asks a handler to remember is not an
invariant. The 13 one-method entities are fine as-is once demoted to `Entity<Guid>` (§3.1).

---

## 3.13 `EnumContentStatus` drives 26 duplicated `if` comparisons, not a state pattern

**Severity: Low**

**Where:** 26 `Status == EnumContentStatus.X` comparisons across 17 handlers — 18 write-side
(the transition guards of §3.3) and 6 read-side `== Published` visibility filters that
duplicate predicates already in the specifications.

**Judgement.** A state pattern is **not** warranted — no status has its own rendering,
pricing, or workflow. Seven `IContentState` classes would express in 7 files what a 7-entry
dictionary expresses in 9 lines. The real problem is duplication of *one* concern (legal
transitions), which is a table (§3.3), not a hierarchy.

**Solution.** §3.3's table collapses the 18 write-side comparisons. Route the 6 read-side
filters through the existing published-content specifications so "what the public can see"
has one definition. Leave `ContentOrderEntity.Cancel`'s switch expression alone — it is the
correct shape.

---

## What is done well here

- **The domain-event pipeline is genuinely correct** — collect on `SavingChanges`, dispatch
  on `SavedChanges`, discard on failure/cancel, fresh DI scope per handler, request token
  deliberately not forwarded. All 30 events are past-tense records.
- **Delete flows capture state before the row disappears** —
  `ArticleEntity.MarkDeleted(bodyImageStorageKeys)` and the video twins put file ids/keys
  *into the event* so the cleanup consumer never queries a deleted row. Right pattern,
  applied to three of four content types (the fourth is §3.10).
- **Money is snapshotted, not recomputed** — `PriceSnapshotUsd`/`PromoPriceSnapshotUsd` freeze
  prices at quote time so a later admin price change can't alter a customer's bill.
- **`ContentOrderEntity.MarkPaid` computes promotion windows once, at raise time**, and puts
  them in `OrderPaidEvent` with millisecond truncation so replay compares equal — careful,
  well-reasoned idempotency.
- **`ArtistEntity` is the one properly-modelled aggregate** — `NameFolded`/`InitialLetter`
  derived inside the entity on create *and* rename, `ReplaceAliases` normalises inside the
  domain "so the invariant holds for every writer, including seeds", immutable slug, atomic
  `ClaimOwnership`. This is what the other 48 should look like.
- **`ArticleCommentEntity.SoftDelete` reasons about counter drift explicitly**, returning
  `false` on a second delete so owner-delete + admin-moderation can't double-decrement the
  cached count.
- **Idempotent `MarkPendingReview`** — the one transition where the permissive `bool` return
  is the right call, tied to payment-effect replay.
