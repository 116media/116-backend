# Phase 5 — Delete Handler Changes

When a content entity is deleted, its associated `FileEntity` records must be soft-deleted via `IFileRepository.SoftDeleteByIdAsync()`.

---

## 1. `AdminDeleteArticleHandler`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteArticle/AdminDeleteArticleHandler.cs`

### Current behavior

1. Loads article
2. Checks status is Draft or Rejected
3. Loads all `ArticleImageEntity` records
4. Batch-deletes Cloudinary assets via `cloudinaryService.DeleteImagesAsync(storageKeys)`
5. Removes article from DB (cascades to `ArticleImageEntity`)

### New behavior

Add `IFileRepository fileRepository` to the constructor.

After step 2, before cleaning up body images:

```csharp
// Soft-delete cover image FileEntity if exists
if (article.CoverImageFileId.HasValue)
{
    await fileRepository.SoftDeleteByIdAsync(
        fileId: article.CoverImageFileId.Value,
        cancellationToken: cancellationToken
    );
}
```

Body image cleanup (steps 3–5) stays unchanged — body images use `ArticleImageEntity` + direct Cloudinary deletion.

---

## 2. `AdminDeleteShortVideoHandler`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteShortVideo/AdminDeleteShortVideoHandler.cs`

### Current behavior

1. Loads short video
2. Deletes thumbnail from Cloudinary if `ThumbnailStorageKey` exists
3. Deletes video from Cloudinary via `cloudinaryService.DeleteImageAsync(VideoStorageKey)`
4. Removes short video from DB

### New behavior

Replace `ICloudinaryService` with `IFileRepository` in the constructor.

```csharp
public class AdminDeleteShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,              // was: ICloudinaryService cloudinaryService
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminDeleteShortVideoCommand, AdminDeleteShortVideoResult>
{
    public async Task<AdminDeleteShortVideoResult> Handle(
        AdminDeleteShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        // Soft-delete thumbnail FileEntity if exists (manual upload only)
        if (shortVideo.ThumbnailFileId.HasValue)
        {
            await fileRepository.SoftDeleteByIdAsync(
                fileId: shortVideo.ThumbnailFileId.Value,
                cancellationToken: cancellationToken
            );
        }

        // Soft-delete video FileEntity
        await fileRepository.SoftDeleteByIdAsync(
            fileId: shortVideo.VideoFileId,
            cancellationToken: cancellationToken
        );

        shortVideoRepository.Remove(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteShortVideoResult(IsSuccess: true);
    }
}
```

**Note:** The Cloudinary assets are NOT deleted here — only the `FileEntity` is soft-deleted. A future cleanup job can hard-delete Cloudinary assets based on soft-deleted `FileEntity` records. This is consistent with how avatar deletion works (soft-delete the `FileEntity`, Cloudinary asset remains until manually purged).

**Alternative:** If you want to also delete from Cloudinary on entity deletion, keep `ICloudinaryService` as a dependency alongside `IFileRepository` and call `cloudinaryService.DeleteImageAsync(fileEntity.StorageKey)` after soft-deleting the `FileEntity`.

---

## 3. `AbandonedDraftCleanupJob`

**File:** `src/Modules/Content/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJob.cs`

### Current behavior

1. Finds abandoned draft articles (Draft, empty body/headline, 7+ days old)
2. Loads their `ArticleImageEntity` records
3. Batch-deletes Cloudinary assets
4. Removes articles from DB

### New behavior

Add `IFileRepository fileRepository` to the constructor.

For each abandoned article, before cleaning up body images:

```csharp
if (article.CoverImageFileId.HasValue)
{
    await fileRepository.SoftDeleteByIdAsync(
        fileId: article.CoverImageFileId.Value,
        cancellationToken: cancellationToken
    );
}
```

---

## 4. Video Delete (Future)

There is currently no `AdminDeleteVideoHandler`. When one is implemented, it must:
1. Soft-delete `ThumbnailFileId` via `fileRepository.SoftDeleteByIdAsync()` (if not null)
2. Remove the video entity from DB

---

## Summary

| Handler | FileEntity to soft-delete | Field |
|---------|--------------------------|-------|
| `AdminDeleteArticleHandler` | Cover image | `article.CoverImageFileId` |
| `AdminDeleteShortVideoHandler` | Video file + thumbnail | `shortVideo.VideoFileId`, `shortVideo.ThumbnailFileId` |
| `AbandonedDraftCleanupJob` | Cover image | `article.CoverImageFileId` |
| Future `AdminDeleteVideoHandler` | Thumbnail | `video.ThumbnailFileId` |

---

## Files Changed

| File | Change |
|------|--------|
| `AdminDeleteArticleHandler.cs` | Add `IFileRepository`, soft-delete cover `FileEntity` |
| `AdminDeleteShortVideoHandler.cs` | Replace `ICloudinaryService` with `IFileRepository`, soft-delete both files |
| `AbandonedDraftCleanupJob.cs` | Add `IFileRepository`, soft-delete cover `FileEntity` |
