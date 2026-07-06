# 05 — Public Video Feed Query

The public feed endpoint returns an ordered list of **sections** — one per pinned
video category — each carrying up to 8 latest published videos. It follows the same
CQRS + Carter shape as `PublicGetVideoPromotionFeed` and `PublicGetExclusiveCategory`.

```
src/Modules/Content/Content/Application/Editorial/UseCases/Public/Queries/GetVideoFeed/
├── PublicGetVideoFeedQuery.cs
├── PublicGetVideoFeedHandler.cs
├── PublicGetVideoFeedMetaField.cs
└── V1/
    └── PublicGetVideoFeedEndpointV1.cs
```

## Endpoint

```
GET /api/v1/public/videos/feed
```

| Field        | Value                                            |
|--------------|--------------------------------------------------|
| Method       | `GET`                                            |
| Auth         | `AllowAnonymous`                                 |
| Rate limit   | `ContentBrowsing`                                |
| Query params | none (videos-per-section is fixed at 8)          |
| Response     | `PublicGetVideoFeedResponse` — list of sections  |

Add the route segment to `EditorialRouteConstants`:

```csharp
/// <summary>
/// Route segment for the homepage content feed endpoint.
/// Example: /api/v1/public/videos/feed.
/// </summary>
public const string Feed = "feed";
```

## Query and Result DTOs

**File:** `.../GetVideoFeed/PublicGetVideoFeedQuery.cs`

```csharp
/// <summary>
/// Query for retrieving the public video feed: pinned video categories, each with
/// its latest published videos.
/// </summary>
public record PublicGetVideoFeedQuery : IQuery<PublicGetVideoFeedResult>;

/// <summary>
/// A single feed section: one pinned category and its latest published videos.
/// </summary>
/// <param name="Category">The pinned category metadata.</param>
/// <param name="Videos">Up to 8 latest published videos in the category, newest first.</param>
public record VideoFeedSectionDto(CategoryDto Category, IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Result of the <see cref="PublicGetVideoFeedQuery" />.
/// </summary>
/// <param name="Sections">
/// Ordered feed sections (most recently pinned category first). Sections whose
/// category has no published videos are omitted.
/// </param>
public record PublicGetVideoFeedResult(IReadOnlyList<VideoFeedSectionDto> Sections);
```

## Handler

**File:** `.../GetVideoFeed/PublicGetVideoFeedHandler.cs`

```csharp
public class PublicGetVideoFeedHandler(
    ICategoryRepository categoryRepository,
    IVideoRepository videoRepository,
    IFileRepository fileRepository,
    IMapper mapper
) : IQueryHandler<PublicGetVideoFeedQuery, PublicGetVideoFeedResult>
{
    private const int VideosPerSection = EditorialFeedConstants.MaxVideosPerFeedSection;

    /// <inheritdoc />
    public async Task<PublicGetVideoFeedResult> Handle(
        PublicGetVideoFeedQuery query,
        CancellationToken cancellationToken
    )
    {
        // Query 1 — pinned categories (ContentType is eager-loaded by the repo). The pinned
        // set is capped at 5 per content type, so filtering to Video in memory is trivial and
        // avoids a separate content-type lookup query.
        IReadOnlyList<CategoryEntity> pinned = await categoryRepository.GetPinnedToFeedCategoriesAsync(
            cancellationToken: cancellationToken
        );

        List<CategoryEntity> videoCategories = pinned
            .Where(c => c.ContentType.Name == nameof(EnumCoreContentType.Video))
            .ToList();

        if (videoCategories.Count == 0)
        {
            return new PublicGetVideoFeedResult(Sections: []);
        }

        // Queries 2..N — latest published videos per category. Bounded by the cap (≤ 5),
        // so this is a small, fixed number of indexed lookups, not an unbounded N+1.
        var videosByCategory = new Dictionary<Guid, IReadOnlyList<VideoEntity>>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            videosByCategory[category.Id] = await videoRepository.GetLatestPublishedByCategoryAsync(
                categoryId: category.Id,
                limit: VideosPerSection,
                cancellationToken: cancellationToken
            );
        }

        // One query for ALL file URLs (category posters + video thumbnails) instead of one
        // per item. This is the change that removes the real N+1.
        var fileIds = videoCategories
            .Where(c => c.PosterFileId.HasValue)
            .Select(c => c.PosterFileId!.Value)
            .Concat(
                videosByCategory.Values
                    .SelectMany(videos => videos)
                    .Where(v => v.ThumbnailFileId.HasValue)
                    .Select(v => v.ThumbnailFileId!.Value)
            )
            .Distinct()
            .ToList();

        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            fileIds: fileIds,
            cancellationToken: cancellationToken
        );

        // Pure in-memory assembly — the map-from-dictionary overloads do no IO.
        var sections = new List<VideoFeedSectionDto>(videoCategories.Count);

        foreach (CategoryEntity category in videoCategories)
        {
            IReadOnlyList<VideoEntity> videos = videosByCategory[category.Id];

            // Omit empty sections so the UI never renders a blank block.
            if (videos.Count == 0)
            {
                continue;
            }

            CategoryDto categoryDto = category.ToCategoryDto(mapper, files);
            IReadOnlyList<VideoSummaryDto> videoDtos = videos
                .Select(v => v.ToVideoSummaryDto(mapper, files))
                .ToList();

            sections.Add(new VideoFeedSectionDto(Category: categoryDto, Videos: videoDtos));
        }

        return new PublicGetVideoFeedResult(Sections: sections);
    }
}
```

### What the optimization changes

| Concern | Before | After |
|---------|--------|-------|
| Content-type filter | extra query via a non-existent `IContentTypeRepository` | in-memory filter on the already-loaded `ContentType` nav (0 extra queries) |
| File URLs (posters + thumbnails) | one `GetByIdAsync` per item (~45 round trips) | one `GetByIdsAsync` batch (1 round trip) |
| Total queries (full feed) | ~50 | ~7 (1 categories + ≤5 videos + 1 files) |

### Supporting changes this handler needs

1. **`IFileRepository.GetByIdsAsync`** (Core module) — batch fetch returning a `Guid → FileEntity` map. See [06-dto-and-mapper.md](06-dto-and-mapper.md).
2. **Map-from-dictionary mapper overloads** — `ToCategoryDto(mapper, files)` and `ToVideoSummaryDto(mapper, files)` that resolve URLs from the pre-fetched map and do **no** IO. See [06-dto-and-mapper.md](06-dto-and-mapper.md). These are reusable — the promotion feed has the same N+1 and can adopt them later.

### Notes

- **Min-videos gate is enforced at pin time, not here.** A category can only be pinned with ≥ `MinVideosToPinToFeed` published videos ([04](04-admin-pin-category.md)), so sections are well-populated by construction. The feed still applies the `MaxVideosPerFeedSection` display cap and the empty-section omission as defensive measures (e.g. if videos are unpublished or archived after the category was pinned).
- **Ordering.** Sections inherit the `PinnedToFeedAt`-descending order from `GetPinnedToFeedCategoriesAsync` (most recently pinned first — see decision #4 in [00-overview.md](00-overview.md)).
- **Keep the per-category video loop sequential.** A single `DbContext` is not thread-safe, so do **not** wrap the loop in `Task.WhenAll` over the same context. The loop is ≤ 5 iterations and each query is indexed, so it is not the bottleneck — the batched file fetch is the meaningful win.
- **Further step (only if profiling demands it).** The ≤5 per-category video queries can be collapsed into one round trip with a top-N-per-category (`LATERAL`) query. EF Core's translation of "top N per group" is shape-sensitive, so this is deliberately left as an optional follow-up rather than the default — the constant ~7 queries above is already well within budget.

## Endpoint (Carter)

**File:** `.../GetVideoFeed/V1/PublicGetVideoFeedEndpointV1.cs`

Mirror `PublicGetVideoPromotionFeedEndpointV1` (same group, `AllowAnonymous`, rate limit).

```csharp
public record PublicGetVideoFeedResponse(IReadOnlyList<VideoFeedSectionDto> Sections);

public class PublicGetVideoFeedEndpointV1 : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapApiVersionGroup(1)
            .MapGroup($"{ContentConstants.Public}/{EditorialRouteConstants.Videos}")
            .WithTags($"{ContentConstants.Public}::{EditorialRouteConstants.Videos}");

        group
            .MapGet(
                $"/{EditorialRouteConstants.Feed}",
                async (IDispatcher dispatcher) =>
                {
                    PublicGetVideoFeedResult result = await dispatcher.Send(request: new PublicGetVideoFeedQuery());
                    return Results.Ok(new PublicGetVideoFeedResponse(Sections: result.Sections));
                }
            )
            .WithName(endpointName: PublicGetVideoFeedMetaField.GetVideoFeed.Name)
            .WithSummary(summary: PublicGetVideoFeedMetaField.GetVideoFeed.Summary)
            .WithDescription(description: PublicGetVideoFeedMetaField.GetVideoFeed.Description)
            .AllowAnonymous()
            .RequireRateLimiting(policyName: RateLimitPolicies.ContentBrowsing)
            .Produces<PublicGetVideoFeedResponse>(statusCode: StatusCodes.Status200OK)
            .ProducesProblem(statusCode: StatusCodes.Status429TooManyRequests);
    }
}
```

## Future: article feed

`GET /api/v1/public/articles/feed` is the same handler shape with two swaps:

1. resolve the `Article` content type instead of `Video`, and
2. fetch latest published **articles** per category (article summary DTO + mapper).

No additional entity, migration, or admin work is required — the `PinnedToFeedAt` flag and
the per-content-type cap already cover article categories.
