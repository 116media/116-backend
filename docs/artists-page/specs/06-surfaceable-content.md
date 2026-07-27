# Spec 06 — Surfaceable Content: One Predicate, Three Uses

**Frontend gap 1a**, and the design rule underneath gaps 1 and the profile 404
([frontend 16](../../../../frontend/docs/artists-page/16-backend-gaps-and-contracts.md)).

**Depends on [spec 03](03-release-type-discriminator.md), [04](04-artist-scoped-release-query.md)
and [05](05-article-artist-tagging.md)** — the predicate counts albums and tagged articles, so it
cannot be written correctly before those surfaces exist. Written earlier, it gets written twice.

## The problem

Three separate places need the same answer to "does this artist have anything?":

1. **The directory filter** — which artists appear in `GET /public/artists` at all.
2. **`contentCount`** — the number on each directory card.
3. **The profile 404** — an artist with nothing renders no page.

If any two disagree, the site contradicts itself in a way users see immediately:

| Disagreement | What the user gets |
| --- | --- |
| Filter is wider than the 404 rule | An artist is listed, the user clicks, **404**. The worst outcome available here. |
| Filter is narrower than the 404 rule | A real profile exists but is unreachable from the directory — orphaned, and only found by URL. |
| `contentCount` is narrower than the filter | A card reads **`0 contenus`**, which looks like a bug because it is one. |

The failure mode is not that someone writes the wrong rule once. It is that three correct
implementations drift apart over the following year as surfaces land, because each one is edited by
whoever is shipping that surface.

## The rule

**There is exactly one predicate, and the count is its cardinality.** The predicate lives in one
file; the count terms live inline in the two repository projections that apply it. Every future
surface extends the predicate and both projections in the same change.

```
HasSurfaceableContent(artist) ⟺ ContentCount(artist) > 0
```

They are not two rules that happen to agree — the second is the first's cardinality, and they are
implemented next to each other so that stays true.

## What counts

Five surfaces, matching the profile exactly
([frontend 01](../../../../frontend/docs/artists-page/01-overview.md), decision 4):

| Surface | Counted from | Condition |
| --- | --- | --- |
| Songs | `content.lyrics` | `ArtistId = @id AND Status = Published` |
| Videos | `content.videos` | `ArtistId = @id AND Status = Published` |
| Albums | `content.albums` | `ArtistId = @id AND ReleaseType = Album` |
| Mixtapes | `content.albums` | `ArtistId = @id AND ReleaseType = Mixtape` |
| News | `content.article_artists` ⋈ `content.articles` | `ArtistId = @id AND Status = Published` |

Three decisions inside that table are worth stating, because each is a place an implementer would
reasonably guess differently:

- **Albums have no publish state.** `AlbumEntity` has no `Status` — an album row exists or it does
  not. Do not invent a status filter for it.
- **`EP` and `Single` do not count.** The UI renders neither ([spec 03](03-release-type-discriminator.md)),
  and counting content the profile will not show reintroduces exactly the contradiction this spec
  exists to prevent: an artist listed with `contentCount: 1` whose profile renders nothing and
  404s. The album term filters to `Album` and `Mixtape` explicitly, never "not a mixtape".
- **The news term joins through to `articles.Status`.** A join row pointing at a draft article is
  not content. Counting join rows directly is the easy mistake here and produces a profile that
  404s while claiming to have news.

## Implementation

The predicate: `Application/Editorial/Specifications/ArtistContentSpecifications.cs`. The counts:
inlined in `ArtistRepository.GetPublicDirectoryAsync` and `GetTotalsAsync`.

### The predicate

```csharp
/// <summary>
/// Matches artists with at least one item on any surface the public profile renders.
/// Single source of truth for the directory filter, contentCount, and the profile's 404
/// rule — the three must never diverge.
/// </summary>
public class ArtistHasContentSpecification(
    IQueryable<LyricsEntity> lyrics,
    IQueryable<VideoEntity> videos,
    IQueryable<AlbumEntity> albums,
    IQueryable<ArticleArtistEntity> articleArtists
) : Specification<ArtistEntity>
{
    public override Expression<Func<ArtistEntity, bool>> ToExpression()
    {
        return artist =>
            lyrics.Any(l => l.ArtistId == artist.Id && l.Status == EnumContentStatus.Published)
            || videos.Any(v => v.ArtistId == artist.Id && v.Status == EnumContentStatus.Published)
            || albums.Any(a =>
                a.ArtistId == artist.Id
                && (a.ReleaseType == EnumReleaseType.Album || a.ReleaseType == EnumReleaseType.Mixtape))
            || articleArtists.Any(aa =>
                aa.ArtistId == artist.Id && aa.Article.Status == EnumContentStatus.Published);
    }
}
```

Passing the `IQueryable<T>` sets in is what keeps this a *specification* rather than a repository
method: the expression composes into the same SQL statement, and the repository supplies its own
`DbSet`s.

`Any` short-circuits — Postgres stops at the first `EXISTS` that hits, so the common case (an artist
with songs) never touches the other three tables.

### The count projection

The count terms live **inline in the repository**, inside the projections of
`GetPublicDirectoryAsync` and `GetTotalsAsync`, immediately beside the `ApplySpecification`
call that uses the predicate:

```csharp
.Select(a => new ArtistDirectoryRow(
    a,
    context.Lyrics.Count(l => l.ArtistId == a.Id && l.Status == EnumContentStatus.Published)
        + context.Videos.Count(...) + context.Albums.Count(...) + context.ArticleArtists.Count(...)
))
```

They are inline rather than a shared `Expression<Func<ArtistEntity, int>>` for a mechanical
reason: EF Core cannot *invoke* a lambda-typed expression inside a projection — that requires an
expression-expansion library this codebase does not carry, and a helper that exists but cannot be
used inside the query would be dead code wearing the design's name. The alignment guarantee
instead comes from two things: the count terms sit in the same file review as the specification's
call sites, and [spec 10](10-verification-checklist.md)'s term-alignment check plus the
`contentCount`-equals-profile-totals integration assertion catch any drift.

EF Core translates the inlined counts to correlated subqueries in **one statement**. The whole
point.

### Not N+1 — and how that is enforced

This is the highest-risk item in the feature. Computed naively — `foreach (artist) { await
CountAsync(...) }` — a 30-row directory page fires **121 queries** and the directory becomes the
slowest route on the site.

The guard is not a code comment. [Spec 10](10-verification-checklist.md) requires an integration
test that seeds 30 artists, hits the real endpoint, and asserts the executed command count against
a `DbCommandInterceptor`. It is a **Phase-3 exit criterion, not a follow-up** — the N+1 version
works perfectly on a developer's 5-row seed and only fails in production.

The indexes that make the subqueries cheap are in
[`../ARTISTS_FEATURE_SCHEMA.sql`](../ARTISTS_FEATURE_SCHEMA.sql): `(artist_id, status)` on lyrics
and videos, `(artist_id, release_type)` on albums (from spec 03), `(artist_id)` on
`article_artists` (from spec 05). Without them the subqueries are correct and still slow.

## The three call sites

| Use | Where | How it consumes this spec |
| --- | --- | --- |
| Directory filter | `IArtistRepository.GetPublicDirectoryAsync` ([spec 07](07-public-artist-list-endpoint.md)) | `ApplySpecification(hasContent)` in the `WHERE` |
| `contentCount` | Same query | The count expression in the `SELECT` |
| Profile 404 | `PublicGetArtistBySlugHandler` ([spec 08](08-profile-payload-and-totals.md)) | Sums the totals it already computes; 404s at zero |

The third does **not** re-run the predicate. The profile handler already computes a total per
surface for the tab-visibility rules, so it has the same number for free — running the specification
again would be a second round trip to re-derive something already in hand. What matters is that the
totals it sums are the same five surfaces, which [spec 08](08-profile-payload-and-totals.md) states
as its own requirement and [spec 10](10-verification-checklist.md) asserts end-to-end: *an artist
absent from the directory must 404 on the profile, and vice versa.*

## Adding a surface later

The whole design exists to make this a one-file change. When a surface lands:

1. Add its term to `ArtistHasContentSpecification`.
2. Add the matching count term to `GetPublicDirectoryAsync` **and** `GetTotalsAsync`.
3. Add its total to the profile payload ([spec 08](08-profile-payload-and-totals.md)).
4. Add its `(artist_id, …)` index.

**A gap that ships its section without steps 1 and 2** leaves artists correctly rendering that
section while still missing from the directory, or 404-ing. That is the drift this spec prevents,
and it is worth repeating in the PR description of any future surface.

## Checklist

- [x] `ArtistContentSpecifications.cs` created with the predicate; counts inlined in both repository projections
- [x] `ArtistHasContentSpecification` covering all five surfaces
- [x] Album term filters explicitly to `Album` **or** `Mixtape`, never "not mixtape"
- [x] News term joins through to `articles.Status = Published`
- [x] Count terms in `GetPublicDirectoryAsync` and `GetTotalsAsync`, term-for-term aligned with the predicate
- [x] `(artist_id, status)` indexes on lyrics and videos; `(status)` on articles
- [x] Supporting indexes captured in the `AddArtistPageFeature` migration
- [x] Unit: the 404 rule accepts each of the five surfaces alone and rejects all-zero totals (handler-level, since the predicate itself only executes against a real database)
- [ ] Integration: an artist with only a draft article is absent from the directory
- [ ] Integration: an artist with only an `EP` is absent from the directory **and** 404s on the profile
- [ ] Integration: `contentCount` on a card equals the number of items the profile actually renders
- [ ] Integration: **query-count assertion** — 30 artists in the directory execute a bounded number of commands, not one per row
- [ ] `dotnet build` and both test suites clean
