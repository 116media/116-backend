# Spec 13 — Homepage Discovery Endpoints

Covers the three `/lyrics` homepage sections from
[../../../frontend/docs/lyrics-page/06-musixmatch-scale-expansion.md](../../../frontend/docs/lyrics-page/06-musixmatch-scale-expansion.md) §1:
a "116 Lyrics" video rail, "Top Lyrics" tabs, and "New Lyrics." All three reuse existing or
already-specced endpoints — no new domain concepts, only query-shape additions.

## a. Lyrics videos rail — no backend change at all

"116 Lyrics" is an ordinary `CategoryEntity` row — the same table that already backs every other
video category (`116 Le Focus`, `116 Music Video`, etc.), created once by an admin through the
existing category CRUD, with videos tagged into it exactly as any video is tagged into any
category. The homepage section pulls from the **existing**
`GET /api/v1/public/videos?categoryId={id}` endpoint — nothing new to build here. The category's id
is a config value the frontend resolves per environment, not a schema change.

## b. "Top Lyrics" — sort param on the existing list endpoint

Already specced in [01-slug-and-public-list-endpoint.md](01-slug-and-public-list-endpoint.md) §5:
`GET /api/v1/public/lyrics?sort=views|likes|shares&pageSize=10`. No second endpoint — the homepage
section is the same `PublicGetPublishedLyricsQuery`/`Handler` the full listing page uses, called
three times (once per tab) with a fixed small page size and no further pagination.

One addition needed here specifically: **promoted records (spec 12) must never silently blend into
this ranking.** `LyricsQueryBuilder`'s sort switch (spec 01) sorts strictly by the raw counter —
`IsPromoted` is not a sort input. If a promoted-placement UI is ever layered on top of this section,
it renders as a visually distinct slot the frontend adds separately, never by this endpoint
reordering organic results. No backend change is required to guarantee this — it's already true by
construction, since `IsPromoted` isn't part of the `OrderByDescending` switch — called out here
explicitly so it's never "fixed" by someone adding it to the sort later without knowing why it was
deliberately left out.

## c. "New Lyrics" — default sort, sliced

`GET /api/v1/public/lyrics?pageSize=10` with no `sort` param — `LyricsQueryBuilder`'s default
(`OrderByDescending(l => l.CreatedAt)`, spec 01) is already exactly "newest first." No new query
shape; the frontend simply doesn't paginate past the first 10 for this rail.

## Why no new endpoints were needed

This spec exists mainly to document that the "Musixmatch-scale" homepage does **not** require new
backend surface area beyond spec 01's `sort` param — a useful thing to state explicitly, since a
reader skimming the frontend's homepage description might otherwise assume three new endpoints are
needed. All three sections are thin frontend compositions over existing/already-specced reads.

## Task checklist

- [x] `PublicGetPublishedLyricsQuery`/`Handler`/`EndpointV1` ships with a `sort` param. Landed in
  two stages: Phase 4 shipped `newest`/default only (`LyricsEntity` had no interaction counters
  yet) and fixed a real, live bug found during planning — `LyricsRepository.GetAllAsync`
  previously always sorted `OrderBy(SongTitle)` (alphabetical) regardless of any params, so "New
  Lyrics" never actually showed the newest songs first. Phase 5 added `ViewCount`/`LikeCount`/
  `ShareCount` to `LyricsEntity`, and this doc's original `views`/`likes`/`shares` sort values were
  **finally wired up during the spec 14 verification audit** — the counters had existed since
  Phase 5 but were never actually connected to the sort switch until this pass (`GetAllAsync`'s
  `sort switch` now has real `"views"`/`"likes"`/`"shares"` branches, each `OrderByDescending` on
  the matching counter with `CreatedAt` as tiebreaker for `views`).
- [x] Confirm `IsPromoted` (spec 12) is excluded from every sort branch — unit + integration tests
  assert this for the default/`newest` branch (a promoted-but-older record doesn't jump ahead) AND
  for the `views` branch specifically (a promoted-but-fewer-views record doesn't jump ahead of a
  non-promoted, more-viewed one) — so a future change can't silently reintroduce blended ranking
  in any sort mode without deliberately updating these tests
- [x] No migration, no new entities, no new routes beyond the `sort` query param addition to the
  already-existing list endpoint

**Verification, 2026-08-01**: `dotnet build` clean; full suite after the sort-by-popularity fix —
6673/6676 unit (3 pre-existing unrelated skips), 1673/1673 integration, zero failures. All three
sort values (`views`, `likes`, `shares`) confirmed to order strictly by their respective counter,
independent of recency, with a dedicated promotion-guard test per sort mode.
