# 06 — DTO and Mapper Changes

## CategoryDto

**File:** `src/Modules/Content/Content/Application/Shared/DTOs/CategoryDto.cs`

Add `IsPinnedToFeed` and `PinnedToFeedAt` after `IsExclusive`:

```csharp
public record CategoryDto(
    Guid Id,
    Guid ContentTypeId,
    string ContentTypeName,
    string Name,
    string Slug,
    string Description,
    bool IsFree,
    bool IsActive,
    bool IsGossip,
    bool IsExclusive,
    bool IsPinnedToFeed,             // <-- new
    DateTimeOffset? PinnedToFeedAt,  // <-- new
    string? PosterUrl,
    IReadOnlyList<CategoryPricingDto> Pricing
);
```

- `IsPinnedToFeed` lets dashboard list views render an "in feed" badge and toggle state without recomputing from the timestamp.
- `PinnedToFeedAt` lets the dashboard show ordering / "pinned on" and sort the curation list.

Update the XML doc params on the record to document both new fields (the codebase
documents every `CategoryDto` param).

## CategoryMapper

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/CategoryMapper.cs`

**No mapper code changes are required.** Both new DTO fields map by name from the entity:

- `PinnedToFeedAt` → `CategoryEntity.PinnedToFeedAt` (direct property match), and
- `IsPinnedToFeed` → `CategoryEntity.IsPinnedToFeed` (the `[NotMapped]` computed getter is still a
  readable C# property, so Mapster projects it normally — `[NotMapped]` only affects EF, not Mapster).

The existing async `ToCategoryDtoAsync` / `ToCategoryDtosAsync` extension methods (added by
the exclusive-category work) already build the DTO via `mapper.Map<CategoryDto>(entity)` and
then resolve `PosterUrl`. The two new fields flow through that same `mapper.Map` call
untouched.

> Verify the Mapster `NewConfig<CategoryEntity, CategoryDto>()` registration does not
> `.Ignore(...)` unlisted members in strict mode. The existing config explicitly maps
> `ContentTypeName`, `Pricing`, and `PosterUrl`; `IsPinnedToFeed` and `PinnedToFeedAt` rely on
> default name-matching, which is how `IsExclusive` already maps. If the project later
> enables Mapster strict mapping, add explicit `.Map(...)` lines for both.

## Batch file fetch (feed performance)

The feed handler resolves every poster + thumbnail URL in **one** query instead of one
per item (see [05-public-video-feed-query.md](05-public-video-feed-query.md)). That needs
a batch method on the Core file repository.

**File:** `src/Modules/Core/.../IFileRepository.cs` (+ implementation)

```csharp
/// <summary>
/// Retrieves multiple files by id in a single query, keyed by file id.
/// Ids that do not resolve are simply absent from the dictionary.
/// </summary>
/// <param name="fileIds">The file identifiers to fetch.</param>
/// <param name="cancellationToken">Token to observe for cancellation requests.</param>
/// <returns>A read-only map of file id to <see cref="FileEntity" />.</returns>
Task<IReadOnlyDictionary<Guid, FileEntity>> GetByIdsAsync(
    IReadOnlyCollection<Guid> fileIds,
    CancellationToken cancellationToken = default
);
```

Implementation:

```csharp
public async Task<IReadOnlyDictionary<Guid, FileEntity>> GetByIdsAsync(
    IReadOnlyCollection<Guid> fileIds,
    CancellationToken cancellationToken = default
)
{
    if (fileIds.Count == 0)
    {
        return new Dictionary<Guid, FileEntity>();
    }

    return await _context.Files
        .Where(f => fileIds.Contains(f.Id))
        .ToDictionaryAsync(f => f.Id, cancellationToken);
}
```

## Map-from-dictionary mapper overloads (no IO)

The existing `ToCategoryDtoAsync` / `ToVideoSummaryDtoAsync` methods each call
`fileRepository.GetByIdAsync` internally — fine for single-entity reads, but an N+1 when
mapping a whole feed. Add **synchronous** overloads that take the pre-fetched file map and
do no IO. The feed handler calls these after the single batch fetch.

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/CategoryMapper.cs`

```csharp
/// <summary>
/// Maps a CategoryEntity to a CategoryDto, resolving PosterUrl from a pre-fetched file map.
/// Performs no IO — intended for batch mapping where files are loaded once up front.
/// </summary>
public static CategoryDto ToCategoryDto(
    this CategoryEntity entity,
    IMapper mapper,
    IReadOnlyDictionary<Guid, FileEntity> files
)
{
    var dto = mapper.Map<CategoryDto>(entity)
        with { Pricing = mapper.Map<IReadOnlyList<CategoryPricingDto>>(entity.Pricing) };

    if (entity.PosterFileId is { } posterId && files.TryGetValue(posterId, out FileEntity? poster))
    {
        dto = dto with { PosterUrl = poster.StorageUrl };
    }

    return dto;
}
```

**File:** `src/Modules/Content/Content/Application/Shared/Mappers/VideoMapper.cs`

```csharp
/// <summary>
/// Maps a VideoEntity to a VideoSummaryDto, resolving ThumbnailUrl from a pre-fetched
/// file map. Performs no IO — intended for batch mapping (e.g. the video feed).
/// </summary>
public static VideoSummaryDto ToVideoSummaryDto(
    this VideoEntity entity,
    IMapper mapper,
    IReadOnlyDictionary<Guid, FileEntity> files
)
{
    string? thumbnailUrl =
        entity.ThumbnailFileId is { } thumbId && files.TryGetValue(thumbId, out FileEntity? thumb)
            ? thumb.StorageUrl
            : null;

    return new VideoSummaryDto(
        entity.Id,
        entity.CategoryId,
        entity.Category != null ? entity.Category.Name : string.Empty,
        entity.Title,
        entity.Slug,
        thumbnailUrl,
        entity.AuthorId.ToString(),
        entity.Status,
        entity.YoutubeVideoUrl,
        entity.IsPromoted,
        entity.HasLyrics,
        entity.PublishedAt,
        entity.ShootingScheduledAt,
        entity.ShareCount,
        entity.RatingAverage,
        entity.RatingCount
    )
    {
        CreatedAt = entity.CreatedAt,
        CreatedBy = entity.CreatedBy,
        UpdatedAt = entity.UpdatedAt,
        UpdatedBy = entity.UpdatedBy,
    };
}
```

> This mirrors the existing async `ToVideoSummaryDtoAsync` field-for-field; only the thumbnail
> resolution differs (map lookup vs. `GetByIdAsync`). The `GetLatestPublishedByCategoryAsync`
> repo method eager-loads `Category`, so `entity.Category.Name` is populated.
>
> The original async methods stay — single-entity callers (e.g. `GetExclusiveCategory`,
> `GetVideoById`) keep using them. Only batch callers switch to the dictionary overloads.

## VideoSummaryDto

No change to the DTO itself. The feed reuses the existing `VideoSummaryDto`.

## Feed section DTO

`VideoFeedSectionDto` and `PublicGetVideoFeedResult` are defined alongside the query in
[05-public-video-feed-query.md](05-public-video-feed-query.md), not here — they are
query-specific response shapes, following the same placement as
`VideoPromotionSpotDto` in the promotion-feed query file.
