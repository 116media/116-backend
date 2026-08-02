# Lyrics Page — Backend Design & Implementation Docs

Full backend design for the public lyrics feature described on the frontend side in
[`apps/frontend/docs/lyrics-page/`](../../../frontend/docs/lyrics-page/README.md). That folder
covers the frontend module, routes, and product scope; this folder is the backend's own home for
the same feature — every entity, migration, endpoint, and algorithm needed to support it, grounded
in the `Content` module's actual current code (`src/Modules/Content/Content/`), not a hypothetical
rewrite.

This is **documentation only** — nothing here has been implemented yet. Specs are
implementation-ready: real namespaces, real base classes (`Aggregate<Guid>`, `Specification<T>`,
`ICommandHandler`/`IQueryHandler`), real existing constants and patterns, copied and extended from
the actual current codebase, not invented from scratch.

**[LYRICS_FEATURE_SCHEMA.sql](LYRICS_FEATURE_SCHEMA.sql)** is the single SQL reference for
every table/column/constraint introduced across specs 01–13 — a companion to the repo's own
[`../CONTENT_SCHEMA.sql`](../CONTENT_SCHEMA.sql), same conventions (schema `content`, `snake_case`,
`uq_`/`chk_`/`fk_`/`ix_` naming), written as a delta `ALTER`/`CREATE` script rather than a
full-database dump. Read it alongside the specs below, not instead of them — the SQL is the *what*,
the specs are the *why* and the C# *how*.

## What exists today

`LyricsEntity` (`Domain/Entities/LyricsEntity.cs`) is a lean aggregate: `SongTitle`, `ArtistName`,
`LyricsText`, `Language` (ISO 639-1, default `"fr"`), an optional `VideoId` link, SEO fields
(`MetaTitle`, `MetaDescription`, `StructuredData`), and `AuthorId` (the CMS editor who entered the
record — not the songwriter, see the naming note in spec 03). No `Slug` column, no cover image, no
song credits beyond `ArtistName`, no view/like/share counters, and — unlike articles and videos —
**no draft/review/publish workflow at all**: every lyrics record is public the instant it's
created. That last point does not hold up once community submissions and AI translations exist
(unreviewed content must not be public by default), so spec 01 gives `LyricsEntity` the same
`Draft → PendingReview → Approved → Published`/`Rejected`/`Archived` workflow articles and videos
already have. `ArtistName` is also a plain string today, not a link to any artist profile —
spec 08 adds a real `ArtistEntity` and a nullable `LyricsEntity.ArtistId` FK specifically so a
claimed artist can be displayed, browsed by their catalog, and linked to from a lyrics page.

Existing endpoints, all in `Application/Editorial/UseCases/`:

| Method | Route | Handler | Notes |
| --- | --- | --- | --- |
| GET | `/api/v1/admin/lyrics` | `AdminGetAllLyricsHandler` | Paginated, `search` param, resolves `Author` async |
| POST | `/api/v1/admin/lyrics` | `AdminCreateLyricsHandler` | Rejects duplicate `(SongTitle, ArtistName)` pairs |
| PUT | `/api/v1/admin/lyrics/{id}` | `AdminUpdateLyricsHandler` | |
| PUT | `/api/v1/admin/lyrics/{id}/seo` | `AdminUpdateLyricsSeoHandler` | |
| DELETE | `/api/v1/admin/lyrics/{id}` | `AdminDeleteLyricsHandler` | |
| GET | `/api/v1/public/lyrics/{songTitle}/{artistName}` | `PublicGetLyricsBySlugHandler` | **Not slug-based despite the name** — exact `ILIKE` match against the raw, unslugified `SongTitle`/`ArtistName` columns via `LyricsBySongAndArtistSpecification` |
| GET | `/api/v1/public/lyrics/by-video/{videoId}` | `PublicGetLyricsByVideoIdHandler` | Backs the video module's embedded lyrics tab |

Both public lookups call the synchronous `entity.ToLyricsDto(mapper)` extension (not the async,
`IUserLookupService`-backed `ToLyricsDtoAsync`) — `Author` is always `null` on the public surface.
This is correct, not a gap: `AuthorId` is CMS attribution, never a songwriter credit — see spec 03.

## Full scope of this backend design

Twelve additions, specced in full in [specs/](specs/00-index.md):

1. **A real, stored `Slug` column** replacing the misleading two-segment route
   ([specs/01](specs/01-slug-and-public-list-endpoint.md)).
2. **A public list endpoint** with search + language filter — there is no way to *browse* lyrics
   today, only look one up by exact key ([specs/01](specs/01-slug-and-public-list-endpoint.md)).
3. **Video-slug and artist-slug resolution** on the detail lookup, so the frontend never needs a
   second fetch for either cross-link ([specs/02](specs/02-video-and-artist-slug-resolution.md)).
4. **Cover image + song credits** (album, release year, label, songwriter, producer) — named
   `Songwriter`/`Producer`, deliberately not `Author`, to avoid colliding with the existing CMS
   `AuthorId` field ([specs/03](specs/03-song-metadata-and-cover-image.md)).
5. **View, like, and share counters** — a full interaction system mirroring `ShortVideoEntity`'s
   exactly ([specs/04](specs/04-view-like-share-interactions.md)).
6. **A read-time-based view-counting algorithm** — a view counts only after the visitor plausibly
   read the lyrics, not on page load alone ([specs/05](specs/05-read-time-view-algorithm.md)).
7. **A similar-lyrics query** — same video category when linked, else shared tags, else the
   10 latest standalone records ([specs/06](specs/06-similar-lyrics-query.md)).
8. **Tags on lyrics** — reusing the *existing* `TagEntity`/tag-CRUD infrastructure that already
   backs `ArticleTagEntity`/`VideoTagEntity`, not a new tag system
   ([specs/07](specs/07-tags-for-lyrics.md)).
9. **Artist and Album entities** — real, addressable domain objects (there is none today;
   `ArtistName`/`Album` are plain strings) needed for artist pages, album-mates cards, and
   streaming links ([specs/08](specs/08-artist-and-album-entities.md)).
10. **Streaming-platform links + "more from this album"**
    ([specs/09](specs/09-streaming-links-and-album-tracks.md)).
11. **AI-generated translations with Wikipedia-style community review**
    ([specs/10](specs/10-ai-translations-and-community-review.md)).
12. **Community-submitted new lyrics and corrections to existing lyrics**, plus a verified-artist
    fast path that skips the review queue entirely
    ([specs/11](specs/11-community-submissions-and-corrections.md)).

Plus monetization — advertising (no backend work), streaming-affiliate links (an extension of
spec 09, not a new endpoint), and label/artist-paid promoted placement, which **reuses the existing
Commerce module** (`ContentOrderEntity`/`ContentOrderItemEntity`/`ContentPaymentEntity`/
`PromotionLevelEntity`, already live for articles and videos) rather than inventing new commerce
infrastructure ([specs/12](specs/12-monetization-and-promoted-placement.md)) — and the three
homepage discovery endpoints — lyrics videos rail, "Top Lyrics" tabs, "New Lyrics"
([specs/13](specs/13-homepage-discovery-endpoints.md)).

## Conventions every spec follows

- **Namespaces**: everything lives under `_116.Content.*`, following the existing module's own
  layering (`Domain/Entities`, `Application/Editorial/...` or a new `Application/Community/...` /
  `Application/Monetization/...` sub-module, `Infrastructure/Persistence/Configurations`,
  `Infrastructure/Repositories`).
- **Naming**: every use-case type is prefixed `Admin`/`Public` per file, per this codebase's
  established rule (`CLAUDE.md`) — `PublicLikeLyricsCommand`, not `LikeLyricsCommand`.
- **XML docs**: always the multiline block form, never collapsed to one line — enforced project
  convention.
- **CQRS**: `ICommand<TResult>`/`IQuery<TResult>` + `ICommandHandler`/`IQueryHandler`, dispatched
  via `IDispatcher`, exactly like every existing handler in this module.
- **Specification pattern**: every new query filter is a `Specification<T>` in
  `Application/Editorial/Specifications/` (or a new specifications folder for new aggregates),
  combined via `.And()`/`.Or()` — never raw LINQ conditionals scattered in handlers.
- **Errors**: a dedicated `*Errors` factory class per aggregate (mirroring `LyricsErrors`,
  `ShortVideoInteractionErrors`), wired into the single `ContentI18n` facade — never a raw
  `throw new Exception(...)`.
- **Migrations**: `dotnet ef migrations add <Name> --project src/Modules/Content/Content
  --startup-project src/Api --context ContentDbContext`.
- **Rate limiting**: every new public read endpoint uses `RateLimitPolicies.ContentBrowsing`
  (existing); every new authenticated write endpoint (translations, submissions, votes) uses a new
  `RateLimitPolicies.ContentContribution` policy, stricter than browsing, added once and reused
  across all of them.
- **No explicit multi-statement transactions.** Every write is a single aggregate mutation per
  `SaveChangesAsync`/`unitOfWork.CommitAsync()` call — exactly how `LikeShortVideoHandler` /
  `ShareShortVideoHandler` already work (add one row, mutate one cached counter, commit once).
  Multi-step flows (accept a translation revision, approve a community submission) are designed as
  two independently-safe, idempotent steps rather than reaching for a distributed transaction — see
  the ACID note in [specs/11](specs/11-community-submissions-and-corrections.md). This codebase has
  no existing `IDbContextTransaction` usage to mirror, and none is introduced here.
- **Task checklists**: every spec ends with an unchecked `- [ ]` list; a box is ticked only once
  that piece is implemented and its own tests pass.

## Implementation order

1. **Spec 01** (slug + public list) — nothing else needs this to exist, but everything downstream
   assumes the slug and list endpoint are in place.
2. **Spec 02** (cross-link resolution) — depends on spec 01's slug and spec 08's artist slug.
3. **Specs 03, 04, 07** (metadata/cover, interactions, tags) — independent of each other and of
   05/06, can land in parallel.
4. **Spec 05** (read-time algorithm) — extends spec 04's view-recording command; land right after
   or together with it.
5. **Spec 06** (similar lyrics) — depends on spec 07's tags existing.
6. **Spec 08** (artist/album) — independent, can land any time; specs 02 and 09 depend on it.
7. **Spec 09** (streaming links/album tracks) — depends on spec 08.
8. **Specs 10, 11** (translations, submissions/corrections) — independent of each other, depend
   only on spec 01 (the base `LyricsEntity` + slug) existing.
9. **Spec 12** (monetization) — depends only on the existing Commerce module (already live); no
   dependency on specs 01–11.
10. **Spec 13** (homepage endpoints) — depends on spec 01 (sort param) and the existing category
    system (no schema change needed for the lyrics-videos rail).
11. **Spec 14** (verification) — full sweep, after everything above lands.
