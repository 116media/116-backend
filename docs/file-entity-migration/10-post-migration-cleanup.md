# Post-Migration Cleanup Plan

Issues identified after the FileEntity migration was implemented.
Each phase is independent and can be merged separately.

---

## Phase A — Fix `MaxStorageKeyLength` (100, not 500)

**Why:** `StorageKey` stores a Cloudinary public ID, which is a short folder path
like `content/article-images/some-uuid`. 500 is wildly oversized; 100 is sufficient.

### Files

| File | Change |
|------|--------|
| `src/BuildingBlocks/Constants/FileConstants.cs` | `MaxStorageKeyLength = 500` → `100` |

The `[MaxLength]` attribute on `FileEntity.StorageKey` and the EF config in
`FileConfiguration.cs` both reference `FileConstants.MaxStorageKeyLength`, so they
update automatically — no other edits needed.

---

## Phase B — Remove `CoverImageUrl` from `ArticleEntity`

**Why:** Now that cover images are tracked via `CoverImageFileId` → `FileEntity`,
`CoverImageUrl` is redundant. The URL lives on `FileEntity.StorageUrl`. Keeping both
means they can drift out of sync (e.g., `ArticleEntity.Update()` accepts and sets
`coverImageUrl` independently of `UpdateCoverImage()`).

### Files

| File | Change |
|------|--------|
| `ArticleEntity.cs` | Remove `CoverImageUrl` property. Remove `coverImageUrl` param from `Update()`. Remove `coverImageUrl` param from `UpdateCoverImage()` (keep only `coverImageFileId`). |
| `ArticleConfiguration.cs` | Remove `CoverImageUrl` property config line. |
| `AdminUploadArticleImageHandler.cs` | `HandleCoverImage()` — stop setting `CoverImageUrl`. |
| `AdminUpdateArticleHandler.cs` | Remove `coverImageUrl` from `article.Update()` call. Remove old-cover-URL diff logic (lines 87-97) — cover replacement is handled by the upload handler, not the update handler. |
| `AdminUpdateArticleCommand.cs` | Remove `CoverImageUrl` param. |
| `AdminUpdateArticleEndpointV1.cs` | Remove `CoverImageUrl` from request DTO and mapping. |
| `ArticleSummaryDto.cs` | Replace `CoverImageUrl` with `CoverImageFileId` (or remove — the frontend should resolve the URL from a file endpoint/join). |
| `ArticleDetailDto.cs` | Same as above. |
| `ArticleMapper.cs` | Update mappings — map `CoverImageFileId` instead of `CoverImageUrl`. |
| `ContentConstants.cs` | Remove `MaxCoverImageUrlLength = 500` (dead constant). |
| `AbandonedDraftCleanupJob.cs` | No change — already uses `CoverImageFileId`. |

### DTO decision

The DTOs currently return `CoverImageUrl` as a flat string. Two options:

1. **Join in the query/mapper** — load the FileEntity and map `StorageUrl` into a
   `CoverImageUrl` field on the DTO. Frontend doesn't change.
2. **Return `CoverImageFileId`** — frontend resolves the URL via a file endpoint or
   includes it in a batch. Cleaner separation but requires frontend changes.

Option 1 is recommended for now (no frontend changes).

### Migration

Add an EF migration to drop the `cover_image_url` column from the `articles` table.

### Tests

- `ArticleEntityTests` — remove `coverImageUrl` from `UpdateCoverImage()` calls.
- `AdminUpdateArticleHandlerTests` — remove `CoverImageUrl` from command construction.
- `AdminUploadArticleImageHandlerTests` — update cover test assertions.

---

## Phase C — Remove redundant string fields from `ShortVideoEntity`

**Why:** All four string fields (`VideoUrl`, `VideoStorageKey`, `ThumbnailUrl`,
`ThumbnailStorageKey`) are now redundant — both video and thumbnail are tracked via
`VideoFileId` and `ThumbnailFileId` → `FileEntity`. Unlike `VideoEntity`, ShortVideo
has no YouTube path, so everything goes through FileEntity.

### Files

| File | Change |
|------|--------|
| `ShortVideoEntity.cs` | Remove `VideoUrl`, `VideoStorageKey`, `ThumbnailUrl`, `ThumbnailStorageKey` properties. Remove those params from `CreateStandalone()` and `CreateTeaser()`. Remove `UpdateThumbnail()` — the handler sets `ThumbnailFileId` directly. Remove `ReplaceVideoFile()`. |
| `ShortVideoConfiguration.cs` | Remove the 4 property config lines. |
| `AdminCreateShortVideoHandler.cs` | Stop passing `videoUrl`/`videoStorageKey` to factory. Auto-thumbnail: set `ThumbnailFileId` or store the derived URL on the FileEntity. |
| `AdminUploadShortVideoThumbnailHandler.cs` | Stop calling `UpdateThumbnail()` — just set `ThumbnailFileId`. |
| `AdminDeleteShortVideoHandler.cs` | Use `FileEntity.StorageKey` (loaded via FileId) for Cloudinary deletion instead of `shortVideo.VideoStorageKey` / `ThumbnailStorageKey`. |
| `ShortVideoBuilder.cs` | Remove `_videoUrl`, `_videoStorageKey`, `_thumbnailUrl`, `_thumbnailStorageKey` fields. |
| `ShortVideoFactory.cs` | Update factory methods. |
| All ShortVideo DTOs and mappers | Resolve URLs from FileEntity joins instead of flat strings. |

### Auto-thumbnail concern

`AdminCreateShortVideoHandler.GenerateThumbnailUrl()` derives a thumbnail URL from
the video's Cloudinary URL by URL manipulation. With `VideoUrl` removed, the handler
needs to read `videoFile.StorageUrl` (which it already does) to generate this. The
derived thumbnail URL is not a real uploaded file — options:

1. Store it as a FileEntity with `SizeInBytes = 0` (hack).
2. Keep `ThumbnailUrl` as a nullable string for auto-generated thumbnails only,
   alongside `ThumbnailFileId` for manually uploaded ones.
3. Don't store it at all — generate it on read from the video FileEntity URL.

Option 3 is cleanest. The mapper/DTO layer generates the thumbnail URL from the
video file's `StorageUrl` if `ThumbnailFileId` is null.

### Migration

Add an EF migration to drop `video_url`, `video_storage_key`, `thumbnail_url`,
`thumbnail_storage_key` columns from the `short_videos` table.

### Tests

All `ShortVideoEntityTests`, handler tests, and builder tests need updating for
removed parameters.

---

## Phase D — Clean up `VideoEntity` thumbnail fields (partial)

**Why:** `ThumbnailUrl` and `ThumbnailStorageKey` are partially redundant for
user-uploaded thumbnails (now tracked via `ThumbnailFileId`), but YouTube
auto-thumbnails still need them — the `AdminAttachYoutubeVideoUrlHandler` downloads
the YouTube thumbnail and uploads it via `ICloudinaryService` directly (not through
FileEntity).

### Decision needed

Two options:

1. **Keep `ThumbnailUrl` + `ThumbnailStorageKey`** for YouTube thumbnails. Accept the
   dual-path: user uploads go through FileEntity, YouTube auto-thumbnails stay as
   flat strings. This is the current state — no work needed beyond documenting it.

2. **Route YouTube thumbnails through FileEntity too.** Change
   `AdminAttachYoutubeVideoUrlHandler` to use `fileRepository.UploadAndStoreImageFileAsync`
   instead of `cloudinaryService.UploadImageAsync`. Then remove `ThumbnailUrl` and
   `ThumbnailStorageKey` — resolve everything from `ThumbnailFileId`.

Option 2 is recommended for consistency. If chosen:

### Files

| File | Change |
|------|--------|
| `VideoEntity.cs` | Remove `ThumbnailUrl` and `ThumbnailStorageKey`. Remove those params from `UpdateThumbnail()`. |
| `VideoConfiguration.cs` | Remove the 2 property config lines. |
| `AdminAttachYoutubeVideoUrlHandler.cs` | Replace `ICloudinaryService` with `IFileRepository`. Use `fileRepository.ReplaceImageFileAsync()` instead of `cloudinaryService.UploadImageAsync()`. Set `ThumbnailFileId` from the returned FileEntity. |
| `AdminUploadVideoThumbnailHandler.cs` | Already uses FileEntity — just stop passing `thumbnailStorageKey` to `UpdateThumbnail()`. |
| `AdminDeleteShortVideoHandler.cs` (for video delete if exists) | Use FileEntity StorageKey for Cloudinary cleanup. |
| Video DTOs and mappers | Resolve thumbnail URL from FileEntity join. |

### Migration

Add an EF migration to drop `thumbnail_url` and `thumbnail_storage_key` from the
`videos` table.

---

## Phase E — `ReplaceImageFileAsync` Cloudinary cleanup gap

**Why:** `ReplaceImageFileAsync` soft-deletes the old FileEntity but does not delete
the old Cloudinary asset. It currently works because all callers pass the same
`publicId` (entity ID), so Cloudinary overwrites in place. But the abstraction implies
it handles the full lifecycle — if a future caller passes a different `publicId`,
orphaned Cloudinary assets would accumulate.

### Option 1 — Add explicit Cloudinary delete (recommended)

| File | Change |
|------|--------|
| `FileRepository.cs` | In `ReplaceImageFileAsync`: before soft-deleting, load the old FileEntity, read its `StorageKey`, and call `cloudinaryService.DeleteImageAsync(storageKey)` after the soft-delete. This requires injecting `ICloudinaryService` into `FileRepository` (or using `IFileService`). |
| `IFileService.cs` | Add `DeleteFileAsync(string storageKey)` if we want to keep the CloudinaryService hidden behind FileService. |

### Option 2 — Document the overwrite assumption

Add a code comment on `ReplaceImageFileAsync` stating that callers MUST use the same
`publicId` to rely on Cloudinary overwrite. No code changes.

Option 1 is cleaner long-term.

---

## Phase F — Fix double-nested folder in `AdminCreateShortVideoHandler`

**Why (pre-existing bug):** The handler builds
`publicId = "content/short-videos/{guid}"` and also passes
`folder: "content/short-videos"`. Cloudinary nests these, producing an actual
public ID of `content/short-videos/content/short-videos/{guid}`.

### Files

| File | Change |
|------|--------|
| `AdminCreateShortVideoHandler.cs` | Change `publicId` to just `Guid.NewGuid().ToString()` — the `folder` param already handles the path prefix. |

### Tests

Update any tests that assert on the `publicId` or `storageKey` value.

---

## Execution Order

Phases are independent and can be done in any order, but the recommended sequence is:

1. **Phase A** — trivial constant change, no risk
2. **Phase F** — pre-existing bug fix, small scope
3. **Phase E** — Cloudinary cleanup gap, infrastructure-level
4. **Phase D** — decide on YouTube thumbnail path
5. **Phase C** — remove ShortVideo string fields (largest scope)
6. **Phase B** — remove CoverImageUrl (large scope, touches DTOs/frontend)

Phases B, C, and D each require an EF migration. They can be combined into a single
migration if done together, or kept separate for smaller PRs.
