# Spec 06 — Similar Lyrics Query

Depends on spec 07 (tags) for the middle branch. A three-way waterfall: same video category when
linked, else shared tags, else the 10 latest standalone (no video) records — each branch tried only
if the previous one produced nothing, never merged.

## Specifications

```csharp
/// <summary>
/// Specification that matches published lyrics linked to a video in the given category,
/// excluding a specific lyrics record.
/// </summary>
public class LyricsSimilarByVideoCategorySpecification(Guid categoryId, Guid excludeId)
    : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId
            && lyrics.Status == EnumContentStatus.Published
            && lyrics.Video != null
            && lyrics.Video.CategoryId == categoryId;
    }
}

/// <summary>
/// Specification that matches published lyrics sharing at least one tag with the given set,
/// excluding a specific lyrics record.
/// </summary>
public class LyricsBySharedTagsSpecification(IReadOnlyCollection<Guid> tagIds, Guid excludeId)
    : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId
            && lyrics.Status == EnumContentStatus.Published
            && lyrics.Tags.Any(t => tagIds.Contains(t.TagId));
    }
}

/// <summary>
/// Specification that matches published, standalone (no linked video) lyrics,
/// excluding a specific lyrics record.
/// </summary>
public class LyricsStandaloneSpecification(Guid excludeId) : Specification<LyricsEntity>
{
    /// <inheritdoc />
    public override Expression<Func<LyricsEntity, bool>> ToExpression()
    {
        return lyrics =>
            lyrics.Id != excludeId
            && lyrics.Status == EnumContentStatus.Published
            && lyrics.VideoId == null;
    }
}
```

## Repository method — the waterfall

```csharp
/// <summary>
/// Retrieves up to 10 lyrics similar to the given one: same video category when linked,
/// else shared tags, else the 10 latest standalone records. Branches are tried in order;
/// the first branch with any results is returned — branches are never merged.
/// </summary>
public async Task<IReadOnlyList<LyricsEntity>> GetSimilarAsync(Guid lyricsId, CancellationToken ct)
{
    LyricsEntity lyrics = await GetByIdOrThrowAsync(lyricsId, ct);

    if (lyrics.VideoId.HasValue)
    {
        VideoEntity? video = await context.Videos
            .FirstOrDefaultAsync(v => v.Id == lyrics.VideoId.Value, ct);

        if (video is not null)
        {
            var byCategory = new LyricsSimilarByVideoCategorySpecification(video.CategoryId, lyricsId);
            List<LyricsEntity> results = await context.Lyrics
                .ApplySpecification(byCategory)
                .OrderByDescending(l => l.CreatedAt)
                .Take(10)
                .ToListAsync(ct);

            if (results.Count > 0)
            {
                return results;
            }
        }
    }

    List<Guid> tagIds = lyrics.Tags.Select(t => t.TagId).ToList();
    if (tagIds.Count > 0)
    {
        var bySharedTags = new LyricsBySharedTagsSpecification(tagIds, lyricsId);
        List<LyricsEntity> byTagResults = await context.Lyrics
            .ApplySpecification(bySharedTags)
            .Select(l => new { Lyrics = l, SharedCount = l.Tags.Count(t => tagIds.Contains(t.TagId)) })
            .OrderByDescending(x => x.SharedCount)
            .ThenByDescending(x => x.Lyrics.CreatedAt)
            .Select(x => x.Lyrics)
            .Take(10)
            .ToListAsync(ct);

        if (byTagResults.Count > 0)
        {
            return byTagResults;
        }
    }

    var standalone = new LyricsStandaloneSpecification(lyricsId);
    return await context.Lyrics
        .ApplySpecification(standalone)
        .OrderByDescending(l => l.CreatedAt)
        .Take(10)
        .ToListAsync(ct);
}
```

**Resolved decision**: a video-linked record with zero same-category matches **does** fall
through to the shared-tags branch, and from there to the standalone branch, exactly like the code
above. All three branches are always tried in order, regardless of whether the source lyrics page
is video-linked; the first non-empty branch wins. A video-linked song is never scoped exclusively
to its video's category — it can still surface tag-based or latest-standalone matches when its
category has no other video-linked peers. This keeps the "renders nothing when empty" frontend
case rare (only when none of the three branches yield anything) rather than the common case for
video-linked songs in a niche category.

## Endpoint

```csharp
group.MapGet(
    "/{id}/similar",
    async (Guid id, IDispatcher dispatcher) =>
    {
        var query = new PublicGetSimilarLyricsQuery(LyricsId: id);
        PublicGetSimilarLyricsResult result = await dispatcher.Send(query);
        return Results.Ok(new PublicGetSimilarLyricsResponse(Lyrics: result.Lyrics));
    }
)
.WithName(PublicGetSimilarLyricsMetaField.GetSimilarLyrics.Name)
.AllowAnonymous()
.RequireRateLimiting(RateLimitPolicies.ContentBrowsing)
.Produces<PublicGetSimilarLyricsResponse>(StatusCodes.Status200OK)
.ProducesProblem(StatusCodes.Status404NotFound);
```

`GET /api/v1/public/lyrics/{id}/similar` — a sibling route in the same group as the by-slug and
by-video-id lookups. Returns an **empty list**, never a 404, when nothing matches — absence of
similar content is normal, not an error; only a missing `{id}` itself 404s (via
`GetByIdOrThrowAsync` inside the handler).

## Task checklist

- [x] `LyricsSimilarByVideoCategorySpecification`, `LyricsBySharedTagsSpecification`,
  `LyricsStandaloneSpecification`
- [x] `ILyricsRepository.GetSimilarAsync` — video-linked fallthrough behavior resolved and
  documented (see "Resolved decision" note above): all three branches always tried in order
- [x] `PublicGetSimilarLyricsQuery`/`Handler`/`EndpointV1` (`GET /api/v1/public/lyrics/{id}/similar`)
- [x] Integration tests: video-category branch returns matches sorted by recency; shared-tags
  branch ranks by shared-tag count then recency; zero-tag standalone record falls through to
  latest-standalone; a record with no matches in any branch returns an empty list, not a 404 or
  error; the resolved fallthrough behavior is explicitly tested — a video-linked record with zero
  same-category matches but shared tags returns the tag-based matches, proving it falls through

**Verification, 2026-07-31**: `dotnet build` clean; covered by the same Lyrics-scoped test run as
specs 04/05 (376/376 unit, 177/177 integration, zero skips) since all three shipped in the same
implementation pass.
