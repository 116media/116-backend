# Spec 02 — Artist Social Links

**Frontend gap 8.** Blocks the social row in the profile hero
([frontend 06](../../../../frontend/docs/artists-page/06-artist-detail-hero.md)).

A row of platform icons under the identity block, rendered only when at least one link exists. Each
icon is an outbound link to the artist's profile on that platform.

## Not columns — a child table

The tempting shape is `InstagramUrl`, `XUrl`, `FacebookUrl`, `YoutubeUrl`, `TiktokUrl`,
`WebsiteUrl` on `ArtistEntity`. Rejected, for reasons this module has already worked through once:

- **Adding a platform becomes a migration.** With a child table it is one enum member.
- **`(ArtistId, Platform)` uniqueness is not expressible** across six columns; with a child table it
  is one unique index.
- **Ordering is not controllable** — column order is not display order, so the frontend would
  hard-code the sequence.
- **Six mostly-null columns on the hottest artist row** is dead width on every artist query.

`StreamingLinkEntity` already solved exactly this problem in this module
(`AlbumId`/`LyricsId` + `Platform` enum + `Url`, unique per parent+platform, cascade delete). This
spec mirrors it. Following a tested shape in the same module beats inventing a second one.

## `EnumSocialPlatform`

`Domain/Enums/EnumSocialPlatform.cs`:

```csharp
public enum EnumSocialPlatform
{
    Instagram,
    X,
    Facebook,
    YouTube,
    TikTok,
    Website,
}
```

`Website` is in the same enum rather than a separate `WebsiteUrl` column: from the row's point of
view an official site is one more outbound destination with an icon and a label. Splitting it would
mean two shapes for one behaviour.

**Members are only ever appended.** The stored value is the integer; reordering renumbers live rows.

The frontend skips values it does not recognise rather than rendering a generic link
([frontend 18](../../../../frontend/docs/artists-page/18-domain-entities-and-mappers.md)), which is
what makes adding a platform **backend-first** safe. That contract cuts both ways: the backend may
add a member before the frontend ships an icon, and must not assume every stored platform is
rendered.

## `ArtistSocialLinkEntity`

`Domain/Entities/ArtistSocialLinkEntity.cs`, an `Aggregate<Guid>` mirroring `StreamingLinkEntity`:

| Property | Type | Notes |
| --- | --- | --- |
| `ArtistId` | `Guid` | Required. Cascade-deleted with the artist. |
| `Platform` | `EnumSocialPlatform` | Unique per artist. |
| `Url` | `string` | `MaxStreamingLinkUrlLength` (500) — reused, same shape of value. |
| `Artist` | `ArtistEntity` | Navigation property. |

Two members:

```csharp
public static ArtistSocialLinkEntity Create(Guid id, Guid artistId, EnumSocialPlatform platform, string url)
public void UpdateUrl(string url)
```

No `Ordering` column. Display order is a **frontend** decision driven by the enum, not data — a
sort column would let two artists' social rows disagree about where Instagram goes, for no benefit.

### Configuration

```csharp
builder.HasKey(x => x.Id);
builder.Property(x => x.Url).HasMaxLength(ContentConstants.MaxStreamingLinkUrlLength).IsRequired();
builder.HasIndex(x => new { x.ArtistId, x.Platform }).IsUnique();
builder.HasOne(x => x.Artist).WithMany().HasForeignKey(x => x.ArtistId).OnDelete(DeleteBehavior.Cascade);
```

Cascade, not `SetNull`: a social link has no meaning without its artist. Same call
`StreamingLinkConfiguration` makes for the same reason.

## URL validation — `https` only, enforced twice

The URL is staff-entered free text that the public page turns into an `<a href>`. A
`javascript:` value there is a stored XSS vector, and a plain `http://` link on an https page is a
mixed-content warning on someone's profile.

**Validated on write** (`AdminUpsertArtistSocialLinkValidator`):

- not empty,
- within `MaxStreamingLinkUrlLength`,
- parses as an absolute `Uri`,
- scheme is exactly `https`.

Rejecting rather than coercing is deliberate. Silently rewriting `http` to `https` produces a link
that 404s if the destination has no TLS, and the admin never learns why.

The frontend re-checks the scheme at render
([frontend 13](../../../../frontend/docs/artists-page/13-seo-and-metadata.md)). Two checks for one
rule is correct here: the write check gives the admin a real error message, the render check means a
row written before this validation existed still cannot inject.

**The URL is never parsed for a handle.** No `@username` extraction, no display-domain field. The
frontend renders an icon with an accessible label built from the platform name and the artist name;
the URL is navigate-only.

## Repository

`IArtistRepository` gains:

```csharp
Task<IReadOnlyList<ArtistSocialLinkEntity>> GetSocialLinksAsync(Guid artistId, CancellationToken ct = default);
Task<ArtistSocialLinkEntity?> GetSocialLinkAsync(Guid artistId, EnumSocialPlatform platform, CancellationToken ct = default);
Task AddSocialLinkAsync(ArtistSocialLinkEntity link, CancellationToken ct = default);
void UpdateSocialLink(ArtistSocialLinkEntity link);
void RemoveSocialLink(ArtistSocialLinkEntity link);
```

They live on `IArtistRepository` rather than a new `IArtistSocialLinkRepository` because a social
link is not an aggregate anyone addresses independently — it is always reached through its artist.
`StreamingLinkEntity` follows the same rule via `IAlbumRepository`.

Ordering is `Platform` ascending, so the row is stable across requests.

## DTO and mapper

```csharp
public record ArtistSocialLinkDto(EnumSocialPlatform Platform, string Url);
```

No `Id`, no `ArtistId`. The client never addresses a link individually — it renders the row and
follows the URLs. Exposing ids invites them into URLs.

`ArtistSocialLinkMapper` with `ToArtistSocialLinkDto` and `ToArtistSocialLinkDtoList`. The list
helper lives on the mapper; call sites never `.Select(Mapper.X)` themselves.

`ArtistDto` gains `IReadOnlyList<ArtistSocialLinkDto> SocialLinks` — empty list when there are none,
never null. An empty array and a null both mean "render nothing", and collapsing them at the
boundary keeps the client from having to handle two shapes for one state.

## Admin surface

Two use cases, mirroring `AdminUpsertAlbumStreamingLink` / `AdminRemoveAlbumStreamingLink`
one-for-one:

| Use case | Route | Behaviour |
| --- | --- | --- |
| `AdminUpsertArtistSocialLink` | `PUT /api/v1/admin/artists/{id}/social-links/{platform}` | Body `{ url }`. Creates the row, or replaces the URL if that platform already exists. |
| `AdminRemoveArtistSocialLink` | `DELETE /api/v1/admin/artists/{id}/social-links/{platform}` | Removes the row. 404 if absent. |

The platform sits in the **route**, not the body, matching the album streaming-link endpoints
exactly — the slot being addressed is part of the resource's identity, and the body carries only
what changes.

**Upsert, not create+update.** The unique constraint means "create" on an existing platform is
always a conflict the admin has to resolve by finding and editing the existing row instead. One
idempotent verb removes a whole class of 409s from a form that has exactly one field per platform.

Both require `RequireAuthorization()` with the admin policy and
`RateLimitPolicies.ContentBrowsing`, matching the album streaming-link endpoints.

New `ArtistErrors` member:

| Member | Exception | Message |
| --- | --- | --- |
| `SocialLinkNotFound(platform)` | `NotFoundException` | *No {0} link exists for this artist.* |

## Checklist

- [x] `EnumSocialPlatform` added with the six members in the documented order
- [x] `ArtistSocialLinkEntity` with `Create` and `UpdateUrl`
- [x] `ArtistSocialLinkConfiguration`: max length, unique `(ArtistId, Platform)`, cascade delete
- [x] `DbSet<ArtistSocialLinkEntity>` on `ContentDbContext`
- [x] Migration generated (`AddArtistPageFeature`, shared by specs 01–07), left unapplied
- [x] `IArtistRepository` + implementation: get all, get one, add, update, remove — ordered by `Platform`
- [x] `ArtistSocialLinkDto` + `ArtistSocialLinkMapper` with a list helper
- [x] `ArtistDto.SocialLinks`, empty list rather than null
- [x] `ArtistErrors.SocialLinkNotFound` + message + all three `.resx`
- [x] `AdminUpsertArtistSocialLink` — command, handler, validator, meta field, `EndpointV1`
- [x] `AdminRemoveArtistSocialLink` — command, handler, meta field, `EndpointV1`
- [x] Unit: entity `Create`/`UpdateUrl`
- [x] Unit: validator rejects empty, over-length, relative, `http`, and `javascript:` URLs; accepts `https`
- [x] Unit: upsert handler creates when absent and replaces the URL when present
- [x] Unit: remove handler throws `SocialLinkNotFound` when absent
- [ ] Integration: upsert twice on one platform yields **one** row with the second URL
- [ ] Integration: upsert on two platforms yields two rows, returned ordered by platform
- [ ] Integration: delete removes the row and a second delete 404s
- [ ] Integration: a non-`https` URL is rejected with 400 at the real endpoint
- [ ] Integration: deleting the artist cascades the links away
- [ ] Integration: the public profile response carries the social links
- [ ] `dotnet build` and both test suites clean
