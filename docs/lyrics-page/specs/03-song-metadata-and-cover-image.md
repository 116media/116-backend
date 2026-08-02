# Spec 03 — Song Metadata & Cover Image

`LyricsEntity` today only identifies the performer (`ArtistName`). This spec adds a cover image and
the requested song-credit fields: album, release year, label, songwriter, producer.

## Naming — `Songwriter`, not `Author`

`LyricsDto.AuthorId`/`Author` is CMS attribution (the admin who entered the record) — see
[../00-overview.md](../00-overview.md) and `LyricsMapper.ToLyricsDtoAsync`. The credit fields
requested here must **not** be named `Author` or they'd collide with that existing, unrelated
field. These follow the actual music-industry credit terms — **not** the classical/PRO-style
"lyricist"/"composer" split this doc set used in an earlier draft, which doesn't match how credits
are actually labeled in this genre (hip-hop/Afrobeat): `Songwriter` (who wrote the words —
sometimes the performer, sometimes not) and `Producer` (who produced the instrumental/recording —
the dominant credited role in this genre, e.g. "Prod. by X"), both distinct from `ArtistName` (the
performer) when known.

## `LyricsEntity` additions

```csharp
/// <summary>
/// ID of the uploaded cover/album art file tracked in the Core module. Null until an
/// admin uploads one. The cover image URL is resolved from the associated FileEntity.
/// </summary>
public Guid? CoverImageFileId { get; private set; }

/// <summary>
/// The album this song appears on, if known.
/// </summary>
[MaxLength(length: ContentConstants.MaxAlbumNameLength)]
public string? Album { get; private set; }

/// <summary>
/// The year the song was released, if known.
/// </summary>
public short? ReleaseYear { get; private set; }

/// <summary>
/// The record label that released the song, if known.
/// </summary>
[MaxLength(length: ContentConstants.MaxLabelNameLength)]
public string? Label { get; private set; }

/// <summary>
/// The credited songwriter, if distinct from and known separately to the performer.
/// Distinct from <see cref="AuthorId" />, which is CMS attribution, not a song credit.
/// </summary>
[MaxLength(length: ContentConstants.MaxCreditNameLength)]
public string? Songwriter { get; private set; }

/// <summary>
/// The credited producer, if distinct from and known separately to the performer.
/// </summary>
[MaxLength(length: ContentConstants.MaxCreditNameLength)]
public string? Producer { get; private set; }
```

New method, mirroring `ArticleEntity.UpdateSeo`/`VideoEntity.SetThumbnailFileId`'s independence
from the main `Update` — metadata is edited on its own admin action, not folded into the
content-editing `Update` call:

```csharp
/// <summary>
/// Updates the song-credit fields. Each parameter is independently optional — passing
/// null for one field clears only that field, leaving the others untouched.
/// </summary>
/// <param name="album">The album name, or null to clear.</param>
/// <param name="releaseYear">The release year, or null to clear.</param>
/// <param name="label">The record label, or null to clear.</param>
/// <param name="songwriter">The credited songwriter, or null to clear.</param>
/// <param name="producer">The credited producer, or null to clear.</param>
public void UpdateMetadata(string? album, short? releaseYear, string? label, string? songwriter, string? producer)
{
    Album = album;
    ReleaseYear = releaseYear;
    Label = label;
    Songwriter = songwriter;
    Producer = producer;
}

/// <summary>
/// Sets or clears the cover/album art file reference.
/// </summary>
/// <param name="coverImageFileId">The FileEntity ID, or null to clear it.</param>
public void SetCoverImageFileId(Guid? coverImageFileId)
{
    CoverImageFileId = coverImageFileId;
}
```

## `ContentConstants` additions

```csharp
public const int MaxAlbumNameLength = 200;
public const int MaxLabelNameLength = 100;
public const int MaxCreditNameLength = 100;
```

`ReleaseYear` bounds are validated at the application layer (validator, not the entity, matching
how `ContentConstants` values are consumed elsewhere for length — not range — checks):

```csharp
public static IRuleBuilderOptions<T, short?> ValidReleaseYear<T>(
    this IRuleBuilder<T, short?> ruleBuilder
)
{
    return ruleBuilder
        .InclusiveBetween((short)1900, (short)(DateTimeOffset.UtcNow.Year + 1))
        .When(x => ValidationUtils.GetPropertyValue(instance: x, "ReleaseYear") is not null);
}
```

## `LyricsConfiguration` additions

```csharp
builder.Property(x => x.Album).HasMaxLength(ContentConstants.MaxAlbumNameLength).IsRequired(false);
builder.Property(x => x.Label).HasMaxLength(ContentConstants.MaxLabelNameLength).IsRequired(false);
builder.Property(x => x.Songwriter).HasMaxLength(ContentConstants.MaxCreditNameLength).IsRequired(false);
builder.Property(x => x.Producer).HasMaxLength(ContentConstants.MaxCreditNameLength).IsRequired(false);
builder.Property(x => x.ReleaseYear).IsRequired(false);
```

`CoverImageFileId` needs no explicit configuration beyond the default nullable-Guid mapping EF Core
already applies to every other `*FileId` property in this module.

## `LyricsDto` additions

```csharp
public record LyricsDto(
    Guid Id, string SongTitle, string ArtistName, string Slug, string LyricsText, string Language,
    Guid? VideoId, string? MetaTitle, string? MetaDescription, string AuthorId,
    string? CoverImageUrl, string? Album, short? ReleaseYear, string? Label,
    string? Songwriter, string? Producer,
    AuthorDto? Author = null
) : AuditableDto;
```

## Mapper — resolving the cover URL

`LyricsMapper` gains a cover-resolving async path, mirroring `ArticleMapper.ResolveCoverImageUrlAsync`
exactly:

```csharp
/// <summary>
/// Resolves the cover image URL from the associated FileEntity. Returns null when no
/// cover has been uploaded.
/// </summary>
private static async Task<string?> ResolveCoverImageUrlAsync(
    LyricsEntity entity, IFileRepository fileRepository, CancellationToken ct)
{
    if (!entity.CoverImageFileId.HasValue)
    {
        return null;
    }

    FileEntity? coverFile = await fileRepository.GetByIdAsync(entity.CoverImageFileId.Value, ct);
    return coverFile?.StorageUrl;
}

/// <summary>
/// Maps a <see cref="LyricsEntity" /> to a <see cref="LyricsDto" />, resolving the cover
/// image URL from the associated FileEntity. Does not resolve <c>Author</c> — callers
/// needing CMS attribution use <see cref="ToLyricsDtoAsync" /> instead.
/// </summary>
public static async Task<LyricsDto> ToLyricsDtoWithCoverAsync(
    this LyricsEntity entity, IMapper mapper, IFileRepository fileRepository, CancellationToken ct = default)
{
    string? coverImageUrl = await ResolveCoverImageUrlAsync(entity, fileRepository, ct);
    var dto = mapper.Map<LyricsDto>(entity);
    return dto with { CoverImageUrl = coverImageUrl };
}
```

Both existing public lookups (`PublicGetLyricsBySlugHandler`, `PublicGetLyricsByVideoIdHandler`)
and the new public list handler (spec 01) switch from `ToLyricsDto`/`ToLyricsDtos` to
`ToLyricsDtoWithCoverAsync`/a batched equivalent — cover resolution is needed everywhere the
existing sync mapper was used, unlike author resolution, which stays admin-only.

## Admin endpoint

New command, kept separate from `AdminUpdateLyricsCommand` (content fields) — mirrors
`AdminUpdateLyricsSeoCommand`'s existing separate-concern pattern exactly:

```csharp
/// <summary>
/// Command for updating the song-credit metadata of an existing lyrics page.
/// </summary>
/// <param name="Id">The identifier of the lyrics page to update.</param>
/// <param name="Album">The album name, or null to clear.</param>
/// <param name="ReleaseYear">The release year, or null to clear.</param>
/// <param name="Label">The record label, or null to clear.</param>
/// <param name="Songwriter">The credited songwriter, or null to clear.</param>
/// <param name="Producer">The credited producer, or null to clear.</param>
public record AdminUpdateLyricsMetadataCommand(
    Guid Id, string? Album, short? ReleaseYear, string? Label, string? Songwriter, string? Producer
) : ICommand<AdminUpdateLyricsMetadataResult>;
```

`PUT /api/v1/admin/lyrics/{id}/metadata`, mirroring `AdminUpdateLyricsSeoEndpointV1`'s route shape.
Cover image upload is a standalone-file-upload endpoint (`POST /api/v1/admin/lyrics/{id}/cover`),
per this repo's established `standalone-file-upload-pattern.md` convention (JSON metadata endpoints
and file uploads are always separate — never a single multipart-with-fields endpoint) — mirrors
`POST /api/v1/admin/videos/{id}/thumbnail` exactly.

## Task checklist

- [x] `LyricsEntity`: `CoverImageFileId`, `Album`, `ReleaseYear`, `Label`, `Songwriter`, `Producer` +
  `UpdateMetadata`/`SetCoverImageFileId`
- [x] `ContentConstants.MaxAlbumNameLength`/`MaxLabelNameLength`/`MaxCreditNameLength`
- [x] `ValidReleaseYear` validator extension
- [x] `LyricsConfiguration` property mappings
- [x] `CoverImageUrl`/`Album`/`ReleaseYear`/`Label`/`Songwriter`/`Producer` added to
  `LyricsDetailDto` (plus `CoverImageUrl` alone on `LyricsSummaryDto`, for card rendering) — the
  flat `LyricsDto` this doc originally described no longer exists; Phase 1 replaced it with the
  `LyricsSummaryDto`/`LyricsDetailDto` split, mirroring `ArticleSummaryDto`/`ArticleDetailDto`
- [x] `LyricsMapper.ResolveCoverImageUrlAsync` + async `ToLyricsSummaryDtoAsync`/
  `ToLyricsSummaryDtosAsync` and extended `ToLyricsDetailDtoAsync`/`ToLyricsDetailDtosAsync`
  (resolving cover URL, metadata fields, and `Tags`) — renamed from the doc's originally-planned
  `ToLyricsDtoWithCoverAsync` to match the Summary/Detail DTO split
- [x] Both existing public lookups + the list handler (spec 01) resolve cover URLs via the async
  summary/detail mappers
- [x] `AdminUpdateLyricsMetadataCommand`/`Handler`/`Validator`/`EndpointV1`
  (`PUT /api/v1/admin/lyrics/{id}/metadata`)
- [x] `POST /api/v1/admin/lyrics/{id}/cover` standalone file-upload endpoint, mirroring
  `videos/{id}/thumbnail`
- [x] Migration `AddSongMetadataAndCoverToLyrics` (nullable columns only, no backfill needed)
- [x] Integration tests: each metadata field is independently nullable/clearable, cover URL
  resolves correctly and is `null` before upload, `ReleaseYear` bounds rejected outside
  1900–current+1

**Bug caught and fixed during verification**: `EditorialValidation.ValidReleaseYear()`'s internal
`.When()` guard originally routed through `ValidationUtils.GetPropertyValue<T>`, which casts to
`string` and therefore always returned `null` for the `short? ReleaseYear` property — the guard was
always false, so the `InclusiveBetween` bounds check silently never ran in production (any release
year, however invalid, was accepted). Fixed with a typed `short?` reflection accessor local to
`EditorialValidation.cs` instead of the string-only shared helper. Caught by tests written to
assert the correct behavior, which initially failed and were correctly left in place (not weakened)
until the real fix landed.

**Verification, 2026-07-30**: `dotnet build` clean; Lyrics-scoped: 264/264 unit, 112/112
integration (Testcontainers Postgres), zero skips; full unit suite 6355/6358 passed (3 pre-existing
skips, unrelated). Both migrations (`AddSongMetadataAndCoverToLyrics`, `AddLyricsTags`) generated
but **not applied** to any database.
