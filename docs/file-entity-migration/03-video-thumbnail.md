# Phase 4b — Video Thumbnail Migration

---

## Current Flow

```
IFormFile
  → AdminUploadVideoThumbnailHandler
  → ICloudinaryService.UploadImageAsync()
  → VideoEntity.UpdateThumbnail(thumbnailUrl, thumbnailStorageKey)
```

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadVideoThumbnail/AdminUploadVideoThumbnailHandler.cs`

The handler currently:
1. Parses `videoId` from the command
2. Loads the video via `videoRepository.GetByIdOrThrowAsync()`
3. Uploads to Cloudinary with `publicId = videoId`, `folder = "content/video-thumbnails"`
4. Updates `VideoEntity.ThumbnailUrl` and `ThumbnailStorageKey` directly
5. Commits

---

## New Flow

```
IFormFile
  → AdminUploadVideoThumbnailHandler
  → IFileRepository.ReplaceImageFileAsync()
  → FileEntity created in core.files
  → VideoEntity.UpdateThumbnail(fileId, url)
```

---

## Handler Changes

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadVideoThumbnail/AdminUploadVideoThumbnailHandler.cs`

### Replace `ICloudinaryService` with `IFileRepository`

```csharp
public class AdminUploadVideoThumbnailHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    IFileRepository fileRepository           // was: ICloudinaryService cloudinaryService
) : ICommandHandler<AdminUploadVideoThumbnailCommand, AdminUploadVideoThumbnailResult>
{
    public async Task<AdminUploadVideoThumbnailResult> Handle(
        AdminUploadVideoThumbnailCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: videoId,
            cancellationToken: cancellationToken
        );

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: video.ThumbnailFileId,
            file: command.File,
            publicId: videoId.ToString(),
            folder: "content/video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        video.UpdateThumbnail(
            thumbnailFileId: fileEntity.Id,
            thumbnailUrl: fileEntity.StorageUrl
        );

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadVideoThumbnailResult(
            ThumbnailUrl: fileEntity.StorageUrl,
            ThumbnailStorageKey: fileEntity.StorageKey!
        );
    }
}
```

---

## YouTube Auto-Thumbnail — No Change

**File:** `AdminAttachYoutubeVideoUrlHandler.cs`

This handler downloads a thumbnail from YouTube and re-uploads to Cloudinary. There is no user-uploaded `IFormFile`. It continues to:
1. Call `cloudinaryService.UploadImageAsync()` directly
2. Store the URL and storage key on `VideoEntity.ThumbnailUrl` / `ThumbnailStorageKey`

The `ThumbnailFileId` stays `null` for auto-downloaded thumbnails. Only manual uploads via `AdminUploadVideoThumbnailHandler` create a `FileEntity`.

If a user later manually uploads a thumbnail (via `AdminUploadVideoThumbnailHandler`), the handler replaces the auto-thumbnail URL and sets `ThumbnailFileId`.

---

## VideoEntity Changes

See [06-entity-changes.md](06-entity-changes.md) for full details.

Summary:
- Add `ThumbnailFileId` (Guid?) property
- Update `UpdateThumbnail()` signature to accept `(Guid? thumbnailFileId, string thumbnailUrl)`
- Keep `ThumbnailUrl` for fast reads
- Keep `ThumbnailStorageKey` for YouTube auto-thumbnails (no FileEntity)
- `ThumbnailStorageKey` becomes redundant for manual uploads (FileEntity has it), but remains for auto-thumbnails

---

## Files Changed

| File | Change |
|------|--------|
| `AdminUploadVideoThumbnailHandler.cs` | Replace `ICloudinaryService` with `IFileRepository` |
| `VideoEntity.cs` | Add `ThumbnailFileId`, update `UpdateThumbnail()` |
| `VideoConfiguration.cs` | Add FK column |
