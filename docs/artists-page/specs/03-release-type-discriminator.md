# Spec 03 — Release-Type Discriminator

**Frontend gap 4.** Blocks the Mixtapes section inside the `Musique` tab
([frontend 08](../../../../frontend/docs/artists-page/08-catalog-sections.md)).

`AlbumEntity` carries `Name`, `ArtistId`, `CoverImageFileId`, `ReleaseYear`, `Label` — and no type.
An album and a mixtape are therefore indistinguishable rows, so the profile cannot render them as
two sections.

## `EnumReleaseType`

`Domain/Enums/EnumReleaseType.cs`:

```csharp
public enum EnumReleaseType
{
    Album,
    Mixtape,
    EP,
    Single,
}
```

**`EP` and `Single` ship in the enum from day one even though the UI renders neither.** Adding an
enum member later is a one-line change; re-classifying live rows after editors have spent months
filing every EP as an album is manual catalog work nobody will fund. The cost of the two unused
members is zero and the cost of omitting them is paid later, by someone else.

The UI groups only `Album` and `Mixtape`. A row typed `EP` or `Single` renders in **neither**
section — deliberately, not by accident. Both sections filter on an explicit value rather than
"everything that is not a mixtape", so a new member never leaks into an existing heading. This is
asserted in [spec 10](10-verification-checklist.md).

## Column

```
release_type INTEGER NOT NULL DEFAULT 0
```

Non-nullable with a default rather than nullable. "Untyped release" is not a state the UI can
render — it would fall out of both sections and the artist would silently lose a record. A default
means every row is always in exactly one bucket.

### Backfill

Existing rows become `Album`. Two options were weighed:

| Option | Cost |
| --- | --- |
| Default everything to `Album`, editors correct from there | Some mixtapes sit under "Albums" until someone fixes them. Visible, self-correcting, ships today. |
| Audit the catalog before migrating | Blocks a one-column migration on manual work across every existing album. |

The first wins. A mis-filed mixtape is a wrong heading on one card; a blocked migration stops the
whole Mixtapes section from existing. The backfill is the column default — no data script needed.

### Index

```
CREATE INDEX ix_albums_artist_id_release_type ON content.albums (artist_id, release_type);
```

This is the exact shape [spec 04](04-artist-scoped-release-query.md) queries on, and the same index
serves the album count in [spec 06](06-surfaceable-content.md). One index, both readers.

## Domain changes

`AlbumEntity` gains `public EnumReleaseType ReleaseType { get; private set; }`.

`Create` and `Update` both take it as a **required parameter before `errors`**, for the same reason
spelled out in [spec 01](01-artist-identity-fields.md): a trailing optional silently resets the
field on every existing call site that does not pass it, and a release type that resets to `Album`
on an unrelated edit is a real data-loss bug, not a theoretical one.

```csharp
public static AlbumEntity Create(
    Guid id,
    string name,
    Guid? artistId,
    Guid? coverImageFileId,
    short? releaseYear,
    string? label,
    EnumReleaseType releaseType,
    AlbumErrors errors
)

public void Update(
    string name,
    Guid? coverImageFileId,
    short? releaseYear,
    string? label,
    EnumReleaseType releaseType,
    AlbumErrors errors
)
```

No guard is needed on the value itself: an out-of-range integer cast to the enum is caught by the
validator at the boundary, and the domain has no meaningful behaviour to protect. Adding a
defensive `Enum.IsDefined` throw here would be an unreachable guard on an internal call path.

`ArtistId` stays absent from `Update`, matching today's shape — album↔artist linkage is its own
concern and is not re-supplied on every metadata edit.

## DTO

`AlbumDto` gains `EnumReleaseType ReleaseType`. Serialised as its string name, matching how the
module already emits `EnumContentStatus` and `EnumStreamingPlatform`, so the frontend branches on
`"Album"` rather than on `0`.

## Admin surface

`AdminCreateAlbumCommand` and `AdminUpdateAlbumCommand` gain `ReleaseType`, with the field on both
V1 request records and a rule on both validators:

```csharp
RuleFor(x => x.ReleaseType).IsInEnum();
```

`IsInEnum` is the boundary check that makes the domain's lack of a guard correct — an integer
outside the enum is rejected with a 400 and a field path before it reaches the entity.

The admin create form should default the control to `Album`, since it is overwhelmingly the common
case, but that is a dashboard concern; the API requires the value explicitly.

## Checklist

- [x] `EnumReleaseType` added with `Album`, `Mixtape`, `EP`, `Single` in that order
- [x] `AlbumEntity.ReleaseType` property
- [x] `AlbumEntity.Create` and `Update` take `releaseType` as a required parameter before `errors`
- [x] `AlbumConfiguration`: `release_type` non-nullable with default `Album`
- [x] `ix_albums_artist_id_release_type` composite index
- [x] Migration generated (`AddArtistPageFeature`), existing rows defaulting to `Album`, left unapplied
- [x] `AlbumDto.ReleaseType`, serialised by name
- [x] `AlbumMapper` passes it through
- [x] `AdminCreateAlbumCommand`/`Validator`/`EndpointV1` request updated with `IsInEnum`
- [x] `AdminUpdateAlbumCommand`/`Validator`/`EndpointV1` request updated with `IsInEnum`
- [x] Every other `AlbumEntity.Create`/`Update` call site updated (seeds, fixtures, tests)
- [x] Unit: `Create` and `Update` persist the type; `Update` does not reset it
- [x] Unit: both validators reject an out-of-range enum value
- [ ] Integration: admin create with `Mixtape` round-trips and reads back as `Mixtape`
- [ ] Integration: admin update changes the type without touching the other fields
- [ ] Integration: an album row created before the migration reads back as `Album`
- [ ] `dotnet build` and both test suites clean
