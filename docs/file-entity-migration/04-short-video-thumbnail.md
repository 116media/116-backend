# Phase 4c — Short Video Thumbnail Migration

---

## Current Flow

```
IFormFile
  → AdminUploadShortVideoThumbnailHandler
  → ICloudinaryService.UploadImageAsync()
  → ShortVideoEntity.UpdateThumbnail(thumbnailUrl, thumbnailStorageKey)
```

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailHandler.cs`

The handler currently:
1. Parses `shortVideoId` from the command
2. Loads the short video via `shortVideoRepository.GetByIdOrThrowAsync()`
3. Uploads to Cloudinary with `publicId = shortVideoId`, `folder = "content/short-video-thumbnails"`
4. Updates `ShortVideoEntity.ThumbnailUrl` and `ThumbnailStorageKey` directly
5. Commits

---

## New Flow

```
IFormFile
  → AdminUploadShortVideoThumbnailHandler
  → IFileRepository.ReplaceImageFileAsync()
  → FileEntity created in core.files
  → ShortVideoEntity.UpdateThumbnail(fileId, url)
```

---

## Handler Changes

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailHandler.cs`

### Replace `ICloudinaryService` with `IFileRepository`

```csharp
public class AdminUploadShortVideoThumbnailHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,              // was: ICloudinaryService cloudinaryService
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadShortVideoThumbnailCommand, AdminUploadShortVideoThumbnailResult>
{
    public async Task<AdminUploadShortVideoThumbnailResult> Handle(
        AdminUploadShortVideoThumbnailCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid shortVideoId = Guid.Parse(command.ShortVideoId);

        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: shortVideoId,
            cancellationToken: cancellationToken
        );

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: shortVideo.ThumbnailFileId,
            file: command.File,
            publicId: shortVideoId.ToString(),
            folder: "content/short-video-thumbnails",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        shortVideo.UpdateThumbnail(
            thumbnailFileId: fileEntity.Id,
            thumbnailUrl: fileEntity.StorageUrl
        );

        shortVideoRepository.Update(shortVideo: shortVideo);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadShortVideoThumbnailResult(
            ThumbnailUrl: fileEntity.StorageUrl,
            ThumbnailStorageKey: fileEntity.StorageKey!
        );
    }
}
```

---

## Auto-Generated Thumbnail in `AdminCreateShortVideoHandler`

**File:** `AdminCreateShortVideoHandler.cs`

When a short video is created, the handler auto-generates a thumbnail URL from the video URL using Cloudinary transformations (`so_1` screenshot at 1 second). This is NOT a user upload — no `IFormFile` is involved.

This stays unchanged:
```csharp
string thumbnailUrl = GenerateThumbnailUrl(uploadResult.SecureUrl);
shortVideo.UpdateThumbnail(thumbnailFileId: null, thumbnailUrl: thumbnailUrl);
```

The `ThumbnailFileId` is `null` for auto-generated thumbnails. Only manual uploads set it.

---

## ShortVideoEntity Changes

See [06-entity-changes.md](06-entity-changes.md) for full details.

Summary:
- Add `ThumbnailFileId` (Guid?) property
- Update `UpdateThumbnail()` to accept `(Guid? thumbnailFileId, string thumbnailUrl)`
- Keep `ThumbnailUrl` for fast reads
- Remove `ThumbnailStorageKey` — for manual uploads the `FileEntity` tracks it; for auto-generated thumbnails no storage key is needed (they use the video's storage key with a transformation)

---

## Files Changed

| File | Change |
|------|--------|
| `AdminUploadShortVideoThumbnailHandler.cs` | Replace `ICloudinaryService` with `IFileRepository` |
| `AdminCreateShortVideoHandler.cs` | Pass `null` for `thumbnailFileId` on auto-generated thumbnails |
| `ShortVideoEntity.cs` | Add `ThumbnailFileId`, update `UpdateThumbnail()`, remove `ThumbnailStorageKey` |
| `ShortVideoConfiguration.cs` | Add FK column, remove `ThumbnailStorageKey` column |
