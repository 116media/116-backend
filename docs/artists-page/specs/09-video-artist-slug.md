# Spec 09 — Artist Slug on the Video Detail Response

**Frontend gap 6.** Blocks one entry point into the artist profile
([frontend 03](../../../../frontend/docs/artists-page/03-information-architecture.md)).

The smallest gap in the feature, and independent of every other spec.

## The asymmetry

The lyrics detail response already resolves `ArtistSlug` server-side — spec 02 of the lyrics feature
did that work — so the lyrics page links its artist name straight to `/artistes/{slug}`.

The video detail response does not. `VideoEntity.ArtistId` exists and is populated by
`AdminLinkVideoArtistCommand`, but `VideoDetailDto` carries only the free-text `ArtistName`. So the
video page renders the artist as plain text while the lyrics page renders it as a link, for the same
artist.

That is a half-built loop: users reach the profile from songs but not from videos.

## The fix

`PublicGetVideoBySlugResult` (and its V1 response) gains `string? ArtistSlug`, resolved through
the existing `ArtistId` FK. This mirrors exactly what the lyrics detail response already does —
the slug is a **result-level** field beside the DTO, not a DTO field, because `VideoDetailDto` is
shared with admin surfaces that have no use for a public link target. Same nullable shape, same
fallback rule as `PublicGetLyricsBySlugResult.ArtistSlug`.

**Resolved server-side, not fetched by the client.** The alternative is the frontend issuing a
second request per video page to turn a name into a slug, which is a round trip to answer a question
the server already has the data for.

### Nullable, and the null case is the common one

`ArtistSlug` is null whenever `ArtistId` is null — a video whose artist has no profile yet, which at
launch is most of them. The frontend renders the name as **plain text with no link** in that case
([frontend 03](../../../../frontend/docs/artists-page/03-information-architecture.md)).

`ArtistName` stays on the DTO and stays the display value. The slug is only ever the link target.
This is the same fallback-string-plus-optional-link pattern the module already uses for
`Album`/`AlbumId` and `ArtistName`/`ArtistId`.

Never render a link from `ArtistName` alone by slugifying it client-side: two artists can slugify to
the same string, and a slugified name that has no profile is a 404 dressed as a link.

## Implementation

The handler resolves the slug the same way `PublicGetLyricsBySlugHandler` already does: when
`video.ArtistId` is set, one `IArtistRepository.GetByIdAsync` — a single primary-key lookup on the
detail route, identical to the established lyrics shape. This is a deliberate trade: folding the
artist into the video detail query's `Include` graph would save one indexed PK hit at the cost of
diverging from the sibling handler that answers the same question. Consistency wins; if profiling
ever says otherwise, both handlers move together.

## Checklist

- [x] `PublicGetVideoBySlugResult.ArtistSlug` (`string?`) and the matching V1 response field
- [x] Handler resolves the slug via `GetByIdAsync` when `ArtistId` is set, mirroring the lyrics handler
- [x] Unit: handler emits the slug when the artist is linked, and null when `ArtistId` is null
- [ ] Integration: a video linked to an artist returns the slug on the real endpoint
- [ ] Integration: a video with no linked artist returns `artistSlug: null`
- [ ] `dotnet build` and both test suites clean
