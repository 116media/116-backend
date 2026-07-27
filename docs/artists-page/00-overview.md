# Artist Page — Backend Overview

The frontend design for the artist directory and artist profile is complete and lives in
[`../../../frontend/docs/artists-page/`](../../../frontend/docs/artists-page/). That doc set is
**frontend-led**: it decides the pages, then states what the backend must return
([14](../../../frontend/docs/artists-page/14-data-requirements.md)) and where today's backend falls
short ([16](../../../frontend/docs/artists-page/16-backend-gaps-and-contracts.md)).

This document is the backend's answer. It audits what actually exists in `src/` today, then
[`specs/`](specs/) turns each gap into an implementable spec with its own checklist.

**Nothing here is deferred.** The frontend doc set already did the deferring — merch, tour dates and
featured/guest credits were cut from v1 outright, and the three most expensive backend items went
with them. What remains is eight gaps, all of which are specified and implemented in full.

## What the pages need

Two routes, both public, both server-rendered:

| Route | What it is |
| --- | --- |
| `/artistes` | Alphabetical artist directory: A–Z rail, name search, paginated grid, per-artist content count |
| `/artistes/{slug}` | Artist profile: hero, identity block, social row, and three tabs — `Musique` (popular songs → latest songs → albums → mixtapes), `Vidéos`, `Actualités` |

Five content surfaces feed the profile: **songs, videos, albums, mixtapes, news**. Six sections,
because songs appear twice (ranked by views, and by publish date).

## Current-state audit

The lyrics feature already built more of this than the gap list suggests. Verified against `src/`,
not assumed:

### Already exists — no work required

| Capability | Where |
| --- | --- |
| `ArtistEntity` aggregate with `Name`, `Slug`, `Bio`, `AvatarFileId`, `UserId`, `VerifiedAt` | `Domain/Entities/ArtistEntity.cs` |
| Slug immutability after creation | `ArtistEntity.Update` never touches `Slug` |
| Claim/verification (`ClaimOwnership`, `AlreadyClaimed` guard, unique `UserId`) | `ArtistEntity.cs`, `AdminVerifyArtistOwnerCommand` |
| `AlbumEntity.ArtistId` — the artist→album relationship | `Domain/Entities/AlbumEntity.cs` |
| `LyricsEntity.ArtistId` / `VideoEntity.ArtistId` + `LinkArtist`/`UnlinkArtist` | both entities |
| Public profile endpoint `GET /public/artists/{slug}` returning artist + paginated lyrics + videos | `PublicGetArtistBySlugEndpointV1` |
| `GetPublishedByArtistAsync` on the lyrics and video repositories | both repositories |
| **Popular songs** — `ViewCount`/`LikeCount`/`ShareCount` and `sort=views\|likes\|shares` | `LyricsEntity`, `ILyricsRepository.GetPublishedAsync` |
| Admin artist CRUD, avatar upload, artist↔lyrics/video linking | `Editorial/UseCases/Admin/Commands/*Artist*` |
| Public claim request | `PublicRequestArtistClaimCommand` |
| `ArtistDto`, `ArtistMapper`, `IArtistRepository`, `ArtistSpecifications` | `Application/Shared/` |

The frontend's own audit reached the same conclusion on the one that matters most: **popular songs
needs no backend gap at all.** The sort parameter already ships.

### Missing — the eight gaps

| # | Gap | Kind | Spec |
| --- | --- | --- | --- |
| 1 | Public artist list endpoint (directory, letter bucket, search, `availableLetters`) | new endpoint + derived columns | [07](specs/07-public-artist-list-endpoint.md) |
| 2 | `isVerified` on artist DTOs | DTO + mapper | [08](specs/08-profile-payload-and-totals.md) |
| 3 | Artist-scoped release query | repository + endpoint, **no migration** | [04](specs/04-artist-scoped-release-query.md) |
| 4 | Release-type discriminator (album vs mixtape) | one column + admin surface + backfill | [03](specs/03-release-type-discriminator.md) |
| 5 | Article → artist link | join table + admin tagging + endpoint | [05](specs/05-article-artist-tagging.md) |
| 6 | `ArtistSlug` on the video detail response | DTO field | [09](specs/09-video-artist-slug.md) |
| 7 | Artist identity fields (real name, aliases, birthdate, hometown) | four columns + admin surface | [01](specs/01-artist-identity-fields.md) |
| 8 | Artist social links | child table + enum + admin surface | [02](specs/02-artist-social-links.md) |

Plus one item the frontend states as a *rule* rather than a gap, which is nonetheless the single
most load-bearing piece of backend design here:

| Item | Kind | Spec |
| --- | --- | --- |
| The `HasSurfaceableContent` predicate and `contentCount`, as **one** definition used three times | specification + projection | [06](specs/06-surfaceable-content.md) |

## The two decisions that shape everything else

### 1. "Has content" is one predicate, not three implementations

[Frontend 16](../../../frontend/docs/artists-page/16-backend-gaps-and-contracts.md) is emphatic
about this, and it is right. Three separate places need the same answer:

1. the directory's filter — which artists are listed at all,
2. `contentCount` on each directory card,
3. the profile's 404 rule — an artist with nothing renders no page.

If those three ever disagree, the site contradicts itself in the worst possible way: an artist is
listed, the user clicks, and gets a 404. Or an artist is listed showing `0 contenus`.

So there is exactly one `ArtistHasContentSpecification` and one counting projection, both built from
the same surface list, in [spec 06](specs/06-surfaceable-content.md). Every later surface extends
that one place.

### 2. Accent folding is a stored column, not `unaccent`

`Élodie` must sort under `E`, bucket under `E`, and match a search for `elodie`. The obvious move is
the Postgres `unaccent` extension. This design does not use it, and the reason is worth stating
once:

- `unaccent()` is **not `IMMUTABLE`** (it depends on a mutable dictionary), so it cannot be used in
  a generated column or a plain expression index without wrapping it in a hand-written `IMMUTABLE`
  wrapper function — which is a lie to the planner and a documented footgun.
- It requires `CREATE EXTENSION`, which is a deployment-environment dependency this module has
  nowhere else.
- Sorting and bucketing would each call it separately, which is exactly the "same rule computed in
  two places" failure the previous decision exists to prevent.

Instead the domain computes and stores the folded form once, on create and on rename:
`NameFolded` (uppercase, accent-stripped) and `InitialLetter` (`A`–`Z`, or `#`). Both are plain
indexed columns. Sorting, bucketing, searching and `availableLetters` all read the same stored
value, the folding rule is unit-testable without a database, and there is no extension to install.

Full reasoning and the exact algorithm: [spec 07](specs/07-public-artist-list-endpoint.md).

## What this feature does *not* add

Stated so nobody builds them:

- **No merch, product, price, or commerce shape of any kind.** Cut from v1
  ([frontend 01](../../../frontend/docs/artists-page/01-overview.md)).
- **No tour dates, events, venues or ticket links.** Same.
- **No credits join table.** `LyricsEntity.ArtistId` / `VideoEntity.ArtistId` already mean *primary
  credit*, which is exactly what the profile shows. The join table existed only to serve the
  `Featured on` section, which was cut.
- **No artist ranking.** The directory is alphabetical, always. The *profile* ranks its own songs by
  `ViewCount`, which already works.
- **No artist `Id` in any public response.** Everything public is slug-addressed.
- **No `UserId` in any public response.** Who claimed a profile is not public information — only the
  derived `isVerified` flag.

## Where to go next

[`specs/00-index.md`](specs/00-index.md) — the ordered spec list and the global progress checklist.
The order there is a real dependency chain, not a preference.

The full SQL shape of every table and column these specs introduce:
[`ARTISTS_FEATURE_SCHEMA.sql`](ARTISTS_FEATURE_SCHEMA.sql).
