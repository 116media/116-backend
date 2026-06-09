# Phase 4d — Short Video File Migration

---

## Current Flow

```
IFormFile (video)
  → AdminCreateShortVideoHandler
  → ICloudinaryService.UploadVideoAsync()
  → ShortVideoEntity.Create*(videoUrl, videoStorageKey, ...)
```

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/CreateShortVideo/AdminCreateShortVideoHandler.cs`

The handler currently:
1. Checks for duplicate slug
2. Generates storage key: `content/short-videos/{Guid}`
3. Uploads video via `cloudinaryService.UploadVideoAsync()`
4. Creates `ShortVideoEntity` with `videoUrl` and `videoStorageKey` from the upload result
5. Auto-generates a thumbnail URL from the video URL
6. Commits

---

## New Flow

```
IFormFile (video)
  → AdminCreateShortVideoHandler
  → IFileRepository.UploadAndStoreVideoFileAsync()
  → FileEntity created in core.files
  → ShortVideoEntity.Create*(videoFileId, videoUrl, ...)
```

---

## Handler Changes

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/CreateShortVideo/AdminCreateShortVideoHandler.cs`

### Replace `ICloudinaryService` with `IFileRepository`

```csharp
public class AdminCreateShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IFileRepository fileRepository,             // was: ICloudinaryService cloudinaryService
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminCreateShortVideoCommand, AdminCreateShortVideoResult>
{
    public async Task<AdminCreateShortVideoResult> Handle(
        AdminCreateShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity? existing = await shortVideoRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.ShortVideo.SlugAlreadyExists(slug: command.Slug);
        }

        string storageKey = $"content/short-videos/{Guid.NewGuid()}";

        // Upload via FileRepository → creates FileEntity
        FileEntity videoFile = await fileRepository.UploadAndStoreVideoFileAsync(
            file: command.VideoFile,
            publicId: storageKey,
            folder: "content/short-videos",
            originalFileName: command.VideoFile.FileName,
            mimeType: command.VideoFile.ContentType,
            cancellationToken: cancellationToken
        );

        ShortVideoEntity shortVideo;

        if (command.VideoId.HasValue)
        {
            shortVideo = ShortVideoEntity.CreateTeaser(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                videoFileId: videoFile.Id,
                videoUrl: videoFile.StorageUrl,
                videoId: command.VideoId.Value,
                authorId: command.AuthorId,
                errors: i18n.ShortVideo
            );
        }
        else
        {
            shortVideo = ShortVideoEntity.CreateStandalone(
                id: Guid.NewGuid(),
                title: command.Title,
                slug: command.Slug,
                videoFileId: videoFile.Id,
                videoUrl: videoFile.StorageUrl,
                authorId: command.AuthorId,
                errors: i18n.ShortVideo
            );
        }

        string thumbnailUrl = GenerateThumbnailUrl(videoFile.StorageUrl);
        shortVideo.UpdateThumbnail(thumbnailFileId: null, thumbnailUrl: thumbnailUrl);

        await shortVideoRepository.AddAsync(shortVideo: shortVideo, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        // ... rest unchanged
    }
}
```

---

## ShortVideoEntity Changes

See [06-entity-changes.md](06-entity-changes.md) for full details.

Summary:
- Add `VideoFileId` (Guid) property — required, not nullable
- Update `CreateTeaser()` and `CreateStandalone()` factory methods to accept `videoFileId`
- Keep `VideoUrl` for fast reads
- Remove `VideoStorageKey` — the `FileEntity.StorageKey` tracks this now

---

## Delete Handler Impact

**File:** `AdminDeleteShortVideoHandler.cs`

Must soft-delete the video `FileEntity` — see [08-delete-handler-changes.md](08-delete-handler-changes.md).

---

## Files Changed

| File | Change |
|------|--------|
| `AdminCreateShortVideoHandler.cs` | Replace `ICloudinaryService` with `IFileRepository` |
| `ShortVideoEntity.cs` | Add `VideoFileId`, update factory methods, remove `VideoStorageKey` |
| `ShortVideoConfiguration.cs` | Add FK column, remove `VideoStorageKey` column |
| `AdminDeleteShortVideoHandler.cs` | Soft-delete video FileEntity |
