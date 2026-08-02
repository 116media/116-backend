# Spec 02 — Video & Artist Slug Resolution

The detail page needs two cross-links — "watch the video" and "view artist" — and both must
resolve with the single by-slug lookup, not a second round trip. `IVideoRepository.GetByIdAsync`
already exists and is already used by `AdminCreateLyricsHandler` (to call
`video.MarkHasLyrics()`), so extending the same handler to also read `video.Slug` costs nothing
extra. The artist side is the same shape, once spec 08's `ArtistEntity`/`LyricsEntity.ArtistId`
land.

## Response-only fields — not on the shared `LyricsDto`

`VideoSlug` and `ArtistSlug` are **detail-page-specific concerns**, not general lyrics data — they
don't belong on `LyricsDto` (which the list endpoint and the video-linked-lyrics endpoint also
return, neither of which needs them). They're added to the by-slug endpoint's own response type
only:

```csharp
/// <summary>
/// Response model for retrieving lyrics by slug.
/// </summary>
/// <param name="Lyrics">The matched lyrics information.</param>
/// <param name="VideoSlug">
/// The slug of the linked video, or null if this lyrics page is standalone or the linked
/// video no longer exists.
/// </param>
/// <param name="ArtistSlug">
/// The slug of the claimed artist profile, or null if <see cref="LyricsEntity.ArtistId" />
/// is unset (the common case — most artists have no claimed profile yet).
/// </param>
public record PublicGetLyricsBySlugResponse(LyricsDto Lyrics, string? VideoSlug, string? ArtistSlug);
```

## Handler

```csharp
public class PublicGetLyricsBySlugHandler(
    ILyricsRepository lyricsRepository,
    IVideoRepository videoRepository,
    IArtistRepository artistRepository,
    IMapper mapper,
    ContentI18n i18n
) : IQueryHandler<PublicGetLyricsBySlugQuery, PublicGetLyricsBySlugResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsBySlugResult> Handle(
        PublicGetLyricsBySlugQuery query, CancellationToken cancellationToken)
    {
        LyricsEntity? lyrics = await lyricsRepository.GetBySlugAsync(
            slug: query.Slug, cancellationToken: cancellationToken);

        if (lyrics is null)
        {
            throw i18n.Lyrics.NotFound(id: Guid.Empty);
        }

        string? videoSlug = null;
        if (lyrics.VideoId is Guid videoId)
        {
            VideoEntity? video = await videoRepository.GetByIdAsync(videoId, cancellationToken);
            videoSlug = video?.Slug;
        }

        string? artistSlug = null;
        if (lyrics.ArtistId is Guid artistId)
        {
            ArtistEntity? artist = await artistRepository.GetByIdAsync(artistId, cancellationToken);
            artistSlug = artist?.Slug;
        }

        return new PublicGetLyricsBySlugResult(
            Lyrics: lyrics.ToLyricsDto(mapper),
            VideoSlug: videoSlug,
            ArtistSlug: artistSlug
        );
    }
}
```

`GetByIdAsync` (not `GetByIdOrThrowAsync`) is used deliberately for both lookups — a stale
`VideoId`/`ArtistId` pointing at a deleted row degrades to `null` (the frontend simply doesn't
render the cross-link) rather than 404ing the entire lyrics page over an unrelated deleted record.
`PublicGetLyricsBySlugResult` gains the same two fields as the response, threaded through from the
query handler to the endpoint.

## Task checklist

- [x] `PublicGetLyricsBySlugResult`/`Response` gain `VideoSlug`, `ArtistSlug`
- [x] Handler resolves `VideoSlug` via `GetByIdAsync` (never `GetByIdOrThrowAsync` — a stale
  reference degrades gracefully, not a 404 of the whole page)
- [x] `ArtistSlug` now resolves for real (spec 08 landed): `IArtistRepository` added to
  `PublicGetLyricsBySlugHandler`'s constructor, resolves via `GetByIdAsync` (never
  `GetByIdOrThrowAsync` — a stale/deleted `ArtistId` degrades to `ArtistSlug: null`, same rule as
  `VideoSlug`), the placeholder-null stopgap and its comment are gone.
- [x] Integration tests: video-linked lyrics resolves `VideoSlug`; lyrics with a deleted linked
  video resolves `VideoSlug: null` without 404ing; standalone lyrics resolves both `VideoSlug` and
  `ArtistSlug` as `null`
- [x] Integration test: artist-claimed lyrics resolves `ArtistSlug`; an unlinked/deleted-artist
  reference resolves `ArtistSlug: null` without 404ing

**Verification, 2026-07-30**: `dotnet build` clean; the pre-existing `Status != Published →
NotFound` gate (Phase 1's bugfix) was explicitly re-confirmed intact after this handler's rewrite.
Covered by the same Lyrics-scoped test run as specs 03/07 (264/264 unit, 112/112 integration, zero
skips).
