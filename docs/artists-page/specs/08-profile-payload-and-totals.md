# Spec 08 — Profile Payload, `isVerified` and Surface Totals

**Frontend gap 2**, plus the payload contract the whole profile route depends on
([frontend 21](../../../../frontend/docs/artists-page/21-performance-and-pagination.md)).

**Depends on [spec 06](06-surfaceable-content.md)** for the 404 rule, and on specs 01–05 for the
fields and surfaces it returns.

`GET /api/v1/public/artists/{slug}` already exists and returns the artist plus paginated lyrics and
videos. This spec extends it to everything the profile needs on first paint, and adds the 404 rule.

## Gap 2 — `isVerified`

`ArtistDto` exposes no verification state, so the badge cannot render.

```csharp
isVerified = artist.UserId is not null && artist.VerifiedAt is not null
```

Derived in `ArtistMapper`, from data already on the entity. No column, no migration.

**`UserId` is never exposed.** Who claimed a profile is not public information, and the client needs
only the derived flag ([frontend 14](../../../../frontend/docs/artists-page/14-data-requirements.md)).
The temptation is to return `userId` and let the client compare it to `null` — that leaks the
identity of every claimed artist to anyone who opens devtools, to save one line in a mapper.

Both conditions are required. `UserId` set with `VerifiedAt` null means a claim is in flight, not
verified — the request/verify split already exists via `PublicRequestArtistClaimCommand` and
`AdminVerifyArtistOwnerCommand`.

Applies to `ArtistDto` and to `ArtistSummaryDto` ([spec 07](07-public-artist-list-endpoint.md)), so
the badge renders identically on a card and on a profile.

The verified badge **never influences ordering or ranking anywhere**
([frontend 22](../../../../frontend/docs/artists-page/22-verified-artists-and-claiming.md)). The
directory sorts by folded name and nothing else. Stated here because a "verified first" tie-break is
exactly the kind of thing that gets added later as an obvious improvement.

## The payload

```csharp
public record PublicGetArtistBySlugResult(
    ArtistDto Artist,
    ArtistTotalsDto Totals,
    PaginatedResult<LyricsSummaryDto> Lyrics,
    PaginatedResult<VideoSummaryDto> Videos
);

public record ArtistTotalsDto(int Songs, int Videos, int Albums, int Mixtapes, int News);
```

`ArtistDto` now carries the identity fields ([spec 01](01-artist-identity-fields.md)), the social
links ([spec 02](02-artist-social-links.md)) and `IsVerified`. `Lyrics` and `Videos` stay as they
are — the two surfaces already in the payload.

### Every total ships, even for surfaces whose data does not

This is the load-bearing decision in the payload.

The frontend needs all five totals **before any tab is opened**, for three things: the hero stat
row, tab visibility (a tab with zero items is hidden, never disabled), and default-tab resolution —
the first non-empty tab in `music → videos → news`
([frontend 07](../../../../frontend/docs/artists-page/07-artist-detail-tabs.md)).

If totals were deferred to each tab's own request, the server could not resolve the default tab and
the user would get a **flash of the wrong panel** on every profile load. A total is one `COUNT`
against an index. Five counts to avoid a visible layout flash on the site's most-linked route is a
trade worth making, and it is the same shape of trade [spec 06](06-surfaceable-content.md) already
makes for the directory.

**Albums and mixtapes are separate totals**, not one `releases` number, because they are two
sections with two headings and each hides independently.

### What is *not* in the payload

Only `Récentes` (latest songs) and `Vidéos` ride along. Popular songs, albums, mixtapes and news
each fetch on their own request when their section becomes visible.

That is deliberate and matches [frontend 21](../../../../frontend/docs/artists-page/21-performance-and-pagination.md):
a single endpoint returning every section would block the whole profile on the slowest surface, and
would have to grow a new paginated envelope every time a surface lands. Each section paginates
independently, so each needs its own request anyway — folding page 1 into this payload would mean
two code paths for one list.

## The 404 rule

**An artist with zero items across all five surfaces returns 404.**

Locked in [frontend 01](../../../../frontend/docs/artists-page/01-overview.md), decision 4. The
reason is not tidiness: unclaimed staff-curated stubs are created as a side effect of tagging a
song, and they are real rows. Serving them as pages produces a directory padded with empty profiles
and, worse, **crawlable dead pages** carrying a real person's name in the `<title>`.

Implementation, in `PublicGetArtistBySlugHandler`:

```csharp
if (totals.Songs + totals.Videos + totals.Albums + totals.Mixtapes + totals.News == 0)
{
    throw i18n.Artist.NotFound(id: artist.Id);
}
```

It sums the totals it has just computed rather than re-running
`ArtistHasContentSpecification` — the numbers are already in hand and a second round trip would
re-derive them. What makes this safe is that **the totals cover the same five surfaces as the
predicate**. That equivalence is the contract, and [spec 10](10-verification-checklist.md) asserts
it end-to-end in both directions:

- an artist absent from the directory **must** 404 on the profile,
- an artist present in the directory **must** return 200.

A bio and an avatar are **not** content. An editorial stub with a nice biography and nothing else
still 404s. This is the row that catches a wrong implementation, and it is an explicit test case.

### Ordering matters

The 404 check runs **after** the totals are computed and **before** anything is mapped. Mapping
first wastes file-URL resolution on a response that is about to be discarded; checking before the
artist is loaded is impossible.

`generateMetadata` on the frontend calls the same endpoint and gets the same 404, so a 404 page
never carries the artist's real name in its `<title>` — which would tell a crawler the page exists.
That only works because the rule lives here, in the endpoint, not in the page component.

## Totals and the profile query

The handler needs five counts plus two first pages. Implemented as:

```csharp
Task<ArtistTotalsDto> GetTotalsAsync(Guid artistId, CancellationToken ct = default);
```

on `IArtistRepository` — one method, one round trip, the five counts projected together in a single
statement exactly as the directory does it. Five separate `CountAsync` calls would be five round
trips on the most-linked route on the site.

**The terms must stay aligned with [spec 06](06-surfaceable-content.md).** Same surfaces, same
status filters, same `Album`/`Mixtape`-only album rule. A future surface adds a term in both places
or the two disagree — which is the drift the whole design is built to prevent.

## Checklist

- [x] `ArtistMapper` derives `IsVerified` from `UserId is not null && VerifiedAt is not null`
- [x] `ArtistDto.IsVerified`; `UserId` exposed nowhere public
- [x] `ArtistTotalsDto` with the five totals
- [x] `IArtistRepository.GetTotalsAsync` + implementation — one statement, terms aligned with spec 06
- [x] `PublicGetArtistBySlugResult` gains `Totals`
- [x] `PublicGetArtistBySlugResponse` gains `Totals`
- [x] Handler computes totals, applies the 404 rule **before** mapping, then maps
- [x] Handler returns identity fields and social links on `ArtistDto`
- [x] Unit: `IsVerified` is false when `UserId` is null, false when `VerifiedAt` is null, true when both are set
- [x] Unit: handler throws `NotFound` when every total is zero
- [x] Unit: handler returns 200 when exactly one total is non-zero, for each of the five surfaces
- [x] Unit: handler throws `NotFound` for an artist with a bio and an avatar but no content
- [ ] Integration: profile response carries identity fields, social links, `isVerified`, and all five totals
- [ ] Integration: an artist with only one album returns 200 with `albums: 1` and every other total 0
- [ ] Integration: an artist with only a draft song returns 404
- [ ] Integration: **cross-check** — every artist returned by the directory returns 200 on its profile
- [ ] Integration: **cross-check** — an artist absent from the directory returns 404 on its profile
- [ ] Integration: `userId` appears nowhere in the public response body
- [ ] `dotnet build` and both test suites clean
