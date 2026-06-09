# Implementation Specs — File Entity Migration

Every file, property, method, line, and test to add, update, or remove.

---

## Phase 1 — Infrastructure (Core Module)

### 1.1 `FileConstants.cs`

**File:** `src/BuildingBlocks/Constants/FileConstants.cs`

**Add:**

```csharp
/// <summary>
/// Maximum allowed length for cloud storage keys (e.g., Cloudinary public IDs).
/// </summary>
public const int MaxStorageKeyLength = 500;
```

---

### 1.2 `FileEntity.cs`

**File:** `src/Modules/Core/Core/Domain/Entities/FileEntity.cs`

**Add property** (after `SizeInBytes`):

```csharp
/// <summary>
/// Provider-agnostic storage key used to identify and delete the file
/// from cloud storage. For Cloudinary this is the public ID.
/// </summary>
[MaxLength(FileConstants.MaxStorageKeyLength)]
public string? StorageKey { get; private set; }
```

**Update `Create()` factory** — add `storageKey` parameter:

```csharp
public static FileEntity Create(
    Guid id,
    string fileName,
    string originalFileName,
    string mimeType,
    string storageUrl,
    long sizeInBytes,
    CoreI18n i18n,
    string? storageKey = null
)
{
    // ... existing validation unchanged ...

    return new FileEntity
    {
        Id = id,
        FileName = fileName,
        OriginalFileName = originalFileName,
        MimeType = mimeType,
        StorageUrl = storageUrl,
        SizeInBytes = sizeInBytes,
        StorageKey = storageKey,
    };
}
```

---

### 1.3 `FileUploadResult` record

**File:** `src/Modules/Core/Core/Application/Shared/Services/IFileService.cs`

The existing `FileUploadResult` is missing `PublicId`. **Add it:**

```csharp
public record FileUploadResult(
    Guid FileId,
    string SecureUrl,
    string Format,
    int Width,
    int Height,
    long Bytes,
    string PublicId    // <-- new field
);
```

---

### 1.4 `FileService.cs`

**File:** `src/Modules/Core/Core/Infrastructure/Services/FileService.cs`

**Update `UploadFileAsync()`** — pass `PublicId` through:

```csharp
return new FileUploadResult(
    FileId: fileId,
    SecureUrl: uploadResult.SecureUrl,
    Format: uploadResult.Format,
    Width: uploadResult.Width,
    Height: uploadResult.Height,
    Bytes: uploadResult.Bytes,
    PublicId: uploadResult.PublicId    // <-- add
);
```

**Update `UploadRawFileAsync()`** — same change.

**Add `UploadVideoFileAsync()`:**

```csharp
/// <inheritdoc />
public async Task<FileUploadResult> UploadVideoFileAsync(
    IFormFile file,
    string publicId,
    string? folder = null,
    CancellationToken cancellationToken = default
)
{
    CloudinaryUploadResult uploadResult = await cloudinaryService.UploadVideoAsync(
        file,
        publicId,
        folder,
        cancellationToken
    );

    var fileId = Guid.NewGuid();

    return new FileUploadResult(
        FileId: fileId,
        SecureUrl: uploadResult.SecureUrl,
        Format: uploadResult.Format,
        Width: uploadResult.Width,
        Height: uploadResult.Height,
        Bytes: uploadResult.Bytes,
        PublicId: uploadResult.PublicId
    );
}
```

---

### 1.5 `IFileService.cs`

**File:** `src/Modules/Core/Core/Application/Shared/Services/IFileService.cs`

**Add method:**

```csharp
/// <summary>
/// Uploads a video file to cloud storage.
/// </summary>
Task<FileUploadResult> UploadVideoFileAsync(
    IFormFile file,
    string publicId,
    string? folder = null,
    CancellationToken cancellationToken = default
);
```

---

### 1.6 `IFileRepository.cs`

**File:** `src/Modules/Core/Core/Application/Shared/Repositories/IFileRepository.cs`

**Add 4 methods:**

```csharp
/// <summary>
/// Uploads an image file to cloud storage and persists file metadata to the database.
/// </summary>
Task<FileEntity> UploadAndStoreImageFileAsync(
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
);

/// <summary>
/// Uploads a video file to cloud storage and persists file metadata to the database.
/// </summary>
Task<FileEntity> UploadAndStoreVideoFileAsync(
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
);

/// <summary>
/// Replaces a tracked file: soft-deletes the old FileEntity, uploads a new image.
/// </summary>
Task<FileEntity> ReplaceImageFileAsync(
    Guid? currentFileId,
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
);

/// <summary>
/// Soft-deletes a file entity by its ID.
/// </summary>
Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default);
```

---

### 1.7 `FileRepository.cs`

**File:** `src/Modules/Core/Core/Infrastructure/Repositories/FileRepository.cs`

**Implement 4 new methods:**

```csharp
/// <inheritdoc />
public async Task<FileEntity> UploadAndStoreImageFileAsync(
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
)
{
    FileUploadResult uploadResult = await fileService.UploadFileAsync(
        file: file,
        publicId: publicId,
        folder: folder,
        cancellationToken: cancellationToken
    );

    var fileEntity = FileEntity.Create(
        id: uploadResult.FileId,
        fileName: publicId,
        originalFileName: originalFileName,
        mimeType: mimeType,
        storageUrl: uploadResult.SecureUrl,
        sizeInBytes: uploadResult.Bytes,
        i18n: i18n,
        storageKey: uploadResult.PublicId
    );

    await AddAsync(fileEntity, cancellationToken);
    await SaveChangesAsync(cancellationToken);

    return fileEntity;
}

/// <inheritdoc />
public async Task<FileEntity> UploadAndStoreVideoFileAsync(
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
)
{
    FileUploadResult uploadResult = await fileService.UploadVideoFileAsync(
        file: file,
        publicId: publicId,
        folder: folder,
        cancellationToken: cancellationToken
    );

    var fileEntity = FileEntity.Create(
        id: uploadResult.FileId,
        fileName: publicId,
        originalFileName: originalFileName,
        mimeType: mimeType,
        storageUrl: uploadResult.SecureUrl,
        sizeInBytes: uploadResult.Bytes,
        i18n: i18n,
        storageKey: uploadResult.PublicId
    );

    await AddAsync(fileEntity, cancellationToken);
    await SaveChangesAsync(cancellationToken);

    return fileEntity;
}

/// <inheritdoc />
public async Task<FileEntity> ReplaceImageFileAsync(
    Guid? currentFileId,
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
)
{
    if (currentFileId.HasValue)
    {
        await SoftDeleteByIdAsync(currentFileId.Value, cancellationToken);
    }

    return await UploadAndStoreImageFileAsync(
        file, publicId, folder, originalFileName, mimeType, cancellationToken
    );
}

/// <inheritdoc />
public async Task<bool> SoftDeleteByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
{
    FileEntity? file = await GetByIdAsync(fileId, cancellationToken);
    if (file is null)
    {
        return false;
    }

    bool deleted = file.Delete();
    if (deleted)
    {
        await UpdateAsync(file, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    return deleted;
}
```

**Update existing `UploadAndStoreAvatarAsync()`** — pass `storageKey`:

At line ~80, change `FileEntity.Create()` call to include `storageKey: uploadResult.PublicId`.

**Update existing `UploadAndStoreRawFileAsync()`** — same change at line ~237.

---

### 1.8 `FileConfiguration.cs`

**File:** `src/Modules/Core/Core/Infrastructure/Persistence/Configurations/FileConfiguration.cs`

**Add** (after `StorageUrl` config):

```csharp
builder.Property(f => f.StorageKey)
    .HasMaxLength(FileConstants.MaxStorageKeyLength)
    .IsRequired(false);
```

---

## Phase 2 — Domain Entity Changes (Content Module)

### 2.1 `ArticleEntity.cs`

**File:** `src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs`

**Add property** (after `CoverImageUrl` at line 80):

```csharp
/// <summary>
/// ID of the uploaded cover image file tracked in the Core module.
/// </summary>
public Guid? CoverImageFileId { get; private set; }
```

**Update `UpdateCoverImage()`** at line 357:

Before:
```csharp
public void UpdateCoverImage(string? coverImageUrl)
{
    CoverImageUrl = coverImageUrl;
}
```

After:
```csharp
public void UpdateCoverImage(Guid? coverImageFileId, string? coverImageUrl)
{
    CoverImageFileId = coverImageFileId;
    CoverImageUrl = coverImageUrl;
}
```

**Find and update all callers of `UpdateCoverImage()`:**

- `AdminUploadArticleImageHandler.cs` line 72: `article.UpdateCoverImage(coverImageUrl: uploadResult.SecureUrl)` → changed in Phase 4
- `ArticleEntity.cs` constructor (if cover is set in `CreateFree`/`CreatePaid`) — check if it exists. Based on current code: `CoverImageUrl` is set to `null` by default, so no factory change needed.

---

### 2.2 `VideoEntity.cs`

**File:** `src/Modules/Content/Content/Domain/Entities/VideoEntity.cs`

**Add property** (after `ThumbnailStorageKey` at line 81):

```csharp
/// <summary>
/// ID of the uploaded thumbnail file tracked in the Core module.
/// Null for auto-downloaded YouTube thumbnails.
/// </summary>
public Guid? ThumbnailFileId { get; private set; }
```

**Update `UpdateThumbnail()`** at line 346:

Before:
```csharp
public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
}
```

After:
```csharp
public void UpdateThumbnail(string thumbnailUrl, string? thumbnailStorageKey = null, Guid? thumbnailFileId = null)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
    ThumbnailFileId = thumbnailFileId;
}
```

**Why keep `ThumbnailStorageKey`?** YouTube auto-thumbnails have no `FileEntity` but still need a storage key for Cloudinary deletion. `ThumbnailStorageKey` stays for those. Manual uploads set both `ThumbnailFileId` and `ThumbnailStorageKey`.

**Find and update all callers:**

- `AdminUploadVideoThumbnailHandler.cs` line 43: `video.UpdateThumbnail(thumbnailUrl: result.SecureUrl, thumbnailStorageKey: result.PublicId)` → changed in Phase 4
- `AdminAttachYoutubeVideoUrlHandler.cs`: calls `UpdateThumbnail(url, storageKey)` → stays, just add `thumbnailFileId: null`

---

### 2.3 `ShortVideoEntity.cs`

**File:** `src/Modules/Content/Content/Domain/Entities/ShortVideoEntity.cs`

**Add property** (after `VideoStorageKey` at line 45):

```csharp
/// <summary>
/// ID of the uploaded video file tracked in the Core module.
/// </summary>
public Guid VideoFileId { get; private set; }
```

**Add property** (after `ThumbnailStorageKey` at line 58):

```csharp
/// <summary>
/// ID of the uploaded thumbnail file tracked in the Core module.
/// Null for auto-generated thumbnails.
/// </summary>
public Guid? ThumbnailFileId { get; private set; }
```

**Update `CreateStandalone()`** at line 126 — add `videoFileId` parameter:

```csharp
public static ShortVideoEntity CreateStandalone(
    Guid id,
    string title,
    string slug,
    string videoUrl,
    string videoStorageKey,
    Guid videoFileId,           // <-- new
    Guid authorId,
    ShortVideoErrors errors
)
{
    // ... validation unchanged ...

    return new ShortVideoEntity
    {
        Id = id,
        Title = title,
        Slug = slug,
        VideoUrl = videoUrl,
        VideoStorageKey = videoStorageKey,
        VideoFileId = videoFileId,     // <-- new
        AuthorId = authorId,
        HasFullVideo = false,
        IsActive = true,
    };
}
```

**Update `CreateTeaser()`** at line 168 — same, add `videoFileId`:

```csharp
public static ShortVideoEntity CreateTeaser(
    Guid id,
    string title,
    string slug,
    string videoUrl,
    string videoStorageKey,
    Guid videoFileId,           // <-- new
    Guid videoId,
    Guid authorId,
    ShortVideoErrors errors
)
{
    // ... validation unchanged ...

    return new ShortVideoEntity
    {
        Id = id,
        Title = title,
        Slug = slug,
        VideoUrl = videoUrl,
        VideoStorageKey = videoStorageKey,
        VideoFileId = videoFileId,     // <-- new
        VideoId = videoId,
        AuthorId = authorId,
        HasFullVideo = true,
        IsActive = true,
    };
}
```

**Update `UpdateThumbnail()`** at line 235:

Before:
```csharp
public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
}
```

After:
```csharp
public void UpdateThumbnail(string thumbnailUrl, string? thumbnailStorageKey = null, Guid? thumbnailFileId = null)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
    ThumbnailFileId = thumbnailFileId;
}
```

**Keep `VideoStorageKey` and `ThumbnailStorageKey`** — these are still needed for Cloudinary deletion. The `FileEntity.StorageKey` is the centralized record, but the entity-level storage key is used in delete handlers that call `cloudinaryService.DeleteImageAsync()` directly. Removing them is a separate cleanup task after all handlers route through `IFileRepository` for deletion.

---

## Phase 3 — EF Configuration & Migration

### 3.1 `ArticleConfiguration.cs`

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleConfiguration.cs`

**Add** (after `CoverImageUrl` config at line 32):

```csharp
builder.Property(x => x.CoverImageFileId).IsRequired(false);
```

---

### 3.2 `VideoConfiguration.cs`

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/VideoConfiguration.cs`

**Add** (after `ThumbnailStorageKey` config at line 30):

```csharp
builder.Property(x => x.ThumbnailFileId).IsRequired(false);
```

---

### 3.3 `ShortVideoConfiguration.cs`

**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ShortVideoConfiguration.cs`

**Add** (after `VideoStorageKey` config at line 27):

```csharp
builder.Property(x => x.VideoFileId).IsRequired(false);
```

**Add** (after `ThumbnailStorageKey` config at line 31):

```csharp
builder.Property(x => x.ThumbnailFileId).IsRequired(false);
```

**Note:** `VideoFileId` is `Guid` (not nullable) on the entity, but configured as `IsRequired(false)` in EF to allow the data migration. After backfilling existing rows, a follow-up migration can change it to `IsRequired(true)`.

---

### 3.4 EF Migrations

**Core migration:**

```bash
dotnet ef migrations add AddStorageKeyToFileEntity \
  --project src/Modules/Core/Core/Infrastructure \
  --startup-project src/Api \
  --context CoreDbContext
```

**Content migration:**

```bash
dotnet ef migrations add AddFileIdColumnsToContentEntities \
  --project src/Modules/Content/Content/Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

---

## Phase 4 — Handler Changes

### 4.1 `AdminUploadArticleImageHandler.cs` (Cover Path Only)

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/AdminUploadArticleImageHandler.cs`

**Add dependency:** `IFileRepository fileRepository`

**Change cover branch** (lines 55–73):

```csharp
if (isCover)
{
    IReadOnlyList<ArticleImageEntity> existingImages = await articleRepository.GetImagesByArticleIdAsync(
        articleId: articleId,
        cancellationToken: cancellationToken
    );

    ArticleImageEntity? oldCover = existingImages.FirstOrDefault(img =>
        img.ImageType == EnumArticleImageType.Cover
    );

    if (oldCover is not null)
    {
        articleRepository.RemoveImages(images: [oldCover]);
    }

    FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
        currentFileId: article.CoverImageFileId,
        file: command.File,
        publicId: articleId.ToString(),
        folder: "content/article-images",
        originalFileName: command.File.FileName,
        mimeType: command.File.ContentType,
        cancellationToken: cancellationToken
    );

    article.UpdateCoverImage(
        coverImageFileId: fileEntity.Id,
        coverImageUrl: fileEntity.StorageUrl
    );
    articleRepository.Update(article: article);
    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

    var dto = mapper.Map<ArticleImageDto>(new ArticleImageEntity());
    // Or return a simplified DTO — depends on what the frontend expects.
    // The existing ArticleImageEntity is no longer created for covers.
    return new AdminUploadArticleImageResult(Image: new ArticleImageDto(
        Id: fileEntity.Id,
        StorageKey: fileEntity.StorageKey ?? string.Empty,
        Url: fileEntity.StorageUrl,
        ImageType: EnumArticleImageType.Cover
    ));
}
```

**Body branch** (lines 75–88) — unchanged. Still uses `cloudinaryService.UploadImageAsync()` and creates `ArticleImageEntity`.

---

### 4.2 `AdminUploadVideoThumbnailHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadVideoThumbnail/AdminUploadVideoThumbnailHandler.cs`

**Replace dependency:** `ICloudinaryService cloudinaryService` → `IFileRepository fileRepository`

**Full new handler body:**

```csharp
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
        thumbnailUrl: fileEntity.StorageUrl,
        thumbnailStorageKey: fileEntity.StorageKey,
        thumbnailFileId: fileEntity.Id
    );

    videoRepository.Update(video: video);
    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

    return new AdminUploadVideoThumbnailResult(
        ThumbnailUrl: fileEntity.StorageUrl,
        ThumbnailStorageKey: fileEntity.StorageKey!
    );
}
```

---

### 4.3 `AdminUploadShortVideoThumbnailHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailHandler.cs`

**Replace dependency:** `ICloudinaryService cloudinaryService` → `IFileRepository fileRepository`

**Full new handler body:**

```csharp
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
        thumbnailUrl: fileEntity.StorageUrl,
        thumbnailStorageKey: fileEntity.StorageKey,
        thumbnailFileId: fileEntity.Id
    );

    shortVideoRepository.Update(shortVideo: shortVideo);
    await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

    return new AdminUploadShortVideoThumbnailResult(
        ThumbnailUrl: fileEntity.StorageUrl,
        ThumbnailStorageKey: fileEntity.StorageKey!
    );
}
```

---

### 4.4 `AdminCreateShortVideoHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/CreateShortVideo/AdminCreateShortVideoHandler.cs`

**Replace dependency:** `ICloudinaryService cloudinaryService` → `IFileRepository fileRepository`

**Change upload call** (lines 44–49):

Before:
```csharp
string storageKey = $"content/short-videos/{Guid.NewGuid()}";

CloudinaryUploadResult uploadResult = await cloudinaryService.UploadVideoAsync(
    file: command.VideoFile,
    publicId: storageKey,
    cancellationToken: cancellationToken
);
```

After:
```csharp
string storageKey = $"content/short-videos/{Guid.NewGuid()}";

FileEntity videoFile = await fileRepository.UploadAndStoreVideoFileAsync(
    file: command.VideoFile,
    publicId: storageKey,
    folder: "content/short-videos",
    originalFileName: command.VideoFile.FileName,
    mimeType: command.VideoFile.ContentType,
    cancellationToken: cancellationToken
);
```

**Update entity creation** (lines 54–78):

Before:
```csharp
shortVideo = ShortVideoEntity.CreateStandalone(
    id: Guid.NewGuid(),
    title: command.Title,
    slug: command.Slug,
    videoUrl: uploadResult.SecureUrl,
    videoStorageKey: uploadResult.PublicId,
    authorId: command.AuthorId,
    errors: i18n.ShortVideo
);
```

After:
```csharp
shortVideo = ShortVideoEntity.CreateStandalone(
    id: Guid.NewGuid(),
    title: command.Title,
    slug: command.Slug,
    videoUrl: videoFile.StorageUrl,
    videoStorageKey: videoFile.StorageKey!,
    videoFileId: videoFile.Id,
    authorId: command.AuthorId,
    errors: i18n.ShortVideo
);
```

Same for the `CreateTeaser()` branch.

**Update auto-thumbnail** (line 80):

Before:
```csharp
string thumbnailUrl = GenerateThumbnailUrl(uploadResult.SecureUrl);
shortVideo.UpdateThumbnail(thumbnailUrl, uploadResult.PublicId);
```

After:
```csharp
string thumbnailUrl = GenerateThumbnailUrl(videoFile.StorageUrl);
shortVideo.UpdateThumbnail(thumbnailUrl: thumbnailUrl, thumbnailStorageKey: videoFile.StorageKey, thumbnailFileId: null);
```

---

### 4.5 `AdminAttachYoutubeVideoUrlHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/AttachYoutubeVideoUrl/AdminAttachYoutubeVideoUrlHandler.cs`

**Update `UpdateThumbnail()` call** to include `thumbnailFileId: null`:

Before:
```csharp
video.UpdateThumbnail(thumbnailUrl: result.SecureUrl, thumbnailStorageKey: result.PublicId);
```

After:
```csharp
video.UpdateThumbnail(thumbnailUrl: result.SecureUrl, thumbnailStorageKey: result.PublicId, thumbnailFileId: null);
```

---

## Phase 5 — Delete Handler Changes

### 5.1 `AdminDeleteArticleHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteArticle/AdminDeleteArticleHandler.cs`

**Add dependency:** `IFileRepository fileRepository`

**Add** after status check (after line 39), before image cleanup:

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

### 5.2 `AdminDeleteShortVideoHandler.cs`

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteShortVideo/AdminDeleteShortVideoHandler.cs`

**Add dependency:** `IFileRepository fileRepository` (keep `ICloudinaryService` for now — still needed for Cloudinary deletion)

**Add** after loading the entity (after line 30):

```csharp
if (shortVideo.ThumbnailFileId.HasValue)
{
    await fileRepository.SoftDeleteByIdAsync(
        fileId: shortVideo.ThumbnailFileId.Value,
        cancellationToken: cancellationToken
    );
}

await fileRepository.SoftDeleteByIdAsync(
    fileId: shortVideo.VideoFileId,
    cancellationToken: cancellationToken
);
```

Keep the existing `cloudinaryService.DeleteImageAsync()` calls for now — the `FileEntity` soft-delete tracks the record, but Cloudinary assets still need explicit deletion.

---

### 5.3 `AbandonedDraftCleanupJob.cs`

**File:** `src/Modules/Content/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJob.cs`

**Add** to the service scope resolution (after line 56):

```csharp
var fileRepository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
```

**Add** inside the per-draft loop (after line 76, before `storageKeys`):

```csharp
if (draft.CoverImageFileId.HasValue)
{
    await fileRepository.SoftDeleteByIdAsync(
        fileId: draft.CoverImageFileId.Value,
        cancellationToken: context.CancellationToken
    );
}
```

---

## Phase 6 — Test Infrastructure Updates

### 6.1 `TestConstants.cs`

**File:** `tests/Fixtures/Constants/TestConstants.cs`

**Add** to `TestConstants.File`:

```csharp
public const string ValidStorageKey = "content/test-images/test-image-id";
public const string ValidVideoStorageKey = "content/short-videos/test-video-id";
```

---

### 6.2 `FileBuilder.cs`

**File:** `tests/Fixtures/Builders/Entities/FileBuilder.cs`

**Add field and method:**

```csharp
private string? _storageKey;

public FileBuilder WithStorageKey(string storageKey)
{
    _storageKey = storageKey;
    return this;
}
```

**Update `Build()`:** pass `storageKey: _storageKey` to `FileEntity.Create()`.

---

### 6.3 `FileFactory.cs`

**File:** `tests/Fixtures/Factories/FileFactory.cs`

**Add methods:**

```csharp
public static FileEntity CreateWithStorageKey(string storageKey)
    => new FileBuilder().WithStorageKey(storageKey).Build();

public static FileEntity CreateImage()
    => new FileBuilder().AsJpegImage().WithStorageKey(TestConstants.File.ValidStorageKey).Build();

public static FileEntity CreateVideo()
    => new FileBuilder()
        .WithMimeType("video/mp4")
        .WithStorageKey(TestConstants.File.ValidVideoStorageKey)
        .Build();
```

---

### 6.4 `ShortVideoBuilder.cs`

**File:** `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs`

**Add fields and methods:**

```csharp
private Guid _videoFileId = Guid.NewGuid();
private Guid? _thumbnailFileId;

public ShortVideoBuilder WithVideoFileId(Guid fileId)
{
    _videoFileId = fileId;
    return this;
}

public ShortVideoBuilder WithThumbnailFileId(Guid fileId)
{
    _thumbnailFileId = fileId;
    return this;
}
```

**Update `Build()`:** pass `videoFileId: _videoFileId` to `CreateStandalone()` / `CreateTeaser()`.

---

### 6.5 `ShortVideoFactory.cs`

**File:** `tests/Fixtures/Factories/Content/ShortVideoFactory.cs`

**Update all existing methods** to pass `videoFileId` through the builder.

**Add:**

```csharp
public static ShortVideoEntity CreateWithVideoFileId(Guid videoFileId)
    => new ShortVideoBuilder().WithVideoFileId(videoFileId).Build();

public static ShortVideoEntity CreateWithThumbnailFileId(Guid thumbnailFileId)
    => new ShortVideoBuilder().WithThumbnailFileId(thumbnailFileId).Build();
```

---

### 6.6 `MockFileRepository.cs`

**File:** `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs`

**Add setup methods:**

```csharp
public static Mock<IFileRepository> SetupUploadAndStoreImageFile(
    this Mock<IFileRepository> mock, FileEntity file)
{
    mock.Setup(x => x.UploadAndStoreImageFileAsync(
            It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(file);
    return mock;
}

public static Mock<IFileRepository> SetupUploadAndStoreVideoFile(
    this Mock<IFileRepository> mock, FileEntity file)
{
    mock.Setup(x => x.UploadAndStoreVideoFileAsync(
            It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(file);
    return mock;
}

public static Mock<IFileRepository> SetupReplaceImageFile(
    this Mock<IFileRepository> mock, FileEntity file)
{
    mock.Setup(x => x.ReplaceImageFileAsync(
            It.IsAny<Guid?>(), It.IsAny<IFormFile>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(file);
    return mock;
}

public static Mock<IFileRepository> SetupSoftDeleteById(
    this Mock<IFileRepository> mock, bool result = true)
{
    mock.Setup(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(result);
    return mock;
}
```

**Add verify methods:**

```csharp
public static void VerifyUploadAndStoreImageFileCalled(this Mock<IFileRepository> mock)
    => mock.Verify(x => x.UploadAndStoreImageFileAsync(
        It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

public static void VerifyUploadAndStoreVideoFileCalled(this Mock<IFileRepository> mock)
    => mock.Verify(x => x.UploadAndStoreVideoFileAsync(
        It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);

public static void VerifyReplaceImageFileCalled(this Mock<IFileRepository> mock)
    => mock.Verify(x => x.ReplaceImageFileAsync(
        It.IsAny<Guid?>(), It.IsAny<IFormFile>(), It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
        It.IsAny<CancellationToken>()), Times.Once);

public static void VerifySoftDeleteByIdCalled(this Mock<IFileRepository> mock, Guid fileId)
    => mock.Verify(x => x.SoftDeleteByIdAsync(fileId, It.IsAny<CancellationToken>()), Times.Once);

public static void VerifySoftDeleteByIdCalled(this Mock<IFileRepository> mock)
    => mock.Verify(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

public static void VerifySoftDeleteByIdNotCalled(this Mock<IFileRepository> mock)
    => mock.Verify(x => x.SoftDeleteByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
```

---

### 6.7 `MockCloudinaryService.cs`

**File:** `tests/Unit/Common/Mocks/Services/MockCloudinaryService.cs`

**Add setup for video upload:**

```csharp
public static Mock<ICloudinaryService> SetupUploadVideo(
    this Mock<ICloudinaryService> mock, CloudinaryUploadResult? result = null)
{
    mock.Setup(x => x.UploadVideoAsync(
            It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(result ?? DefaultUploadResult());
    return mock;
}
```

---

## Phase 7 — Unit Tests

### 7.1 `FileEntityTests` — 3 new tests

```
Create_WithStorageKey_ShouldSetStorageKey
Create_WithoutStorageKey_ShouldHaveNullStorageKey
Delete_ShouldMarkAsDeleted  (may already exist — check first)
```

### 7.2 `ArticleEntityTests` — 2 new tests

```
UpdateCoverImage_WithFileId_ShouldSetFileIdAndUrl
UpdateCoverImage_WithNull_ShouldClearBoth
```

### 7.3 `VideoEntityTests` — 2 new tests

```
UpdateThumbnail_WithFileId_ShouldSetFileIdAndUrl
UpdateThumbnail_WithNullFileId_ShouldSetUrlAndStorageKeyOnly
```

### 7.4 `ShortVideoEntityTests` — 4 new + ~3 updated

```
CreateStandalone_ShouldSetVideoFileId
CreateTeaser_ShouldSetVideoFileId
UpdateThumbnail_WithFileId_ShouldSetFileIdAndUrl
UpdateThumbnail_WithNullFileId_ShouldSetUrlOnly
```

Existing `CreateStandalone` and `CreateTeaser` tests need `videoFileId` parameter added.

### 7.5 `AdminUploadArticleImageHandlerTests` — 4 tests

```
Handle_WhenCoverImage_ShouldUseFileRepository
Handle_WhenCoverImageWithExistingCover_ShouldRemoveOldCoverAndReplaceFile
Handle_WhenBodyImage_ShouldUseCloudinaryServiceDirectly
Handle_WhenArticleNotFound_ShouldThrowNotFoundException
```

### 7.6 `AdminUploadVideoThumbnailHandlerTests` — 3 tests

```
Handle_WhenVideoFound_ShouldUploadAndSetThumbnailFileId
Handle_WhenVideoNotFound_ShouldThrowNotFoundException
Handle_WhenVideoHasExistingThumbnail_ShouldReplaceFile
```

### 7.7 `AdminUploadShortVideoThumbnailHandlerTests` — 3 tests

```
Handle_WhenShortVideoFound_ShouldUploadAndSetThumbnailFileId
Handle_WhenShortVideoNotFound_ShouldThrowNotFoundException
Handle_WhenExistingThumbnail_ShouldReplaceFile
```

### 7.8 `AdminCreateShortVideoHandlerTests` — update ~4 existing

Replace `MockCloudinaryService` with `MockFileRepository`. Use `SetupUploadAndStoreVideoFile()`.

### 7.9 `AdminDeleteArticleHandlerTests` — 2 new

```
Handle_WhenArticleHasCoverImageFile_ShouldSoftDeleteCoverFile
Handle_WhenArticleHasNoCoverImageFile_ShouldNotCallSoftDelete
```

### 7.10 `AdminDeleteShortVideoHandlerTests` — 2 new + ~2 updated

```
Handle_WhenDeleted_ShouldSoftDeleteVideoAndThumbnailFiles
Handle_WhenNoThumbnailFile_ShouldOnlySoftDeleteVideoFile
```

---

## Complete File Inventory

### Files to create: 0

### Files to modify: ~25

| # | File | Phase |
|---|------|-------|
| 1 | `src/BuildingBlocks/Constants/FileConstants.cs` | 1 |
| 2 | `src/Modules/Core/Core/Domain/Entities/FileEntity.cs` | 1 |
| 3 | `src/Modules/Core/Core/Application/Shared/Services/IFileService.cs` | 1 |
| 4 | `src/Modules/Core/Core/Infrastructure/Services/FileService.cs` | 1 |
| 5 | `src/Modules/Core/Core/Application/Shared/Repositories/IFileRepository.cs` | 1 |
| 6 | `src/Modules/Core/Core/Infrastructure/Repositories/FileRepository.cs` | 1 |
| 7 | `src/Modules/Core/Core/Infrastructure/Persistence/Configurations/FileConfiguration.cs` | 1 |
| 8 | `src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs` | 2 |
| 9 | `src/Modules/Content/Content/Domain/Entities/VideoEntity.cs` | 2 |
| 10 | `src/Modules/Content/Content/Domain/Entities/ShortVideoEntity.cs` | 2 |
| 11 | `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ArticleConfiguration.cs` | 3 |
| 12 | `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/VideoConfiguration.cs` | 3 |
| 13 | `src/Modules/Content/Content/Infrastructure/Persistence/Configurations/ShortVideoConfiguration.cs` | 3 |
| 14 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/AdminUploadArticleImageHandler.cs` | 4 |
| 15 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadVideoThumbnail/AdminUploadVideoThumbnailHandler.cs` | 4 |
| 16 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailHandler.cs` | 4 |
| 17 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/CreateShortVideo/AdminCreateShortVideoHandler.cs` | 4 |
| 18 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/AttachYoutubeVideoUrl/AdminAttachYoutubeVideoUrlHandler.cs` | 4 |
| 19 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteArticle/AdminDeleteArticleHandler.cs` | 5 |
| 20 | `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/DeleteShortVideo/AdminDeleteShortVideoHandler.cs` | 5 |
| 21 | `src/Modules/Content/Content/Infrastructure/BackgroundJobs/AbandonedDraftCleanupJob.cs` | 5 |
| 22 | `tests/Fixtures/Constants/TestConstants.cs` | 6 |
| 23 | `tests/Fixtures/Builders/Entities/FileBuilder.cs` | 6 |
| 24 | `tests/Fixtures/Factories/FileFactory.cs` | 6 |
| 25 | `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs` | 6 |
| 26 | `tests/Fixtures/Factories/Content/ShortVideoFactory.cs` | 6 |
| 27 | `tests/Unit/Common/Mocks/Repositories/MockFileRepository.cs` | 6 |
| 28 | `tests/Unit/Modules/Core/Domain/FileEntityTests.cs` | 7 |
| 29 | `tests/Unit/Modules/Content/Domain/Entities/ArticleEntityTests.cs` | 7 |
| 30 | `tests/Unit/Modules/Content/Domain/Entities/VideoEntityTests.cs` | 7 |
| 31 | `tests/Unit/Modules/Content/Domain/Entities/ShortVideoEntityTests.cs` | 7 |
| 32+ | Handler test files (4.1–4.4 + 5.1–5.2) | 7 |
