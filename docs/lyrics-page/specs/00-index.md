# Lyrics Page — Backend Implementation Specs

Read [../00-overview.md](../00-overview.md) first for the *why* and the current-state audit. This
index is the *how* — start here, work in the order below. For the full SQL shape of every table
these specs introduce, see [../LYRICS_FEATURE_SCHEMA.sql](../LYRICS_FEATURE_SCHEMA.sql).

| # | File | Covers |
| --- | --- | --- |
| 01 | [01-slug-and-public-list-endpoint.md](01-slug-and-public-list-endpoint.md) | `Slug` column + migration, admin commands updated, `PublicGetPublishedLyricsQuery` (list, search, language filter, sort) |
| 02 | [02-video-and-artist-slug-resolution.md](02-video-and-artist-slug-resolution.md) | The by-slug detail lookup resolves `VideoSlug` and `ArtistSlug` server-side |
| 03 | [03-song-metadata-and-cover-image.md](03-song-metadata-and-cover-image.md) | `CoverImageFileId`, `Album`, `ReleaseYear`, `Label`, `Songwriter`, `Producer` |
| 04 | [04-view-like-share-interactions.md](04-view-like-share-interactions.md) | `ViewCount`/`LikeCount`/`ShareCount`, `LyricsLikeEntity`/`LyricsShareEntity`/`LyricsViewEventEntity`, four endpoints |
| 05 | [05-read-time-view-algorithm.md](05-read-time-view-algorithm.md) | Server-side expected-reading-time computation gating the view counter |
| 06 | [06-similar-lyrics-query.md](06-similar-lyrics-query.md) | `GetSimilarAsync` — video category → shared tags → latest standalone waterfall |
| 07 | [07-tags-for-lyrics.md](07-tags-for-lyrics.md) | `LyricsTagEntity` join, reusing the existing `TagEntity` |
| 08 | [08-artist-and-album-entities.md](08-artist-and-album-entities.md) | `ArtistEntity`, `AlbumEntity`, claim/verification, `LyricsEntity.ArtistId`/`AlbumId` |
| 09 | [09-streaming-links-and-album-tracks.md](09-streaming-links-and-album-tracks.md) | `StreamingLinkEntity`, curated-or-generated platform URLs, "more from this album" |
| 10 | [10-ai-translations-and-community-review.md](10-ai-translations-and-community-review.md) | `LyricsTranslationEntity` + revision/vote review workflow |
| 11 | [11-community-submissions-and-corrections.md](11-community-submissions-and-corrections.md) | New-song submissions, canonical-text corrections, verified-artist fast path |
| 12 | [12-monetization-and-promoted-placement.md](12-monetization-and-promoted-placement.md) | Advertising (no work), streaming-affiliate links, promoted placement — reusing the existing Commerce module |
| 13 | [13-homepage-discovery-endpoints.md](13-homepage-discovery-endpoints.md) | 116-Lyrics video category rail, "Top Lyrics" sort param, "New Lyrics" |
| 14 | [14-verification-checklist.md](14-verification-checklist.md) | Full backend test/verification sweep across every spec above |

## Global progress

- [x] 01 — Slug column + public list endpoint
- [x] 02 — Video/artist slug resolution (VideoSlug and ArtistSlug both fully resolve now)
- [x] 03 — Song metadata & cover image
- [x] 04 — View/like/share interactions
- [x] 05 — Read-time view algorithm
- [x] 06 — Similar lyrics query
- [x] 07 — Tags for lyrics
- [x] 08 — Artist & album entities
- [x] 09 — Streaming links & album tracks
- [x] 10 — AI translations & community review
- [x] 11 — Community submissions & corrections
- [x] 12 — Monetization: advertising, streaming affiliate, promoted placement
- [x] 13 — Homepage discovery endpoints (`sort=views|likes|shares` now fully wired, fixed during spec 14's audit)
- [x] 14 — Verification

Mark a box `- [x]` only once that spec's own checklist is fully implemented, its tests pass, and
`dotnet build`/the module's test suite are clean.
