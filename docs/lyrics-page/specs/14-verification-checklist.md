# Spec 14 — Verification

Full backend sweep across specs 01–13, after everything above lands. Mirrors the frontend's own
[08-verification.md](../../../../frontend/docs/lyrics-page/specs/08-verification.md) but scoped to
what's actually testable from `apps/backend`.

This checklist was audited item-by-item against the real, current codebase on 2026-08-01 — not a
rubber stamp of what earlier phases assumed. Several items below were corrected (marked
**STALE-WORDING**, with the real behavior described) and one real functional gap was found and
fixed during the audit itself (`sort=views|likes|shares` — see the "Slug & public list" section).

## Status workflow (spec 01)

- [x] A `Draft`/`PendingReview`/`Approved`/`Rejected`/`Archived` lyrics record is invisible to
  `GET /public/lyrics/{slug}`, `GET /public/lyrics/videos/{videoId}` (**STALE-WORDING**: the real
  route is `/videos/{videoId}`, not `/by-video/{videoId}` as earlier drafts of this doc set said),
  and `GET /public/lyrics` — visible only once `Published`.
- [x] **STALE-WORDING, partially FAIL as originally written**: `Approve`/`Publish`/`Reject` each
  reject an out-of-order call with `InvalidStatusTransition` (`BadRequestException`). `Submit`'s
  out-of-order case actually throws `AlreadyPendingReview` (`ConflictException`), not
  `InvalidStatusTransition` — a naming difference in the error thrown, not a missing guard.
  `Archive` has no "out-of-order" concept at all — it's reachable from any status and only
  no-ops if already `Archived` (matches `ArticleEntity`'s own `Archive()` exactly, which has the
  same unconditional-except-idempotent shape).
- [x] **STALE-WORDING**: `GET /admin/lyrics` (list) is correctly unfiltered by status by default.
  `GET /admin/lyrics/{id}` does not exist as a standalone endpoint — only the list endpoint and
  `GET /admin/lyrics/submissions` exist, both unfiltered by default.

## Slug & public list (spec 01)

- [x] Creating two lyrics records with the same slug is rejected (`SlugAlreadyExists`); updating a
  record to a slug already used elsewhere is rejected the same way; updating a record to keep its
  own current slug succeeds.
- [ ] **FAIL — frontend/dashboard scope, not fixed in this backend effort**: the dashboard's
  lyrics-create form does not follow the `{artist}-{title}-lyrics` slug formula, and does not even
  send `slug`/`categoryId` to the backend despite both being required by `AdminCreateLyricsCommand`.
  Left unchecked deliberately — this is dashboard/frontend work, out of scope for the backend
  phases that shipped this feature; flagging it here so it's not lost.
- [x] `GET /public/lyrics` — `search` matches song title, artist name, and lyrics text (`ILike`);
  `language` filters to an exact match; `sort=views|likes|shares` now genuinely orders by the
  matching `LyricsEntity` counter (**fixed during this audit** — the counters existed since Phase 5
  but were never wired into the sort switch until now, see `LyricsRepository.GetAllAsync`); all
  filters combine via `LyricsQueryBuilder`.
- [ ] `116.api.ts` regenerated — **NOT-APPLICABLE-TO-BACKEND**, this is a frontend TypeScript
  codegen step with nothing to verify from `apps/backend`. Left unchecked as out of scope here.

## Cross-link resolution (spec 02)

- [x] Video-linked lyrics resolves `videoSlug`; a stale/deleted linked video degrades to
  `videoSlug: null` without 404ing (verified: `GetByIdAsync`, never `GetByIdOrThrowAsync`).
- [x] Artist-linked lyrics resolves `artistSlug`; an unclaimed artist resolves `artistSlug: null`.

## Song metadata & cover (spec 03)

- [x] `coverImageUrl`, `album`, `releaseYear`, `label`, `songwriter`, `producer` all present on
  `LyricsSummaryDto`/`LyricsDetailDto` and correctly resolved; each metadata field is independently
  nullable/clearable via `PUT /admin/lyrics/{id}/metadata`.
- [x] `releaseYear` rejected outside `1900`–`current year + 1` (verified against
  `EditorialValidation.ValidReleaseYear`'s real, fixed implementation).

## Interactions & read-time algorithm (specs 04, 05)

- [x] Like requires auth; double-like conflicts (`AlreadyLiked`); unlike-without-a-like rejects
  (`LikeNotFound`).
- [x] Share and view-record work anonymously (`AllowAnonymous()` confirmed on both endpoints).
- [x] A view with `dwellMs` below the floor never counts, even with full scroll depth
  (`MinDwellFloor` check runs first in `SatisfiesReadTimeRule`).
- [x] A view with insufficient dwell time relative to a long song's word count doesn't count, even
  above the absolute floor.
- [x] A genuine full read counts exactly once per 24h dedup window (`ViewCountingConstants.DedupWindow`).
- [x] `viewCount`/`likeCount`/`shareCount`/`isLiked` all present and correct on the summary/detail
  DTOs; `isLiked` resolved per-caller, `false` for anonymous.

## Similar lyrics (spec 06, depends on spec 07 tags)

- [x] A video-linked record with same-category matches returns them, most recent first.
- [x] A record with tags but no video-category match returns shared-tag matches, ranked by shared
  count then recency.
- [x] A record with neither falls through to the 10 latest standalone records — **test gap closed
  during this audit**: only the empty-result variant had a test before; added
  `GetSimilarAsync_NoVideoAndNoTags_FallsThroughToLatestStandaloneBranch` proving the positive case.
- [x] A record with zero matches in any branch returns an empty list, not a 404 or error.
- [x] The chosen video-linked-fallthrough behavior (spec 06's explicit open note, resolved during
  Phase 6 planning: all three branches always tried in order, regardless of video linkage) is
  implemented and tested as decided.

## Tags (spec 07)

- [x] Setting an empty tag array clears all tags; setting a new set fully replaces the old one.
- [x] The same `TagEntity` row applies to an article, a video, and a lyrics page simultaneously
  (separate junction tables, shared `TagId`).

## Artist & album (spec 08)

- [x] Claiming an already-claimed artist profile conflicts (`AlreadyClaimed`, backed by a DB
  partial unique index on `UserId`).
- [x] An artist page (`GET /public/artists/{slug}`) shows only `Published` lyrics/videos.
- [x] Unlinking an artist from a lyrics record reverts display to the plain-text `artistName` with
  no data loss (`UnlinkArtist()` only clears `ArtistId`).
- [x] **STALE-WORDING**: the `SetNull` FK behavior is correctly configured on both Lyrics and Video
  (confirmed in their EF configurations) — but there is no admin "delete artist" use case/command
  in the codebase at all to actually exercise it end-to-end. The DB-level guarantee is real; the
  feature to trigger it doesn't exist yet. Not a bug in what was built, just narrower scope than
  this checklist item implies — no delete-artist endpoint was ever specced.

## Streaming links & album tracks (spec 09)

- [x] A stored streaming link takes precedence over the generated fallback; a missing platform
  link falls back to a valid generated URL for all four platforms (Spotify, Apple Music, YouTube
  Music, Tidal).
- [x] `albumTracks` excludes the current song and is empty when the song has no `albumId`.

## Translations & review (spec 10)

- [x] Requesting a translation that already exists returns it without a second AI-provider call.
- [x] A duplicate vote on the same revision by the same user is rejected by the unique constraint
  (`(RevisionId, UserId)` on both `LyricsTranslationVoteEntity` and `LyricsRevisionVoteEntity`).
- [x] The vote threshold auto-accepts a revision and updates the published translation text —
  **a real off-by-one bug was found and fixed during this phase's verification**: the tally query
  ran against the database before the just-cast vote was flushed, so the threshold check was
  always one vote behind and never actually fired on the deciding vote. Fixed in both
  `PublicVoteOnTranslationRevisionHandler` and `PublicVoteOnLyricsRevisionHandler` by tallying
  existing votes first, then adding the current vote's own +1/-1 contribution in memory rather
  than re-querying after an unflushed insert.
- [x] An admin override accepts or rejects a revision regardless of its current vote tally
  (`AdminDecideTranslationRevisionHandler`/`AdminDecideLyricsRevisionHandler` never query votes).

## Community submissions & corrections (spec 11)

- [x] A submission from a user who owns a claimed artist profile skips the queue entirely, starts
  in `Draft`, and is attributed to that profile's own name/id — verified with a deliberately
  mismatched `artistName` in the request, proving the gate is identity-based (`UserId`), not
  string-based.
- [x] A submission from a user with no claimed profile always queues, and is rejected if it omits
  `artistName`.
- [x] Approving a submission creates the `LyricsEntity` and links `publishedLyricsId`; rejecting or
  requesting revision records the reviewer and note.
- [x] A duplicate vote on a lyrics-text revision is rejected the same way translation-revision
  votes are (identical mechanism, separate table).

## Monetization: advertising, streaming affiliate, promoted placement (spec 12)

- [ ] Ad slots render on `/lyrics` and `/lyrics/{slug}` — **NOT-APPLICABLE-TO-BACKEND**: no
  dedicated Ads entity/module exists in `src/Modules/Content` to verify against; this is purely a
  frontend layout concern per spec 12 §1. Left unchecked as out of backend scope.
- [ ] Streaming-affiliate query params on generated/curated links — **not implemented**, and
  correctly so: `ResolveStreamingLinks` has an explicit inline comment marking this as deferred
  pending confirmation that each platform's affiliate program is actually active in this
  platform's markets — a business/legal gate, not a code gap. Left unchecked deliberately.
- [x] Verifying payment on a `ContentOrderItemEntity` with `ContentKind = Lyrics` stamps the
  linked `LyricsEntity.IsPromoted`/`PromotedUntil` correctly, without affecting that same order's
  article/video branches (`AdminVerifyPaymentFactory` resolves article/video/lyrics as mutually
  exclusive).
- [x] A lyrics record with no `OrderItemId` linked is untouched by payment verification.
- [x] **STALE-WORDING, corrected**: there is no `AdminLinkLyricsOrderItemCommand` — confirmed
  absent from the entire codebase (it only ever appeared in this checklist doc). The real
  mechanism: `customerId`/`orderItemId` are supplied directly via `LyricsEntity.CreatePaid(...)`
  (at creation) or `LyricsEntity.Update(...)` (retroactively, on an existing record) — neither
  requires the record already be `Published`, matching `ArticleEntity`'s identical mechanism.
- [x] **STALE-WORDING (route naming only)**: force-unpromoting a lyrics record clears
  `IsPromoted`/`PromotedUntil` (plus the `UnpromotedAt`/`By`/`Reason` audit trio) the same way it
  does for articles/videos, via `AdminForceUnpromoteLyricsCommand`
  (`POST /admin/lyrics/{id}/unpromote`) — note the route uses `{id}` while the article/video
  siblings use `{slug}`; functionally uniform, a minor naming inconsistency not worth a breaking
  change to fix now.
- [x] `IsPromoted` never appears in the "Top Lyrics" sort logic, for any sort mode including the
  newly-wired `views`/`likes`/`shares` branches (extended guard test added during this audit).

## Homepage discovery (spec 13)

- [x] `GET /public/videos?categoryId={116LyricsCategoryId}` returns the tagged videos — confirmed,
  no new endpoint required (`PublicGetPublishedVideosEndpointV1` already exposes `categoryId`).
- [x] `GET /public/lyrics?sort=views|likes|shares&pageSize=10` and the default-sort/`pageSize=10`
  "new lyrics" call both return correctly shaped, correctly capped results, **and now actually
  order by the requested criterion** (fixed during this audit — previously all three sort values
  silently produced identical newest-first ordering despite the counters already existing).

## Cross-cutting

- [x] `dotnet build` clean; `dotnet test` — every new unit + integration test suite passes;
  existing lyrics test suites (`LyricsEntityTests`, `LyricsRepositoryTests`,
  `AdminCreateLyricsHandlerTests`, etc.) updated for the `Slug`/`Status` requirements and otherwise
  unregressed. **Final numbers (2026-08-01, full unfiltered run, not trusted from prior phase
  notes)**: unit 6673 passed / 0 failed / 3 skipped (pre-existing, unrelated) / 6676 total;
  integration 1673 passed / 0 failed / 1673 total.
- [x] Every new rate-limited endpoint carries the correct policy (`ContentBrowsing` for reads,
  `ContentContribution` for authenticated writes) — **two real bugs found and fixed in this exact
  area**: `RateLimitPolicies.ContentContribution` was defined as a constant but never registered
  with real limits in `RateLimitingExtension.ConfigureFixedWindowPolicies` (fixed — new
  `ContentContributionRateLimitConstants` + `AddPolicy` call), and separately, the integration test
  fixture `ApiFixture.DisableRateLimiting`'s hardcoded policy-name array didn't include the new
  policy either (fixed — every write endpoint in specs 10/11 was returning a bare 500 until both
  were corrected).
- [x] Every new XML doc comment uses the multiline block form (project convention) — spot-checked
  across entities/repositories/errors/constants from every phase, zero collapsed single-line `///`
  summaries found.
- [x] No explicit multi-statement transaction was introduced anywhere in this feature —
  `grep -rn "BeginTransactionAsync|TransactionScope" src/Modules/Content/` returns zero matches;
  every handler's database interaction is expressible as the single-`CommitAsync`-per-step shape
  described in [../00-overview.md](../00-overview.md).

## Summary of drift this audit found and corrected

- **Sort-by-popularity was a dead param** despite its backing counters existing since Phase 5 —
  found and fixed here (`views`/`likes`/`shares` now genuinely order the list/homepage endpoints).
- **Vote-tally auto-accept had a real off-by-one bug** (tally queried before the deciding vote was
  flushed) — found and fixed in both the translation-revision and lyrics-revision vote handlers.
- **Two rate-limiting registration bugs** — the `ContentContribution` policy existed as a named
  constant but wasn't wired into either the real limiter config or the test-disable fixture, so
  every spec 10/11 write endpoint 500'd until both were fixed.
- **A missing test for `GetSimilarAsync`'s standalone-fallback path** — closed.
- Several checklist items referenced stale route paths, a never-built command
  (`AdminLinkLyricsOrderItemCommand`), and a not-yet-built dashboard feature (slug-formula
  generation) — all corrected or explicitly marked out of scope above, rather than silently
  checked off.
