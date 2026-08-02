# Spec 08 — Artist & Album Entities

`ArtistName` on `LyricsEntity` (and on `VideoEntity`) is a plain string today — there is no
`ArtistEntity` anywhere in the backend. That's fine for display, but it means there is no way to:
answer "show me every song/video by this artist," show artist bio/avatar info, or navigate from a
lyrics page to a real artist page. This spec adds a real, addressable `ArtistEntity` and links
`LyricsEntity` to it — directly answering that gap.

## Why a nullable FK alongside the existing string, not a replacement

`ArtistName` cannot simply become a required FK, for one concrete reason: **every existing lyrics
and video row already has a free-text `ArtistName`/no artist profile at all**, and new records will
keep being created by admins typing a name before any matching `ArtistEntity` necessarily exists.
The design keeps both:

- `ArtistName` (existing, unchanged) — always present, always the display fallback.
- `ArtistId` (new, nullable FK → `Artists`) — set once that artist has a claimed/curated profile.

This is the same relationship spec 03 already uses for `Album` (free-text) vs. spec 09's
`AlbumId` (FK) — a fallback string plus an optional link to a real row, not a breaking migration.

## `ArtistEntity`

New aggregate, `Domain/Entities/ArtistEntity.cs`, following the exact same shape as
`TagEntity`/`CategoryEntity` (simple lookup aggregate, no editorial workflow — an artist profile
isn't "published," it's either claimed or not):

```csharp
/// <summary>
/// Represents a real, addressable artist profile — distinct from the plain-text
/// <c>ArtistName</c> field on <see cref="LyricsEntity" /> and <see cref="VideoEntity" />.
/// A profile can exist unclaimed (staff-curated, no linked account) or claimed by a verified
/// artist account via <see cref="UserId" />.
/// </summary>
public class ArtistEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the artist (e.g., "Fally Ipupa").
    /// </summary>
    [MaxLength(length: ContentConstants.MaxArtistNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// URL-safe slug for the artist's public page (e.g., "fally-ipupa"). Unique across all artists.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxSlugLength)]
    public string Slug { get; private set; } = null!;

    /// <summary>
    /// Free-text biography shown on the artist's public page. Null until curated.
    /// </summary>
    public string? Bio { get; private set; }

    /// <summary>
    /// ID of the uploaded avatar file tracked in the Core module. Null until uploaded.
    /// </summary>
    public Guid? AvatarFileId { get; private set; }

    /// <summary>
    /// The identity user UUID of the verified artist account that owns this profile, or
    /// null for a staff-curated, unclaimed profile — the common case at launch, since most
    /// profiles are created by an admin just to group an artist's catalog, with no
    /// associated login. Once set, this is the identity gate spec 11's "verified-artist
    /// fast path" checks — a submission from this exact user id is treated as coming
    /// authoritatively from this artist, never by comparing the submitted artist name as
    /// text (names change, get misspelled, and can collide between unrelated people). No
    /// FK to the identity schema by design, matching every other cross-schema reference in
    /// this module.
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// When ownership verification completed. Null until <see cref="ClaimOwnership" /> is called.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; private set; }

    private ArtistEntity() { }

    /// <summary>
    /// Creates a new, unclaimed artist profile — typically staff-curated from an existing
    /// lyrics or video record's <c>ArtistName</c>.
    /// </summary>
    public static ArtistEntity Create(Guid id, string name, string slug, string? bio, ArtistErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        if (string.IsNullOrWhiteSpace(value: slug))
        {
            throw errors.SlugRequired();
        }

        return new ArtistEntity { Id = id, Name = name, Slug = slug, Bio = bio };
    }

    /// <summary>
    /// Updates the artist's editable profile fields. Slug is immutable after creation to
    /// preserve public URLs, matching <see cref="ShortVideoEntity" />'s own slug-immutability rule.
    /// </summary>
    public void Update(string name, string? bio, ArtistErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        Name = name;
        Bio = bio;
    }

    /// <summary>
    /// Sets or clears the avatar file reference.
    /// </summary>
    public void SetAvatarFileId(Guid? avatarFileId) => AvatarFileId = avatarFileId;

    /// <summary>
    /// Links this profile to a verified artist account. One profile can be claimed by
    /// exactly one account — enforced here and by a database unique constraint on
    /// <c>UserId</c>.
    /// </summary>
    /// <exception cref="ConflictException">Thrown if the profile is already claimed.</exception>
    public void ClaimOwnership(Guid userId, ArtistErrors errors)
    {
        if (UserId.HasValue)
        {
            throw errors.AlreadyClaimed();
        }

        UserId = userId;
        VerifiedAt = DateTimeOffset.UtcNow;
    }
}
```

### What `UserId` is actually for

`UserId` is the field that answers one question: **has this artist profile been claimed by a real,
verified user account, or is it still just a staff-curated shell nobody's logged into?** It
defaults to `null` — the common case — because most profiles start life as an admin grouping an
artist's existing catalog (spec 08's own backfill note above), with no account behind them at all.

It becomes load-bearing downstream in **spec 11's verified-artist fast path**: when a signed-in
user submits a new song, `IArtistRepository.GetByUserIdAsync(submittingUserId)` looks up whether
*that exact user* owns a claimed profile. If it finds one, the submission is attributed to that
profile's own `Name`/`Id` directly — **never** by comparing the submitted artist-name text against
anything. That sidesteps name-change/typo/collision problems entirely: the identity check is "is
this the same user id," not "does this string look like that string."

(An earlier draft of this doc also tied `UserId` to a per-artist revenue-share ledger — that's been
dropped from spec 12's scope: creator payouts assume payment/payout infrastructure this platform's
actual markets don't reliably have yet. `UserId`'s only current downstream use is the fast-path
gate above.)

Verification itself (confirming the claiming user really is that artist — email domain match,
label-provided roster, manual staff check) is a business/trust process, not a technical one, and is
deliberately left out of scope here; `ClaimOwnership` is the single point where a verified claim is
recorded once that out-of-band process completes.

## `LyricsEntity` / `VideoEntity` additions

```csharp
/// <summary>
/// Optional link to a claimed <see cref="ArtistEntity" /> profile. Null for the common case
/// of an unclaimed artist — <see cref="ArtistName" /> remains the display fallback either way.
/// </summary>
public Guid? ArtistId { get; private set; }

/// <summary>
/// Links this record to a claimed artist profile.
/// </summary>
public void LinkArtist(Guid artistId) => ArtistId = artistId;

/// <summary>
/// Clears the artist profile link, reverting display to the plain-text <see cref="ArtistName" />.
/// </summary>
public void UnlinkArtist() => ArtistId = null;
```

`VideoEntity` gains the identical pair, for the same reason — an artist page needs to show *both*
their songs and their videos.

## Configuration

```csharp
public class ArtistConfiguration : IEntityTypeConfiguration<ArtistEntity>
{
    public void Configure(EntityTypeBuilder<ArtistEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(ContentConstants.MaxArtistNameLength).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(ContentConstants.MaxSlugLength).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.UserId).IsUnique().HasFilter("user_id IS NOT NULL");
    }
}
```

`LyricsConfiguration`/`VideoConfiguration` each gain:

```csharp
builder
    .HasOne<ArtistEntity>()
    .WithMany()
    .HasForeignKey(x => x.ArtistId)
    .IsRequired(false)
    .OnDelete(DeleteBehavior.SetNull);
```

`OnDelete(DeleteBehavior.SetNull)` — deleting an artist profile never cascades into deleting the
songs/videos that reference it; they simply fall back to their plain-text `ArtistName` again.

## How existing rows get linked — backfill, not a breaking change

Existing `LyricsEntity`/`VideoEntity` rows keep working unchanged (`ArtistId` starts `null`
everywhere). Linking happens two ways, neither of which blocks this migration from shipping:

1. **Admin-curated backfill**: a new dashboard action, "Create artist from this record" (or "Link
   existing artist"), on the lyrics/video edit form — an admin picks or creates the matching
   `ArtistEntity` for a record's `ArtistName` by hand. This is the primary path at launch: no
   automated matching is trustworthy enough to run unsupervised (same-named artists, spelling
   variants, featured-artist strings like "Fally Ipupa ft. Innoss'B" are all real failure modes).
2. **Verified-artist self-claim** (spec 11 §"verified-artist fast path"): once an artist verifies
   their own account, claiming their profile is a one-time action on their side, not a bulk
   backend job.

No automatic fuzzy-matching backfill job is specced here — the risk of silently mislinking one
artist's catalog to a same-named but different person is worse than leaving `ArtistId` null a
while longer. If a bulk-assist tool is wanted later, it should propose matches for admin
confirmation, never auto-apply them.

## The artist page endpoint — the actual answer to "navigate from lyrics to artist info"

```csharp
/// <summary>
/// Query for retrieving a claimed artist's public profile along with their published
/// lyrics and videos.
/// </summary>
/// <param name="Slug">The artist's URL-safe slug.</param>
/// <param name="LyricsPage">Pagination parameters for the artist's lyrics catalog.</param>
/// <param name="VideosPage">Pagination parameters for the artist's video catalog.</param>
public record PublicGetArtistBySlugQuery(
    string Slug, PaginatedRequest LyricsPage, PaginatedRequest VideosPage
) : IQuery<PublicGetArtistBySlugResult>;

public record PublicGetArtistBySlugResult(
    ArtistDto Artist, PaginatedResult<LyricsDto> Lyrics, PaginatedResult<VideoDto> Videos
);
```

```csharp
public class PublicGetArtistBySlugHandler(
    IArtistRepository artistRepository, ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository, IFileRepository fileRepository, IMapper mapper, ContentI18n i18n
) : IQueryHandler<PublicGetArtistBySlugQuery, PublicGetArtistBySlugResult>
{
    public async Task<PublicGetArtistBySlugResult> Handle(
        PublicGetArtistBySlugQuery query, CancellationToken cancellationToken)
    {
        ArtistEntity? artist = await artistRepository.GetBySlugAsync(query.Slug, cancellationToken);
        if (artist is null)
        {
            throw i18n.Artist.NotFound(id: Guid.Empty);
        }

        (List<LyricsEntity> lyrics, int lyricsCount) = await lyricsRepository.GetPublishedByArtistAsync(
            artistId: artist.Id, page: query.LyricsPage.PageIndex + 1,
            pageSize: query.LyricsPage.PageSize, cancellationToken: cancellationToken);

        (List<VideoEntity> videos, int videosCount) = await videoRepository.GetPublishedByArtistAsync(
            artistId: artist.Id, page: query.VideosPage.PageIndex + 1,
            pageSize: query.VideosPage.PageSize, cancellationToken: cancellationToken);

        var artistDto = await artist.ToArtistDtoAsync(mapper, fileRepository, cancellationToken);

        return new PublicGetArtistBySlugResult(
            Artist: artistDto,
            Lyrics: new PaginatedResult<LyricsDto>(
                query.LyricsPage.PageIndex, query.LyricsPage.PageSize, lyricsCount,
                lyrics.AsReadOnly().ToLyricsDtos(mapper).ToList()),
            Videos: new PaginatedResult<VideoDto>(
                query.VideosPage.PageIndex, query.VideosPage.PageSize, videosCount,
                mapper.Map<List<VideoDto>>(videos))
        );
    }
}
```

`ILyricsRepository.GetPublishedByArtistAsync`/`IVideoRepository.GetPublishedByArtistAsync` are new
methods combining `LyricsPublishedSpecification`/the video equivalent (spec 01) with a plain
`ArtistId == artistId` filter — same shape as every other paginated-by-filter method in this
module.

Route: `GET /api/v1/public/artists/{slug}`. This is what the frontend's artist-name link on a
lyrics page (once `ArtistId` is set) actually navigates to, and what powers "their songs, their
videos, their bio" on that page.

## Claim endpoints

The claim itself is two steps, matching the "verification is a business process, not a technical
one" note above — a user-initiated request, then a separate admin action that actually calls
`ClaimOwnership` once verification completes off-platform:

| Method | Route | Auth |
| --- | --- | --- |
| POST | `/api/v1/artists/{id}/claim` | Authenticated — records a pending claim request (which artist, which user, any supporting info); does **not** itself set `UserId` |
| POST | `/api/v1/admin/artists/{id}/verify-owner` | Admin — calls `ArtistEntity.ClaimOwnership(userId)` once the admin has confirmed the request out-of-band |

`POST /api/v1/artists/{id}/claim` does not need its own entity for the MVP of this spec — a request
can be logged as a simple audit row or routed to an existing support/ticket flow if one exists in
this codebase; the technical contract this feature actually depends on is only the second endpoint
(`verify-owner`, which mutates `ArtistEntity`). A dedicated `ArtistClaimRequestEntity` (status,
requester, evidence) is a reasonable follow-up if claim volume justifies a real queue, but isn't
required to ship `UserId`/`ClaimOwnership` itself.

## Admin endpoints

| Method | Route | Auth |
| --- | --- | --- |
| GET | `/api/v1/admin/artists` | Admin — paginated list, `search` param |
| POST | `/api/v1/admin/artists` | Admin — create an unclaimed profile |
| PUT | `/api/v1/admin/artists/{id}` | Admin — update name/bio |
| POST | `/api/v1/admin/artists/{id}/avatar` | Admin — standalone file upload |
| PUT | `/api/v1/admin/lyrics/{id}/artist` | Admin — `LinkArtist`/`UnlinkArtist` on a lyrics record |
| PUT | `/api/v1/admin/videos/{id}/artist` | Admin — same, for videos |

## `AlbumEntity` — the same pattern, one level down

Needed for spec 09's "more from this album" card and per-album streaming links. Same shape as
`ArtistEntity`, minus the ownership/claiming concept (albums aren't claimed, artists are):

```csharp
/// <summary>
/// Represents a real, addressable album — distinct from the plain-text <c>Album</c> field
/// on <see cref="LyricsEntity" /> (spec 03). Groups songs for "more from this album" and
/// carries per-platform streaming links (spec 09).
/// </summary>
public class AlbumEntity : Aggregate<Guid>
{
    /// <summary>
    /// Display name of the album.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxAlbumNameLength)]
    public string Name { get; private set; } = null!;

    /// <summary>
    /// Optional link to the claimed artist profile this album belongs to.
    /// </summary>
    public Guid? ArtistId { get; private set; }

    /// <summary>
    /// ID of the uploaded cover art file tracked in the Core module.
    /// </summary>
    public Guid? CoverImageFileId { get; private set; }

    /// <summary>
    /// The year the album was released, if known.
    /// </summary>
    public short? ReleaseYear { get; private set; }

    /// <summary>
    /// The record label that released the album, if known.
    /// </summary>
    [MaxLength(length: ContentConstants.MaxLabelNameLength)]
    public string? Label { get; private set; }

    private AlbumEntity() { }

    public static AlbumEntity Create(
        Guid id, string name, Guid? artistId, Guid? coverImageFileId, short? releaseYear, string? label,
        AlbumErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        return new AlbumEntity
        {
            Id = id, Name = name, ArtistId = artistId, CoverImageFileId = coverImageFileId,
            ReleaseYear = releaseYear, Label = label,
        };
    }

    public void Update(string name, Guid? coverImageFileId, short? releaseYear, string? label, AlbumErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value: name))
        {
            throw errors.NameRequired();
        }

        Name = name;
        CoverImageFileId = coverImageFileId;
        ReleaseYear = releaseYear;
        Label = label;
    }
}
```

`LyricsEntity` gains the identical `AlbumId`/`LinkAlbum`/`UnlinkAlbum` triple as `ArtistId` above,
coexisting with the free-text `Album` field from spec 03 the same way `ArtistId` coexists with
`ArtistName`. `AlbumConfiguration` mirrors `ArtistConfiguration` (no ownership/unique-owner index,
otherwise identical), with `HasOne<ArtistEntity>().WithMany().HasForeignKey(x => x.ArtistId)
.OnDelete(DeleteBehavior.SetNull)`.

Admin endpoints: `GET`/`POST /api/v1/admin/albums`, `PUT /api/v1/admin/albums/{id}`,
`POST /api/v1/admin/albums/{id}/cover`, `PUT /api/v1/admin/lyrics/{id}/album` — same shapes as the
artist endpoints above.

## Migration

```bash
dotnet ef migrations add AddArtistEntityAndLinks \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `ArtistEntity` + `ArtistErrors` (`NameRequired`, `SlugRequired`, `AlreadyClaimed`, `NotFound`)
  and `ArtistConfiguration`
- [x] `ContentI18n.Artist` property (`.Album` added alongside it for `AlbumEntity`)
- [x] `LyricsEntity.ArtistId`/`AlbumId`/`LinkArtist`/`UnlinkArtist`/`LinkAlbum`/`UnlinkAlbum`;
  `VideoEntity.ArtistId`/`LinkArtist`/`UnlinkArtist` (no album concept on video, per the plan)
- [x] `IArtistRepository`/`ArtistRepository`: `GetBySlugAsync`, `GetByIdAsync`,
  `GetByIdOrThrowAsync`, `GetByUserIdAsync` (the identity-gate lookup spec 11's fast path will use
  later), `GetAllAsync`, `AddAsync`, `Update`. `IAlbumRepository`/`AlbumRepository` shipped
  alongside it, same shape minus `GetByUserIdAsync`.
- [x] `ILyricsRepository.GetPublishedByArtistAsync`; `IVideoRepository.GetPublishedByArtistAsync`
- [x] `PublicGetArtistBySlugQuery`/`Handler`/`EndpointV1` — response uses
  `PaginatedResult<LyricsSummaryDto>`/`PaginatedResult<VideoSummaryDto>`, not the flat
  `LyricsDto`/`VideoDto` this doc originally sketched (both were already split into
  Summary/Detail DTOs before this phase)
- [x] Admin CRUD endpoints for `ArtistEntity` + avatar upload + the two link/unlink endpoints on
  lyrics (`artist`, `album`) + the link/unlink endpoint on video (`artist`). `AlbumEntity` shipped
  its own admin CRUD + cover upload alongside it.
- [x] `ArtistDto` (`Id`, `Name`, `Slug`, `Bio`, `AvatarUrl`) + `ArtistMapper.ToArtistDtoAsync`
  (resolves avatar URL, mirroring `LyricsMapper.ResolveCoverImageUrlAsync`). `AlbumDto`/
  `AlbumMapper` shipped the same way.
- [x] Claim endpoints: `POST /api/v1/artists/{id}/claim` (authenticated, logs the request via
  `ILogger`, does **not** call `ClaimOwnership` — no dedicated claim-request entity exists in this
  phase, per the spec's own scoping) and `POST /api/v1/admin/artists/{id}/verify-owner`
  (admin-only, calls `ClaimOwnership`)
- [x] Migration `AddArtistEntityAndLinks`
- [x] Integration tests: claiming an already-claimed profile conflicts, an artist page shows only
  `Published` lyrics/videos (a Draft one linked to the same artist is confirmed absent), unlinking
  an artist reverts a record to its plain-text `ArtistName` with no data loss, deleting a claimed
  artist sets `ArtistId` to null on all linked records rather than cascading (verified against real
  Postgres via the `SetNull` FK behavior)

**Spec 02 follow-through**: `PublicGetLyricsBySlugHandler`'s `ArtistSlug` field — stubbed `null`
in Phase 2 pending this spec — now resolves for real via `IArtistRepository.GetByIdAsync` (never
`GetByIdOrThrowAsync`, so a stale/deleted `ArtistId` degrades to `null` rather than 404ing the whole
lyrics page, same rule as the existing `VideoSlug` resolution next to it).

**Verification, 2026-07-30**: `dotnet build` clean; Artist/Album/Lyrics/Video-scoped: 1045/1045
unit, 490/490 integration, zero skips; full suite 6471/6471 unit (3 pre-existing unrelated skips),
1562/1562 integration. Migration `AddArtistEntityAndLinks` generated but **not applied** to any
database.
