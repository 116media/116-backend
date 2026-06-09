# Phase 1 — FileEntity & IFileRepository Infrastructure Changes

---

## 1. Add `StorageKey` to `FileEntity`

**File:** `src/Modules/Core/Core/Domain/Entities/FileEntity.cs`

Currently, `FileEntity` stores `StorageUrl` (the Cloudinary HTTPS URL) but has no `StorageKey` (the Cloudinary `publicId` needed for deletion). Content entities like `VideoEntity` and `ShortVideoEntity` store their own `StorageKey` — this should be centralized.

### Add property

```csharp
/// <summary>
/// Cloud storage key used to identify and delete the file from the provider.
/// For Cloudinary, this is the public ID.
/// </summary>
[MaxLength(FileConstants.MaxStorageKeyLength)]
public string? StorageKey { get; private set; }
```

### Add constant

**File:** `src/BuildingBlocks/Constants/FileConstants.cs`

```csharp
public const int MaxStorageKeyLength = 500;
```

### Update `Create()` factory

Add `storageKey` parameter:

```csharp
public static FileEntity Create(
    Guid id,
    string fileName,
    string originalFileName,
    string mimeType,
    string storageUrl,
    long sizeInBytes,
    CoreI18n i18n,
    string? storageKey = null  // <-- new parameter
)
{
    // ... existing validation ...

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

### Add update method

```csharp
/// <summary>
/// Updates the storage key of the file.
/// </summary>
public void UpdateStorageKey(string storageKey)
{
    StorageKey = storageKey;
}
```

---

## 2. Add generic upload methods to `IFileRepository`

**File:** `src/Modules/Core/Core/Application/Shared/Repositories/IFileRepository.cs`

The existing methods are avatar-specific (`UploadAndStoreAvatarAsync`, `UpdateAvatarFromFileAsync`). Add generic methods that any module can use.

### New interface methods

```csharp
/// <summary>
/// Uploads an image file to cloud storage and persists file metadata to the database.
/// </summary>
/// <param name="file">The image file to upload.</param>
/// <param name="publicId">The public ID for cloud storage.</param>
/// <param name="folder">The destination folder in cloud storage.</param>
/// <param name="originalFileName">The original filename as submitted by the client.</param>
/// <param name="mimeType">The MIME type of the file.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created FileEntity with persisted metadata.</returns>
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
/// <param name="file">The video file to upload.</param>
/// <param name="publicId">The public ID for cloud storage.</param>
/// <param name="folder">The destination folder in cloud storage.</param>
/// <param name="originalFileName">The original filename as submitted by the client.</param>
/// <param name="mimeType">The MIME type of the file.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The created FileEntity with persisted metadata.</returns>
Task<FileEntity> UploadAndStoreVideoFileAsync(
    IFormFile file,
    string publicId,
    string folder,
    string originalFileName,
    string mimeType,
    CancellationToken cancellationToken = default
);

/// <summary>
/// Replaces a tracked file by soft-deleting the old FileEntity and uploading a new image.
/// </summary>
/// <param name="currentFileId">The current file ID to replace (may be null).</param>
/// <param name="file">The new image file to upload.</param>
/// <param name="publicId">The public ID for cloud storage.</param>
/// <param name="folder">The destination folder in cloud storage.</param>
/// <param name="originalFileName">The original filename.</param>
/// <param name="mimeType">The MIME type.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>The newly created FileEntity.</returns>
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
/// <param name="fileId">The ID of the file to soft-delete.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>True if the file was found and deleted; false otherwise.</returns>
Task<bool> SoftDeleteByIdAsync(
    Guid fileId,
    CancellationToken cancellationToken = default
);
```

---

## 3. Implement in `FileRepository`

**File:** `src/Modules/Core/Core/Infrastructure/Repositories/FileRepository.cs`

### `UploadAndStoreImageFileAsync`

```csharp
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
```

### `UploadAndStoreVideoFileAsync`

Same pattern but calls `cloudinaryService.UploadVideoAsync()` instead. The `IFileService` does not currently have a video upload method, so either:

- **Option A:** Add `UploadVideoFileAsync()` to `IFileService` (wraps `ICloudinaryService.UploadVideoAsync`)
- **Option B:** Inject `ICloudinaryService` directly into `FileRepository` for video uploads

Option A is preferred for consistency.

### `ReplaceImageFileAsync`

```csharp
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
```

### `SoftDeleteByIdAsync`

```csharp
public async Task<bool> SoftDeleteByIdAsync(
    Guid fileId,
    CancellationToken cancellationToken = default
)
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

---

## 4. Add `UploadVideoFileAsync` to `IFileService`

**File:** `src/Modules/Core/Core/Application/Shared/Services/IFileService.cs`

```csharp
/// <summary>
/// Uploads a video file to cloud storage.
/// </summary>
/// <param name="file">The video file to upload.</param>
/// <param name="publicId">The public ID for the file.</param>
/// <param name="folder">Optional folder path in cloud storage.</param>
/// <param name="cancellationToken">Cancellation token.</param>
/// <returns>Upload result containing public URL, metadata, and generated file ID.</returns>
Task<FileUploadResult> UploadVideoFileAsync(
    IFormFile file,
    string publicId,
    string? folder = null,
    CancellationToken cancellationToken = default
);
```

Implementation wraps `ICloudinaryService.UploadVideoAsync()` and maps the result to `FileUploadResult`.

---

## 5. Update existing `UploadAndStoreAvatarAsync`

Pass `storageKey` to `FileEntity.Create()` so avatar files also have their storage key tracked:

```csharp
var fileEntity = FileEntity.Create(
    id: uploadResult.FileId,
    fileName: userId,
    originalFileName: originalFileName,
    mimeType: mimeType,
    storageUrl: uploadResult.SecureUrl,
    sizeInBytes: uploadResult.Bytes,
    i18n: i18n,
    storageKey: uploadResult.PublicId  // <-- add this
);
```

Same for `UploadAndStoreRawFileAsync`.

---

## 6. Update `FileEntity` EF Configuration

**File:** `src/Modules/Core/Core/Infrastructure/Persistence/Configurations/FileConfiguration.cs`

```csharp
builder.Property(x => x.StorageKey)
    .HasMaxLength(FileConstants.MaxStorageKeyLength)
    .IsRequired(false);
```

---

## Files Changed

| File | Change |
|------|--------|
| `Core/Domain/Entities/FileEntity.cs` | Add `StorageKey` property, update `Create()`, add `UpdateStorageKey()` |
| `BuildingBlocks/Constants/FileConstants.cs` | Add `MaxStorageKeyLength` |
| `Core/Application/Shared/Repositories/IFileRepository.cs` | Add 4 new methods |
| `Core/Infrastructure/Repositories/FileRepository.cs` | Implement 4 new methods, update existing methods |
| `Core/Application/Shared/Services/IFileService.cs` | Add `UploadVideoFileAsync()` |
| `Core/Infrastructure/Services/FileService.cs` | Implement `UploadVideoFileAsync()` |
| `Core/Infrastructure/Persistence/Configurations/FileConfiguration.cs` | Add `StorageKey` column |
