# Spec 09 — Streaming Links & Album Tracks

Depends on spec 08's `AlbumEntity`. Two pieces: "more from this album" and the "go to streaming
platform" launcher.

A release is either an album or a standalone single — a song with no `AlbumId` (spec 08's
`LyricsEntity.AlbumId` is nullable). Streaming links attach to whichever one applies, exactly how
Spotify/Apple Music themselves distinguish an album release from a single release; scoping this
table to albums only would leave every single with no "go to Spotify" launcher at all, which isn't
acceptable given how many releases on this platform are singles rather than album tracks.

## `StreamingLinkEntity`

```csharp
/// <summary>
/// A curated deep link to a release on a specific streaming platform. A release is either an
/// album (<see cref="AlbumId" /> set) or a standalone single (<see cref="LyricsId" /> set) —
/// exactly one of the two is ever set. Absence of a row for a given platform is expected — the
/// public endpoint falls back to a generated search URL.
/// </summary>
public class StreamingLinkEntity : Aggregate<Guid>
{
    /// <summary>
    /// The album this link belongs to, when the release is an album. Mutually exclusive with
    /// <see cref="LyricsId" />.
    /// </summary>
    public Guid? AlbumId { get; private set; }

    /// <summary>
    /// The standalone single (lyrics with no <c>AlbumId</c>) this link belongs to. Mutually
    /// exclusive with <see cref="AlbumId" />.
    /// </summary>
    public Guid? LyricsId { get; private set; }

    /// <summary>
    /// The streaming platform this link points to.
    /// </summary>
    public EnumStreamingPlatform Platform { get; private set; }

    /// <summary>
    /// The curated deep link URL.
    /// </summary>
    [MaxLength(500)]
    public string Url { get; private set; } = null!;

    public AlbumEntity? Album { get; private set; }

    public LyricsEntity? Lyrics { get; private set; }

    private StreamingLinkEntity() { }

    public static StreamingLinkEntity ForAlbum(Guid id, Guid albumId, EnumStreamingPlatform platform, string url)
    {
        return new StreamingLinkEntity
        {
            Id = id, AlbumId = albumId, Platform = platform, Url = url, CreatedAt = DateTime.UtcNow,
        };
    }

    public static StreamingLinkEntity ForSingle(Guid id, Guid lyricsId, EnumStreamingPlatform platform, string url)
    {
        return new StreamingLinkEntity
        {
            Id = id, LyricsId = lyricsId, Platform = platform, Url = url, CreatedAt = DateTime.UtcNow,
        };
    }

    public void UpdateUrl(string url) => Url = url;
}
```

```csharp
/// <summary>
/// A music streaming platform a "go to album" link can point to.
/// </summary>
public enum EnumStreamingPlatform
{
    Spotify,
    AppleMusic,
    YoutubeMusic,
    Tidal,
}
```

## Configuration

```csharp
public class StreamingLinkConfiguration : IEntityTypeConfiguration<StreamingLinkEntity>
{
    public void Configure(EntityTypeBuilder<StreamingLinkEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.AlbumId, x.Platform }).IsUnique();
        builder.HasIndex(x => new { x.LyricsId, x.Platform }).IsUnique();
        builder.ToTable(t => t.HasCheckConstraint(
            "ck_streaming_links_exactly_one_target",
            "(album_id IS NOT NULL AND lyrics_id IS NULL) OR (album_id IS NULL AND lyrics_id IS NOT NULL)"));

        builder.HasOne(x => x.Album).WithMany().HasForeignKey(x => x.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Lyrics).WithMany().HasForeignKey(x => x.LyricsId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

## Upsert commands — single statement, no check-then-insert

Two commands, one per release kind — mirrors the `CreateFree`/`CreatePaid` pattern of keeping
the two cases as distinct, explicit call sites rather than one command with an "either/or" param
pair the caller could get wrong.

```csharp
/// <summary>
/// Command to create or replace a curated streaming link for an album's platform slot.
/// </summary>
/// <param name="AlbumId">The album to link.</param>
/// <param name="Platform">The streaming platform.</param>
/// <param name="Url">The deep link URL.</param>
public record AdminUpsertAlbumStreamingLinkCommand(Guid AlbumId, EnumStreamingPlatform Platform, string Url)
    : ICommand<AdminUpsertStreamingLinkResult>;

/// <summary>
/// Command to create or replace a curated streaming link for a standalone single's platform slot.
/// </summary>
/// <param name="LyricsId">The single (lyrics with no <c>AlbumId</c>) to link.</param>
/// <param name="Platform">The streaming platform.</param>
/// <param name="Url">The deep link URL.</param>
public record AdminUpsertSingleStreamingLinkCommand(Guid LyricsId, EnumStreamingPlatform Platform, string Url)
    : ICommand<AdminUpsertStreamingLinkResult>;
```

```csharp
public async Task<AdminUpsertStreamingLinkResult> Handle(AdminUpsertAlbumStreamingLinkCommand command, CancellationToken ct)
{
    await albumRepository.GetByIdOrThrowAsync(command.AlbumId, ct);

    StreamingLinkEntity? existing = await streamingLinkRepository.GetByAlbumAndPlatformAsync(
        command.AlbumId, command.Platform, ct);

    if (existing is not null)
    {
        existing.UpdateUrl(command.Url);
        streamingLinkRepository.Update(existing);
    }
    else
    {
        var link = StreamingLinkEntity.ForAlbum(Guid.NewGuid(), command.AlbumId, command.Platform, command.Url);
        await streamingLinkRepository.AddAsync(link, ct);
    }

    await unitOfWork.CommitAsync(ct);
    return new AdminUpsertStreamingLinkResult(IsSuccess: true);
}
```

`AdminUpsertSingleStreamingLinkCommand`'s handler is the same shape, swapping in
`lyricsRepository.GetByIdOrThrowAsync`, `GetByLyricsAndPlatformAsync`, and `StreamingLinkEntity.ForSingle`.
The handler must also reject a `LyricsId` whose `AlbumId` is not null — a track that belongs to an
album gets its streaming links through the album, not per-track, so this command only accepts
standalone singles (mirrors `ArtistEntity.ClaimOwnership`'s style of guarding an invariant in the
handler rather than the entity, since it's a cross-aggregate check).

`DELETE /api/v1/admin/albums/{id}/streaming-links/{platform}` and
`DELETE /api/v1/admin/lyrics/{id}/streaming-links/{platform}` remove the stored row, reverting that
platform to the generated fallback below.

## Resolving links — curated, or generated, always all four

Takes a plain `releaseName` (the album's `Name`, or the single's `SongTitle`) rather than an
`AlbumEntity`, so the same helper serves both release kinds without an artificial album wrapper.

```csharp
/// <summary>
/// Resolves a usable deep link for every streaming platform: the curated
/// <see cref="StreamingLinkEntity" /> if one exists, otherwise a generated search-query URL.
/// Always returns exactly one entry per platform — never a partial or missing set.
/// </summary>
public static IReadOnlyList<(EnumStreamingPlatform Platform, string Url)> ResolveStreamingLinks(
    string artistName, string releaseName, IReadOnlyDictionary<EnumStreamingPlatform, string> curated)
{
    string query = Uri.EscapeDataString($"{artistName} {releaseName}");

    return Enum.GetValues<EnumStreamingPlatform>()
        .Select(platform => (
            Platform: platform,
            Url: curated.TryGetValue(platform, out string? url) ? url : GenerateSearchUrl(platform, query)
        ))
        .ToList();
}

private static string GenerateSearchUrl(EnumStreamingPlatform platform, string query) => platform switch
{
    EnumStreamingPlatform.Spotify => $"https://open.spotify.com/search/{query}",
    EnumStreamingPlatform.AppleMusic => $"https://music.apple.com/search?term={query}",
    EnumStreamingPlatform.YoutubeMusic => $"https://music.youtube.com/search?q={query}",
    EnumStreamingPlatform.Tidal => $"https://listen.tidal.com/search?q={query}",
    _ => throw new ArgumentOutOfRangeException(nameof(platform)),
};
```

## Wiring into the by-slug endpoint

`PublicGetLyricsBySlugHandler` (already extended twice, in specs 01/02) gains one more resolution
step. `AlbumTracks` only makes sense for an album release, but `StreamingLinks` is resolved either
way — album tracks when `lyrics.AlbumId` is set, or the single's own curated/generated links
otherwise:

```csharp
AlbumTrackDto[] albumTracks = Array.Empty<AlbumTrackDto>();
StreamingLinkDto[] streamingLinks;

if (lyrics.AlbumId is Guid albumId)
{
    AlbumEntity? album = await albumRepository.GetByIdAsync(albumId, cancellationToken);

    if (album is not null)
    {
        List<LyricsEntity> tracks = await lyricsRepository.GetPublishedByAlbumAsync(
            albumId, excludeLyricsId: lyrics.Id, cancellationToken);
        albumTracks = tracks.Select(t => new AlbumTrackDto(t.Slug, t.SongTitle)).ToArray();
    }

    IReadOnlyDictionary<EnumStreamingPlatform, string> curated =
        await streamingLinkRepository.GetByAlbumAsync(albumId, cancellationToken);
    streamingLinks = ResolveStreamingLinks(lyrics.ArtistName, album?.Name ?? lyrics.SongTitle, curated)
        .Select(r => new StreamingLinkDto(r.Platform.ToString(), r.Url))
        .ToArray();
}
else
{
    IReadOnlyDictionary<EnumStreamingPlatform, string> curated =
        await streamingLinkRepository.GetByLyricsAsync(lyrics.Id, cancellationToken);
    streamingLinks = ResolveStreamingLinks(lyrics.ArtistName, lyrics.SongTitle, curated)
        .Select(r => new StreamingLinkDto(r.Platform.ToString(), r.Url))
        .ToArray();
}
```

`ILyricsRepository.GetPublishedByAlbumAsync(Guid albumId, Guid excludeLyricsId, ...)` — a plain
`WHERE AlbumId = @albumId AND Id != @excludeLyricsId AND Status = Published ORDER BY CreatedAt`,
no pagination (an album has a bounded, small track count).

`IStreamingLinkRepository.GetByLyricsAsync(Guid lyricsId, ...)` mirrors `GetByAlbumAsync` —
`WHERE LyricsId = @lyricsId`, keyed into a dictionary by `Platform`.

## Migration

```bash
dotnet ef migrations add AddStreamingLinksAndAlbumLink \
  --project src/Modules/Content/Content \
  --startup-project src/Api \
  --context ContentDbContext
```

## Task checklist

- [x] `StreamingLinkEntity` (nullable `AlbumId`/`LyricsId`, `ForAlbum`/`ForSingle` factories),
  `EnumStreamingPlatform` + `StreamingLinkConfiguration` (both unique indexes, the
  exactly-one-target check constraint, `Cascade` FKs since a streaming link has no meaning without
  its parent release)
- [x] `IStreamingLinkRepository`: `GetByAlbumAndPlatformAsync`, `GetByAlbumAsync`,
  `GetByLyricsAndPlatformAsync`, `GetByLyricsAsync`, `AddAsync`, `Update`
- [x] `AdminUpsertAlbumStreamingLinkCommand`/`Handler`/`EndpointV1`
  (`PUT /api/v1/admin/albums/{id}/streaming-links/{platform}`)
- [x] `AdminUpsertSingleStreamingLinkCommand`/`Handler`/`EndpointV1`
  (`PUT /api/v1/admin/lyrics/{id}/streaming-links/{platform}`) — rejects a `LyricsId` whose
  `AlbumId` is set, via a new `LyricsErrors.BelongsToAlbum()`
- [x] `DELETE /api/v1/admin/albums/{id}/streaming-links/{platform}`
- [x] `DELETE /api/v1/admin/lyrics/{id}/streaming-links/{platform}`
- [x] `ResolveStreamingLinks` helper — always returns all four platforms, for either release kind
  (shipped as `StreamingLinkResolver.ResolveStreamingLinks`, a shared static class, not nested in a
  handler, since spec 12 §2's future affiliate-param change also needs to reach it)
- [x] `ILyricsRepository.GetPublishedByAlbumAsync`
- [x] `PublicGetLyricsBySlugResponse` gains `AlbumTracks` (empty when `AlbumId` is null) and
  `StreamingLinks` (never empty by omission — always resolved, for albums and singles alike)
- [x] Migration `AddStreamingLinksAndAlbumLink`
- [x] Integration tests: a stored link takes precedence over the generated fallback for both an
  album and a standalone single; a missing platform link falls back to a valid generated URL for
  all four platforms in both cases; `AlbumTracks` excludes the current song and is empty when
  `AlbumId` is null; deleting a stored link reverts that platform to the generated fallback on the
  next read; `AdminUpsertSingleStreamingLinkCommand` rejects a lyrics id that belongs to an album

**Verification, 2026-07-31**: `dotnet build` clean; combined with specs 12/13 in the same phase —
634/635 unit (1 pre-existing unrelated skip), 265/265 integration, zero failures; full suite
6527/6530 unit, 1567/1567 integration.
