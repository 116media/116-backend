# Content Module — Implementation Progress

> Read this file at the start of every session to restore full context.
> Last updated: 2026-03-13

---

## Sub-Modules Status

| Sub-module | Status |
|---|---|
| Lookup | ✅ Complete (17/17 endpoints) |
| Catalog | ✅ Complete (25/25 endpoints) |
| **Editorial** | ✅ Complete — all phases done, build 0 errors |
| Commerce | ⏳ Not started |
| Interactions | ⏳ Not started |

---

## Editorial Sub-Module — COMPLETE

### Architecture decisions (apply consistently)

- **Draft-first articles**: `POST` creates shell (`body=''`, `headline=''`), `PUT` fills content
- **`author_id`**: TEXT, no FK — read from `HttpContext.User` JWT claims in handler, never passed by client
- **`storage_key` naming**: all media identifiers named `*StorageKey` / `*_storage_key` — CDN-agnostic
- **Image diff on PUT**: compare body URLs vs `article_images` table, delete removed from Cloudinary **after** DB commit
- **Hard delete order**: load `storage_key`s → `ICloudinaryService.DeleteImagesAsync()` → then DB delete (CASCADE wipes `article_images`)
- **Archive ≠ delete**: archive only sets `status = Archived`, never touches Cloudinary
- **YouTube thumbnail**: download via `HttpClient`, re-upload to Cloudinary via `UploadImageAsync` (creates `FormFile` from bytes). Never store YouTube CDN URLs directly.
- **`ICloudinaryService`**: keep name as-is, has `UploadImageAsync(IFormFile, publicId, folder?)`, `DeleteImageAsync(storageKey)`, `DeleteImagesAsync(storageKeys)`
- **Concurrent batch delete**: `Task.WhenAll` fires all batches of 100 in parallel (not sequential loop)
- **`CloudinaryUploadResult`**: properties are `PublicId` (= storage key) and `SecureUrl` (= url)

---

## All Completed Phases

### Phase 0 — Core Module: ICloudinaryService delete methods ✅
- `ICloudinaryService.cs`: Added `DeleteImageAsync` and `DeleteImagesAsync`
- `CloudinaryService.cs`: Implemented with concurrent `Task.WhenAll` batching

### Phase 1 — Domain Enums ✅
- `EnumContentStatus.cs`: Draft, PendingPayment, PendingReview, Approved, Published, Rejected, Archived
- `EnumArticleImageType.cs`: Cover = 0, Body = 1

### Phase 2 — Domain Entities ✅
- `ArticleImageEntity.cs` — `Entity<Guid>`, factory `Create(id, articleId, storageKey, url, imageType)`
- `ArticleTagEntity.cs` — junction, composite PK (ArticleId, TagId), no factory (use object init)
- `VideoTagEntity.cs` — junction, composite PK (VideoId, TagId), no factory
- `ArticleEntity.cs` — `Aggregate<Guid>`, two factories + status methods + counter methods
- `VideoEntity.cs` — `Aggregate<Guid>`, Publish() guards YoutubeVideoId at domain level
- `ShortVideoEntity.cs` — `Aggregate<Guid>`, CreateStandalone/CreateTeaser, Activate()/Deactivate() return bool
- `LyricsEntity.cs` — `Aggregate<Guid>`, three factories, UpdateLyrics/UpdateSeo

### Phase 3 — EF Configurations ✅
Location: `Infrastructure/Persistence/Configurations/`
- `ArticleTagConfiguration.cs`, `VideoTagConfiguration.cs`
- `ArticleImageConfiguration.cs`, `ArticleConfiguration.cs`
- `VideoConfiguration.cs`, `ShortVideoConfiguration.cs`, `LyricsConfiguration.cs`
- Existing: `ShortVideoConfiguration.cs` and `LyricsConfiguration.cs` were already present (updated)

### Phase 4 — ContentDbContext updated ✅
Added DbSets: Articles, ArticleImages, ArticleTags, Videos, VideoTags, ShortVideos, Lyrics

### Phase 5 — EF Migration ✅
Migration `AddEditorialEntities` created and applied

### Phase 6 — Repository Interfaces ✅
- `IArticleRepository.cs` — includes image and tag sub-methods, GetAbandonedDraftsAsync
- `IVideoRepository.cs` — includes tag sub-methods
- `IShortVideoRepository.cs`
- `ILyricsRepository.cs` — includes GetBySongTitleAndArtistAsync
- `ILookupRepository.cs` — added `GetTagByIdOrThrowAsync`

### Phase 7 — Repository Implementations ✅
- `ArticleRepository.cs`, `VideoRepository.cs`, `ShortVideoRepository.cs`, `LyricsRepository.cs`
- `LookupRepository.cs` — added `GetTagByIdOrThrowAsync` + `TagByIdSpecification`

### Phase 8 — Specifications ✅
Location: `Application/Editorial/Specifications/`
- `ArticleSpecifications.cs` — ById, BySlug, ByStatus, ByCategory, Promoted, AbandonedDraft
- `VideoSpecifications.cs` — ById, BySlug, ByStatus, ByCategory, Promoted
- `ShortVideoSpecifications.cs` — ById, Active
- `LyricsSpecifications.cs` — ById, BySongAndArtist

### Phase 9 — DTOs ✅
- `ArticleImageDto.cs`, `ArticleSummaryDto.cs`, `ArticleDetailDto.cs`
- `VideoSummaryDto.cs`, `VideoDetailDto.cs`
- `ShortVideoDto.cs`, `LyricsDto.cs`

### Phase 10 — Mappers ✅
- `ArticleMapper.cs`, `VideoMapper.cs`, `ShortVideoMapper.cs`, `LyricsMapper.cs`
- `MappingRegistration.cs` updated to register all four

### Phase 12 — Route Constants ✅
- `EditorialRouteConstants.cs`: Articles, Videos, Shorts, Lyrics

### Phase 13 — Article Critical Use Cases ✅ (6 endpoints)
- `CreateArticle` POST, `UploadArticleImage` POST (multipart), `SubmitArticle` PATCH
- `ApproveArticle` PATCH, `PublishArticle` PATCH, `RejectArticle` PATCH

### Phase 14 — Article Admin Management ✅ (7 endpoints)
- `GetAllArticlesAdmin` GET, `GetArticleByIdAdmin` GET
- `UpdateArticle` PUT (with image diff + Cloudinary cleanup), `UpdateArticleSeo` PATCH
- `UpdateArticleTags` PUT (clear + replace), `ArchiveArticle` PATCH, `DeleteArticle` DELETE

### Phase 15 — Article Public ✅ (3 endpoints)
- `GetPublishedArticles` GET (anon), `GetArticleBySlug` GET (anon), `GetPromotedArticles` GET (anon)

### Phase 16 — Video Critical Use Cases ✅ (6 endpoints)
- `CreateVideo` POST, `SubmitVideo` PATCH, `ApproveVideo` PATCH
- `AttachYoutubeId` PATCH (downloads YouTube thumbnail via HttpClient, re-uploads), `RejectVideo` PATCH, `PublishVideo` PATCH

### Phase 17 — Video Admin Management ✅ (9 endpoints)
- `GetAllVideosAdmin` GET, `GetVideoByIdAdmin` GET
- `UpdateVideo` PUT, `UploadVideoThumbnail` POST (multipart)
- `ScheduleShoot` PATCH, `UpdateVideoSeo` PATCH, `UpdateVideoTags` PUT
- `ArchiveVideo` PATCH, `DeleteVideo` DELETE

### Phase 18 — Video Public ✅ (3 endpoints)
- `GetPublishedVideos` GET (anon), `GetVideoBySlug` GET (anon), `GetPromotedVideos` GET (anon)

### Phase 19 — Short Video Use Cases ✅ (7 endpoints)
- `CreateShortVideo` POST (multipart), `UploadShortVideoThumbnail` POST
- `ActivateShortVideo` PATCH, `DeactivateShortVideo` PATCH, `DeleteShortVideo` DELETE
- `GetAllShortsAdmin` GET, `GetPublicShorts` GET (anon)

### Phase 20 — Lyrics Use Cases ✅ (5 endpoints)
- `CreateLyrics` POST, `UpdateLyrics` PUT, `UpdateLyricsSeo` PATCH
- `GetAllLyricsAdmin` GET, `GetLyricsBySlug` GET (anon, `/{songTitle}/{artistName}`)

### Phase 21 — Background Cleanup Job ✅ (Quartz.NET)
- `AbandonedDraftCleanupJob.cs` — implements `IScheduledJob` (Quartz `IJob`), runs every hour via cron `"0 0 * * * ?"`, purges drafts with empty body+headline older than **7 days**
- `[DisallowConcurrentExecution]` prevents overlapping runs
- Deletes Cloudinary assets before DB remove, uses `IServiceScopeFactory` for scoped dependencies
- **Quartz.AspNetCore 3.16.1** installed in `Shared/Shared.csproj` (not in individual modules)
- `IScheduledJob` interface at `Shared/Shared/Application/Jobs/IScheduledJob.cs` — extends `Quartz.IJob`
- `QuartzExtension.AddScheduledJob<TJob>(cronExpression)` at `Shared/Shared/Application/Extensions/QuartzExtension.cs` — fully generic, uses `typeof(TJob).Name` for job/trigger identity

### Phase 22 — Module Registration ✅
`ContentModule.cs` updated:
- `AddScoped<IArticleRepository, ArticleRepository>()`
- `AddScoped<IVideoRepository, VideoRepository>()`
- `AddScoped<IShortVideoRepository, ShortVideoRepository>()`
- `AddScoped<ILyricsRepository, LyricsRepository>()`
- `AddScheduledJob<AbandonedDraftCleanupJob>(cronExpression: "0 0 * * * ?")`

---

## Final Build Status
```
Build succeeded. 0 Warning(s) 0 Error(s)
```

---

## Key File Locations

| Component | Path |
|---|---|
| Entities | `src/Modules/Content/Content/Domain/Entities/` |
| Enums | `src/Modules/Content/Content/Domain/Enums/` |
| EF Configs | `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/` |
| Repository interfaces | `src/Modules/Content/Content/Application/Shared/Repositories/` |
| Repository impls | `src/Modules/Content/Content/Infrastructure/Repositories/` |
| Specifications | `src/Modules/Content/Content/Application/Editorial/Specifications/` |
| DTOs | `src/Modules/Content/Content/Application/Shared/DTOs/` |
| Mappers | `src/Modules/Content/Content/Application/Shared/Mappers/` |
| Errors | `src/Modules/Content/Content/Application/Shared/Errors/` |
| Route constants | `src/Modules/Content/Content/Application/Editorial/Constants/` |
| Admin use cases | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/` |
| Public use cases | `src/Modules/Content/Content/Application/Editorial/UseCases/Public/` |
| Background job | `src/Modules/Content/Content/Infrastructure/BackgroundJobs/` |
| Module registration | `src/Modules/Content/Content/ContentModule.cs` |

---

## Remaining Work (Next Sub-modules)

### Commerce Sub-module
- Customer order flow, payment verification
- Stamps `SocialBoost` / `IsPromoted` on articles/videos after payment confirmed

### Interactions Sub-module
- Likes, comments, bookmarks, ratings, shares
- Counter increment/decrement handlers feed into denormalized counters on ArticleEntity/VideoEntity

### Content module needs `Content.csproj` → `Core.csproj` project reference (already added for ICloudinaryService)
