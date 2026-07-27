# Spec 04 — Artist-Scoped Release Query

**Frontend gap 3.** Blocks the Albums and Mixtapes sections
([frontend 08](../../../../frontend/docs/artists-page/08-catalog-sections.md)).

**Depends on [spec 03](03-release-type-discriminator.md)** — the type filter needs the
discriminator.

## This is not a migration

The frontend gap list originally called this a schema change and **corrected itself**:
`AlbumEntity.ArtistId` (nullable `Guid`) already exists and has since the lyrics feature shipped.
Verified in `Domain/Entities/AlbumEntity.cs`.

What is missing is only the **query path**: `IAlbumRepository` has `GetByIdAsync`,
`GetByIdOrThrowAsync` and a paginated `GetAllAsync(page, pageSize, search)`, and nothing that scopes
to an artist. There is no public endpoint returning an artist's releases at all.

So this is a repository method plus an endpoint — the cheapest of the eight gaps, and the one that
should land first among the query-side work.

## Specification

`Application/Editorial/Specifications/AlbumSpecifications.cs` gains:

```csharp
public class AlbumByArtistSpecification(Guid artistId) : Specification<AlbumEntity>
{
    public override Expression<Func<AlbumEntity, bool>> ToExpression() => album => album.ArtistId == artistId;
}

public class AlbumByReleaseTypeSpecification(EnumReleaseType releaseType) : Specification<AlbumEntity>
{
    public override Expression<Func<AlbumEntity, bool>> ToExpression() => album => album.ReleaseType == releaseType;
}
```

Two specifications composed with `.And(...)` rather than one taking both arguments. The artist scope
is reused on its own by [spec 06](06-surfaceable-content.md)'s album count, which does **not**
filter by type — an artist with only a mixtape still has content. One combined specification would
force that caller to pass a type it does not want to filter on.

## Repository

`IAlbumRepository` gains:

```csharp
/// <summary>
/// Retrieves a paginated page of an artist's releases of a given type, newest first.
/// </summary>
Task<(List<AlbumEntity> Albums, int TotalCount)> GetByArtistAsync(
    Guid artistId,
    EnumReleaseType releaseType,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default
);
```

**Ordering: `ReleaseYear` descending, then `Name` ascending.** Newest release first, and a
deterministic tie-break so two releases from the same year do not swap places between page loads —
an unstable sort under pagination silently duplicates and drops rows across page boundaries.

`ReleaseYear` is nullable. Rows with no year sort **last**, not first: Postgres puts `NULL` first
under `DESC` by default, which would head an artist's discography with the records nobody bothered
to date. Express it explicitly:

```csharp
.OrderBy(a => a.ReleaseYear == null)
.ThenByDescending(a => a.ReleaseYear)
.ThenBy(a => a.Name)
```

## Endpoint

`GET /api/v1/public/artists/{slug}/releases`

| Param | Type | Default | Notes |
| --- | --- | --- | --- |
| `type` | `EnumReleaseType` | `Album` | `Album` \| `Mixtape` \| `EP` \| `Single` |
| `pageIndex` | int | 0 | zero-based, matching every other paginated endpoint in this module |
| `pageSize` | int | 12 | frontend page size for both release sections |

Response: `PaginatedResult<AlbumDto>`.

Anonymous, `RateLimitPolicies.ContentBrowsing`, `Produces<PaginatedResult<AlbumDto>>(200)`,
`ProducesProblem(404)` (unknown slug), `ProducesProblem(429)`.

### One endpoint with a filter, not two endpoints

`/releases?type=Mixtape` rather than `/albums` and `/mixtapes`. The two sections differ by a filter
value and a heading, nothing else — same card, same ordering, same page size, same DTO
([frontend 08](../../../../frontend/docs/artists-page/08-catalog-sections.md)). Two endpoints would
be the same handler twice with one literal changed, and would need a third when `EP` is ever
surfaced.

### The slug is resolved, not trusted

The handler resolves `slug → ArtistEntity` first and throws `i18n.Artist.NotFound` if absent, then
queries by the resolved id. It does **not** accept an artist id from the client — the whole public
surface is slug-addressed and no public response carries an artist id
([frontend 14](../../../../frontend/docs/artists-page/14-data-requirements.md)).

## What the response deliberately omits

`AlbumDto` already carries `Id`, and the frontend does not use it: **release cards are not links in
v1** because there is no album detail page. A card that looks clickable and does nothing is worse
than a static tile.

The `Id` stays on the DTO rather than being stripped, because `AlbumDto` is shared with the admin
surface where the id is essential. Removing it here would fork the DTO to express "the frontend
happens not to need this yet".

No track listing is returned. The profile renders a cover, a name and a year; "more from this album"
already exists on the lyrics detail page and is not what this section is.

## Checklist

- [x] `AlbumByArtistSpecification` and `AlbumByReleaseTypeSpecification`
- [x] `IAlbumRepository.GetByArtistAsync` + implementation composing both specifications
- [x] Ordering: nulls last, `ReleaseYear` desc, `Name` asc
- [x] `PublicGetArtistReleasesQuery` + `Result`
- [x] `PublicGetArtistReleasesHandler` — resolves slug, throws `NotFound`, maps to `AlbumDto`
- [x] `PublicGetArtistReleasesMetaField`
- [x] `PublicGetArtistReleasesEndpointV1` — `GET /public/artists/{slug}/releases`, anonymous, rate-limited
- [x] Unit: handler throws `NotFound` for an unknown slug
- [x] Unit: handler passes the resolved artist id, never the slug, to the repository
- [ ] Integration: an artist's albums come back for `type=Album` and mixtapes do not
- [ ] Integration: `type=Mixtape` returns only mixtapes
- [ ] Integration: ordering is year desc, then name asc, with null years last
- [ ] Integration: paging is stable — page 1 and page 2 share no rows
- [ ] Integration: unknown slug returns 404
- [ ] Integration: an artist with no releases returns an empty page with `count: 0`, not a 404
- [ ] `dotnet build` and both test suites clean
