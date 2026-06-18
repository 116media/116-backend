# Phase 2 — Domain Entity Property Changes

---

## 1. `ArticleEntity`

**File:** `src/Modules/Content/Content/Domain/Entities/ArticleEntity.cs`

### Add property

```csharp
/// <summary>
/// ID of the uploaded cover image file, if any.
/// References a FileEntity in the Core module.
/// </summary>
public Guid? CoverImageFileId { get; private set; }
```

### Update `UpdateCoverImage()` method

**Before:**
```csharp
public void UpdateCoverImage(string? coverImageUrl)
{
    CoverImageUrl = coverImageUrl;
}
```

**After:**
```csharp
public void UpdateCoverImage(Guid? coverImageFileId, string? coverImageUrl)
{
    CoverImageFileId = coverImageFileId;
    CoverImageUrl = coverImageUrl;
}
```

### Keep `CoverImageUrl`

`CoverImageUrl` remains as a denormalized field for fast reads. When a cover is uploaded, both `CoverImageFileId` and `CoverImageUrl` are set. This avoids a JOIN to `core.files` for every article query.

---

## 2. `VideoEntity`

**File:** `src/Modules/Content/Content/Domain/Entities/VideoEntity.cs`

### Add property

```csharp
/// <summary>
/// ID of the uploaded thumbnail file, if any.
/// Null for auto-downloaded YouTube thumbnails.
/// </summary>
public Guid? ThumbnailFileId { get; private set; }
```

### Update `UpdateThumbnail()` method

**Before:**
```csharp
public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
}
```

**After:**
```csharp
public void UpdateThumbnail(Guid? thumbnailFileId, string thumbnailUrl)
{
    ThumbnailFileId = thumbnailFileId;
    ThumbnailUrl = thumbnailUrl;
}
```

### Keep `ThumbnailUrl` and `ThumbnailStorageKey`

- `ThumbnailUrl` — denormalized for fast reads
- `ThumbnailStorageKey` — still needed for YouTube auto-thumbnails (no `FileEntity`). For manual uploads, the `FileEntity.StorageKey` is the authoritative source, but `ThumbnailStorageKey` is still set for backward compatibility

**Alternative:** Remove `ThumbnailStorageKey` entirely and only use `FileEntity.StorageKey` for manual uploads. YouTube auto-thumbnails would need a different cleanup strategy. This is a judgment call — keeping `ThumbnailStorageKey` is safer for now.

---

## 3. `ShortVideoEntity`

**File:** `src/Modules/Content/Content/Domain/Entities/ShortVideoEntity.cs`

### Add properties

```csharp
/// <summary>
/// ID of the uploaded video file.
/// References a FileEntity in the Core module.
/// </summary>
public Guid VideoFileId { get; private set; }

/// <summary>
/// ID of the uploaded thumbnail file, if any.
/// Null for auto-generated thumbnails.
/// </summary>
public Guid? ThumbnailFileId { get; private set; }
```

### Update factory methods

**`CreateStandalone()` — Before:**
```csharp
public static ShortVideoEntity CreateStandalone(
    Guid id,
    string title,
    string slug,
    string videoUrl,
    string videoStorageKey,
    Guid authorId,
    ShortVideoErrors errors
)
```

**After:**
```csharp
public static ShortVideoEntity CreateStandalone(
    Guid id,
    string title,
    string slug,
    Guid videoFileId,
    string videoUrl,
    Guid authorId,
    ShortVideoErrors errors
)
```

Same for `CreateTeaser()`.

Inside the factory, set:
```csharp
VideoFileId = videoFileId,
VideoUrl = videoUrl,
```

### Update `UpdateThumbnail()`

**Before:**
```csharp
public void UpdateThumbnail(string thumbnailUrl, string thumbnailStorageKey)
{
    ThumbnailUrl = thumbnailUrl;
    ThumbnailStorageKey = thumbnailStorageKey;
}
```

**After:**
```csharp
public void UpdateThumbnail(Guid? thumbnailFileId, string thumbnailUrl)
{
    ThumbnailFileId = thumbnailFileId;
    ThumbnailUrl = thumbnailUrl;
}
```

### Remove properties

- `VideoStorageKey` — tracked by `FileEntity.StorageKey`
- `ThumbnailStorageKey` — for manual uploads tracked by `FileEntity.StorageKey`; for auto-generated thumbnails, the key is derived from the video URL

### Keep properties

- `VideoUrl` — denormalized for fast reads
- `ThumbnailUrl` — denormalized for fast reads

---

## 4. `FileEntity`

See [01-file-entity-changes.md](01-file-entity-changes.md).

Summary: Add `StorageKey` (string?) property, update `Create()` factory.

---

## Summary Table

| Entity | New Properties | Updated Methods | Removed Properties |
|--------|---------------|----------------|-------------------|
| `ArticleEntity` | `CoverImageFileId` (Guid?) | `UpdateCoverImage()` | — |
| `VideoEntity` | `ThumbnailFileId` (Guid?) | `UpdateThumbnail()` | — |
| `ShortVideoEntity` | `VideoFileId` (Guid), `ThumbnailFileId` (Guid?) | `CreateStandalone()`, `CreateTeaser()`, `UpdateThumbnail()` | `VideoStorageKey`, `ThumbnailStorageKey` |
| `FileEntity` | `StorageKey` (string?) | `Create()` | — |
