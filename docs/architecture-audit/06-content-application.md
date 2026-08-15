# 06 — Content Application Layer

Scope: `src/Modules/Content/Content/Application` — ~1,230 files across six slices (Editorial
549, Interactions 190, Shared 139, Catalog 125, Lookup 116, Commerce 110). 63 read in full,
plus exhaustive greps.

The slice discipline is real and worth protecting: validators are pure, no handler touches
`DbContext`, no handler calls another handler, cross-slice coupling is 3 files. The problems
are at the edges — every endpoint drops the cancellation token, `pageSize` is unbounded, public
detail DTOs leak commercial fields, the whole Interactions slice skips the account-status
policy, and N+1 mapping that a correct batch implementation next door already solved.

---

## 6.1 Every one of the 221 endpoints drops the request's CancellationToken

**Severity: Critical** · same finding as [01 §1.5](01-composition-root-and-shared-kernel.md),
scoped to Content.

**Where:** `dispatcher.Send(` appears 221 times in endpoints; occurrences passing a token: 0.
Handlers then thread `cancellationToken` faithfully through ~12 repository calls — all
`CancellationToken.None`.

**Problem/why.** A client that aborts `GET .../articles/promotion-feed` (7+ queries, gossip
pool, file lookups) leaves the whole chain running to completion holding a pooled connection.
Under a retry storm the pool saturates from work nobody awaits; `connection pool exhausted`
surfaces on unrelated endpoints.

**Solution.** Add `CancellationToken cancellationToken` to each endpoint lambda (before the
optional query params) and pass it to `Send`. Then drop the `= default` on `IDispatcher.Send`
so omission is a compile error (last, after the 221 land).

---

## 6.2 `pageSize` is entirely client-controlled on 33 of 34 paginated endpoints

**Severity: Critical** · same as [08 §6](08-cross-cutting.md).

**Where:** `PaginatedRequest`'s `[Range(1,100)]` is dead — the record is hand-constructed, never
model-bound. 34 endpoints bind loose `int pageSize = 10`; exactly 1 clamps
(`PublicGetShortsFeedEndpointV1`). Handlers do `page: pageIndex + 1`, so `pageIndex=-1` yields a
negative OFFSET and a 500.

**Problem/why.** `GET /api/v1/public/articles?pageSize=1000000` is an anonymous single-request
DoS — a million rows materialized, each (per §6.4) with a file lookup. Rate limiting doesn't
help; one request suffices.

**Solution.** Add `PaginatedRequest.From(pageIndex, pageSize)` clamping to `[1, MaxPageSize=100]`
and `Max(0, pageIndex)`; replace the 35 construction sites; make the primary constructor private.
Fold the shorts feed's bespoke clamp into the shared constant.

---

## 6.3 22 list endpoints have no pagination and no server-side cap

**Severity: High**

**Where:** 34 queries return `PaginatedResult<T>`, 22 return `IReadOnlyList<T>`. The unpaginated
set includes `AdminGetAllTags` (whose Public sibling *does* cap via `Limit`), `GetAllContentTypes`,
`GetActiveVideos`, `GetOwnPlaylists`, `GetLyricsTranslations`, `GetTranslationRevisions`.

**Problem/why.** `GET /api/v1/admin/tags` returns every tag ever created — tens of thousands
after a year — on every admin page that renders a tag picker. Response times creep until the
endpoint times out, with no query parameter to recover.

**Solution.** Two tiers: genuinely bounded reference data (`ContentTypes`, `PricingTiers`,
`PromotionLevels`) gets an `int? Limit` + `Take` backstop; unbounded lists convert to
`PaginatedResult<T>` copying the `AdminGetAllArticles` shape (breaking response change — ship
together, still V1 pre-release, or mint `V2/`).

---

## 6.4 Hand-written mappers do one file lookup per entity — N+1 on 21 call sites, while a correct batch exists next door

**Severity: High** · overlaps [04 §13](04-content-infrastructure.md), [02 §8](02-module-boundaries.md).

**Where:** `ArticleMapper.cs:158` loops `await entity.ToArticleSummaryDtoAsync(...)` and inside
does `fileRepository.GetByIdAsync(entity.CoverImageFileId)` per entity. Identical loops in Video/
Lyrics/Category/Artist/Album mappers. The correct pattern is already written —
`ShortVideoMapper.BuildDtosAsync` collects ids into a `HashSet`, does one `GetByIdsAsync`, maps
in memory ("two round-trips total, regardless of page size"). `VideoMapper` even has a no-IO
batch overload used at 2 sites while the N+1 overload is used at 8. 21 N+1 call sites total; the
promotion feed calls the single-entity mapper inside loops.

**Problem/why.** `GET .../admin/articles?pageSize=100` = 1 article query + 100 sequential file
round-trips. With §6.2 (unbounded pageSize) this is the amplification factor. The homepage
promotion feed — the hottest anonymous route — is affected.

**Solution.** Port `ShortVideoMapper.BuildDtosAsync` into the six mappers; re-point the existing
`ToXxxDtosAsync` list signatures at it (the 21 call sites need no edit). In the two feed handlers,
hoist a single `GetByIdsAsync` for the union of cover files (already built for the likes lookup).
Delete the surviving single-entity-in-a-loop overloads.

---

## 6.5 The whole Interactions slice (35 endpoints) skips the account-status policy the other 148 apply

**Severity: High**

**Where:** `AccountStatusPolicies.RequireActiveUser` appears on 148 endpoints; the 35
non-anonymous endpoints missing it are all in `Interactions/` — `LikeArticle`, `AddArticleComment`,
`AddCommentReply`, `EditArticleComment`, `DeleteArticleComment`, `CreatePlaylist`, `RateVideo`,
`BookmarkArticle`, and the admin comment delete. They carry `RequireVisitorOnly` and a rate-limit
policy but not the status gate.

**Problem/why.** `AccountStatusRequirement` is the suspension/verification gate. A user banned for
comment abuse still holds a valid access token with the `Visitor` role, and `RequireVisitorOnly`
alone passes — so they keep commenting until the token expires (default 60 min). Moderation is
unenforceable for the token lifetime.

**Solution.** Add `.WithAuthorization(AccountStatusPolicies.RequireActiveUser)` above the role
policy in all 35 files (or fold the requirement into `RequireVisitorOnly` centrally, per
[07 S8](07-identity-and-security.md)). Add an architecture test asserting every Content
`ICarterModule` is either `AllowAnonymous` or carries the status policy.

---

## 6.6 27 public endpoints return `AuditableDto` subclasses, leaking audit and commercial fields

**Severity: High**

**Where:** `AuditableDto` documents "Public-facing DTOs must NOT inherit from this type" yet 15
Content DTOs do, and 27 public endpoints return one — including the anonymous
`PublicGetArticleBySlugEndpointV1`. `ArticleDetailDto` serves both `GET /admin/articles/{id}` and
anonymous `GET /public/articles/{slug}`, exposing `RejectionReason`, `SocialBoost`, `Status`,
`CustomerId`, `CustomerName`, `OrderItemId`, plus `CreatedBy`/`UpdatedBy` (staff UUIDs).

**Problem/why.** An anonymous reader of a promoted article learns which B2B customer commissioned
it, that it is a paid placement, the editor's internal `RejectionReason` from a prior review, and
staff UUIDs. For a media business whose value proposition is that promoted content reads as
editorial, `CustomerName` on the public endpoint is a commercial disclosure.

**Solution.** Split the detail/summary DTOs along the Admin/Public axis — the one place where they
genuinely differ. Add `Public*Dto` records (no `AuditableDto`, only the public fields) and
`ToPublic*DtoAsync` mappers; repoint the public handlers. Start with the three detail DTOs (worst
leak), then summaries. Add an architecture test: no `AuditableDto` subtype in the `Produces<T>` of
a `/public/` endpoint.

---

## 6.7 Multi-step writes commit mid-flight with no transaction — a failed validation can empty the "exclusive category" mutex

**Severity: High** · same root as [04 §7](04-content-infrastructure.md).

**Where:** `grep BeginTransaction|TransactionScope` → 0; `IContentUnitOfWork` is `CommitAsync`
only. `AdminUpdateCategoryHandler.cs:64-108` clears the previous exclusive category and commits
(#1), clears the previous default-lyrics category and commits (#2), then `category.Update(...)`
(**throws on bad input**) and commits (#3). Same in `AdminSetExclusiveCategoryHandler`. 13 files
commit more than once.

**Problem/why.** `category.Update` validates and throws after commit #1 has durably cleared the
previous exclusive category. The request returns 400 and the site has **zero** exclusive
categories — the homepage slot goes blank with no rollback. The default-lyrics window similarly
breaks public lyrics submission.

**Solution.** For these two, no transaction is even needed: validate first (`category.Update(...)`
mutates the tracked entity), then clear the old ones, then a **single** `CommitAsync` — EF makes
one `SaveChanges` atomic across both, exactly as `AdminPinCategoryToFeedHandler` already does for
FIFO eviction. Add `ExecuteInTransactionAsync` to the UoW for the genuinely multi-step cases
(`UploadArticleImage`, which calls Cloudinary before the commit). Audit the other 11.

---

## 6.8 The Admin/Public axis is copy-paste: pairs differ by a 3-line guard yet duplicate ~8 files each

**Severity: High**

**Where:** `AdminDeleteArticleCommentHandler` and `PublicDeleteArticleCommentHandler` are
byte-identical apart from the Public one's 3-line ownership check. Around that delta sit 8
duplicated files (~250 lines). The pattern recurs across `GetAllTags`, `SubmitLyrics`, and the
semantic read pairs (`AdminGetAllArticles`/`PublicGetPublishedArticles`, etc.).

**Problem/why.** The delta *is* the authorization rule — the one thing not centralised. When the
ownership rule changes, someone edits the Public handler and misses that the Admin path has no
guard, or vice versa; the 3-line guard is invisible inside 250 lines of identical scaffolding.
`PublicGetAllTagsQuery` already gained filters + caching the Admin sibling never got — the pair
has drifted (§6.3).

**Solution.** Do **not** merge the endpoints (separate routes/policies/docs are correct). Merge
the *rule*: a `CommentModerationPolicy.EnsureCanDelete(comment, actingUserId, errors)` static both
handlers call — the delta becomes one named line. For read pairs differing only in a status
filter, keep both folders but pass `status: Published` into the *same* repository method the Admin
one uses.

---

## 6.9 `ContentI18n` is a 22-dependency god facade constructed on 232 files

**Severity: Medium**

**Where:** `ContentI18n` takes 22 `*Errors` types; referenced in 232 files, most using one or two
(`PublicLikeArticleHandler` uses only `i18n.ArticleInteraction`).

**Problem/why.** (a) Every handler resolution builds all 22 error classes and their localizers for
one property access. (b) It defeats slice isolation at the type level — `PublicLikeArticleHandler`
is compile-time dependent on `ContentOrderErrors`, so a Commerce error change recompiles and can
break Interactions. The "vertical slice" boundary exists in folders but not the type graph.

**Solution.** One facade per slice (`EditorialI18n`, `InteractionsI18n`, …) aggregating only that
slice's errors; keep a thin `ContentI18n` composite for the genuinely cross-slice consumers.
Migrate slice-by-slice (Lookup first, Editorial last) — a mechanical constructor-type swap; the
`i18n.Xxx.Yyy()` call sites are unchanged.

---

## 6.10 Every use case re-declares its route group; 7 public endpoints landed outside `/public`

**Severity: Medium** · overlaps [08 §14](08-cross-cutting.md).

**Where:** all 221 endpoints open with a copied 3-line `MapApiVersionGroup(1).MapGroup(...)
.WithTags(...)`; 37 distinct group strings for ~24 resources. 7 omit the scope prefix entirely
(`POST /api/v1/lyrics/submissions` instead of `/api/v1/public/lyrics/submissions`, plus revision/
vote/claim endpoints). One Commerce group uses a `"customers"` string literal while Catalog uses
the constant.

**Problem/why.** The 7 unscoped routes sit outside whatever the gateway/WAF/CDN keys off `/public`
vs `/admin` — edge cache, auth pre-checks, and per-scope rate policies silently don't apply to
lyrics submission or artist claims. The literal `"customers"` splits the resource in Swagger and
breaks at runtime if the constant is renamed.

**Solution.** Add `MapAdminGroup(resource)`/`MapPublicGroup(resource)` extensions defining the
convention once; replace the preamble in all 221 files. Move the 7 unscoped routes under
`Public` (breaking URL change — coordinate with frontend/mobile, or register both for one release
with the old one `ExcludeFromDescription`). Replace the `"customers"` literal.

---

## 6.11 `ContentBrowsing` — a read policy — rate-limits 129 of 151 write endpoints

**Severity: Medium** · same as [08 §1 context](08-cross-cutting.md).

**Where:** `ContentBrowsing` appears on 199 endpoints, including 129 of the 151 write endpoints
(`POST /admin/articles`, `POST /admin/orders`, `PATCH .../set-exclusive`). The three purpose-built
policies (`ContentContribution`, `FileUpload`, `DataExport`) are used correctly on 22.

**Problem/why.** A read policy tuned for high-volume idempotent GETs gives comment-spam and
duplicate-order floods the same budget as browsing the homepage; a burst of writes can also
exhaust the browsing budget for the same principal.

**Solution.** Reclassify by verb+scope: public writes → `ContentContribution`, admin writes → a
new `AdminMutation` fixed-window policy, GETs stay on `ContentBrowsing`. Fold the choice into the
`MapAdminGroup`/`MapPublicGroup` helpers (§6.10) so it can't be forgotten.

---

## 6.12 Admin authorization is split SuperAdmin/Admin mid-lifecycle on the same resource

**Severity: Medium**

**Where:** of 142 admin endpoints, 54 `RequireSuperAdminOnly`, 88 `RequireAdminOrSuperAdmin`. The
split cuts through single resources: article `Create/Publish/Approve/Reject/Archive/Delete` are
SuperAdmin, but `Update/UpdateSeo/UpdateTags/SetArtists` are Admin. `VerifyPayment` and
`CancelOrder` are Admin; deleting a *tag* is SuperAdmin.

**Problem/why.** An Admin cannot publish an article but *can* rewrite the body, headline, tags, and
SEO of an already-published one — the gate is on the transition, not the content, so the protection
is illusory: publish once as SuperAdmin, then any Admin changes what the URL says. `VerifyPayment`
(fires `OrderPaidEvent` stamping real-money promotions) is available to any Admin. It may be
deliberate, but nothing states the intent and the split follows no axis.

**Solution.** Decide and document the axis. Recommended: SuperAdmin = state transitions +
destructive + money; Admin = content authoring. Encode it as a constant per operation kind so the
mapping is auditable in one file; re-review all 142 against it.

---

## 6.13 44 command use cases ship with no validator; 63 mutation endpoints omit `ProducesValidationProblem`

**Severity: Medium** · overlaps [08 §5](08-cross-cutting.md).

**Where:** 151 commands, 111 validators; 44 command folders have no validator (`CancelOrder`,
`RejectPayment`, `SetExclusiveCategory`, all 21 Interactions commands, several Editorial). 72
commands carry a `string`-typed identifier parsed with `Guid.Parse` in the handler; 6 of those have
no validator, so `Guid.Parse` runs on raw route input. 20 routes use bare `{id}` vs 8 `{id:guid}`.

**Problem/why.** `PATCH /api/v1/admin/categories/not-a-guid/set-exclusive` reaches the handler and
throws `FormatException` (caught → 400, but a generic one with no field name), while the same
mistake on `POST /admin/orders` returns a localized field-level `ValidationProblemDetails`. Two
error contracts for one class of mistake. The 63 missing `ProducesValidationProblem` mean generated
clients have no 400 branch.

**Solution.** Change the 20 `{id}` templates to `{id:guid}` and `string id` to `Guid id` — ASP.NET
then 404s a malformed id before any handler runs, and the `Guid.Parse` disappears. That lets 72
commands take `Guid`, removing 118 `Guid.Parse` and 66 `IsValidGuid` rules. Add validators for the
44 free-text commands; add `ProducesValidationProblem` to the 63 endpoints.

---

## 6.14 The Specification layer is inert (136 single-use types), and query builders in Application depend on `ContentDbContext`

**Severity: Medium** · overlaps [04 §12](04-content-infrastructure.md).

**Where:** 21 spec files, 1,927 lines, 136 classes — 132 used once, 3 twice, 1 dead. Every use is
inside one repository method; 0 are referenced from handlers. Meanwhile 4 `*QueryBuilder` classes in
`Application/*/Builders/` take `ContentDbContext` and return `IQueryable` — even the `I*` interfaces
leak the DbContext. Content is a single project, so nothing prevents it.

**Problem/why.** Two competing abstractions over the same job — 136 single-use specifications add
naming indirection with zero reuse payoff, while the builders that *are* composable sit on the wrong
side of the dependency arrow (untestable without EF, hard-coding `context.Articles`). Because the
layering is folder convention, it will keep spreading.

**Solution.** Make the boundary real: split `Content.csproj` into `Content.Domain`/`.Application`/
`.Infrastructure` with inward-only references — the 8 offending files then fail to compile. Move the
query builders into `Infrastructure/Queries/`; keep only the `I*` contracts in Application, retyped
to return a `Specification` or parameter object. Collapse the 132 single-use specifications into
their one repository method, keeping ~4 genuinely reusable predicates. After §6.1–§6.7.

---

## 6.15 Interaction commands are check-then-act, and the guard sequence is re-typed 28 times

**Severity: Medium**

**Where:** `PublicLikeArticleHandler.cs:26-49` — existence load, `HasLikedAsync` probe, throw
`AlreadyLiked`, `AddLikeAsync`, commit. 28 handlers copy this shape. The DB enforces uniqueness (30
config files declare a unique index), but no handler handles `23505` and there is no
`DbUpdateException` strategy.

**Problem/why.** Two concurrent double-taps both pass `HasLikedAsync`, both reach `SaveChanges`,
one hits `23505` → falls through to `DefaultExceptionHandler` → **500** where the answer should be
409/200. Mobile clients on flaky networks retry — the single most likely 500 in production. The 28
copied guards mean an idempotency change lands in 3 of 28 places.

**Solution.** Add a `DbUpdateExceptionStrategy` mapping `23505` → 409 (fixes all 28 immediately).
Collapse the shape with `TryAddLikeAsync` (`INSERT … ON CONFLICT DO NOTHING`, one round-trip,
race-free) across Like/Unlike/Bookmark/Share for the four content types.

---

## 6.16 111 of 112 handlers inject `IMapper` without calling it

**Severity: Low**

**Where:** 112 handlers declare an `IMapper` parameter; 1 calls `mapper.Map`. Total `mapper.Map<`
sites in the whole layer: 37, of which 35 are inside `Shared/Mappers`. `ArticleMapper.cs:19` even
documents *why* Mapster was abandoned for the main mappings (auto-flattens `PromotionLevel` → NPE).

**Problem/why.** Not a runtime defect — a false signal. Every constructor advertises a mapping
dependency the handler doesn't have, and `MappingRegistration` compiles 13 registrations at startup
for effectively tag/image flattening.

**Solution.** Finish the migration `ArticleMapper`/`VideoMapper` started: replace the remaining
`mapper.Map<...Tags>` calls with hand-written `ToTagDtos`, drop the `IMapper` parameter from every
`ToXxxDtoAsync` and all 112 handler constructors, delete `MappingRegistration`, remove Mapster from
`Content.csproj` (verify no other module needs it first — see [01 §1.6](01-composition-root-and-shared-kernel.md)).

---

## What is done well here

- **Validators are pure** — 0 of 111 reference a repository or `DbContext`. Business rules that need
  the DB live in handlers or the domain. The single most commonly botched thing in FluentValidation
  codebases, done right.
- **Handlers never touch `DbContext`** (0 of 245); data access is uniformly through repository
  interfaces.
- **No handler calls another handler** — 0 inject `IDispatcher`. The CQRS graph is genuinely flat.
- **Cross-slice coupling is near-zero** — only 3 files import another slice's namespace.
- **Sorting/filtering are pushed into SQL** — only 4 in-memory `OrderBy`s exist, each legitimate on
  a small set; ranking is computed in the database.
- **Batch resolution is understood where applied** — the comment-list handler and
  `ShortVideoMapper.BuildDtosAsync` are model implementations. The knowledge exists; it just wasn't
  propagated (§6.4).
- **Domain logic is in the domain** — handlers orchestrate and delegate; guards return `bool` so
  callers detect no-ops, which is what makes the idempotency doc comments true.
- **`OrderPaidEffectsHandler` is genuinely well-reasoned** — documents redispatch safety field by
  field and refuses to revive a promotion a SuperAdmin pulled after payment. The strongest code in
  the module.
- **Rate limiting and authorization are never simply absent** — all 221 endpoints declare both; the
  problems are *which*, not *whether*.
- **Documentation discipline is exceptional and load-bearing** — the 221 `MetaField` files keep
  OpenAPI accurate, and the drift check found 0 cases of a MetaField promising a 404 the endpoint
  doesn't declare.
