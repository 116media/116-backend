# Phase 4a — Article Cover Image Migration

---

## Current Flow

```
IFormFile
  → AdminUploadArticleImageHandler
  → ICloudinaryService.UploadImageAsync()
  → ArticleImageEntity.Create(storageKey, url, ImageType.Cover)
  → ArticleEntity.UpdateCoverImage(coverImageUrl)
```

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/AdminUploadArticleImageHandler.cs`

The handler currently:
1. Parses `articleId` from the command
2. Loads the article via `articleRepository.GetByIdOrThrowAsync()`
3. Determines if it's a cover or body image
4. Uploads to Cloudinary via `cloudinaryService.UploadImageAsync()`
5. For covers: removes old `ArticleImageEntity` with `ImageType.Cover`, updates `ArticleEntity.CoverImageUrl`
6. Creates a new `ArticleImageEntity` with the Cloudinary URL and storage key
7. Commits

---

## New Flow (Cover Only)

```
IFormFile
  → AdminUploadArticleImageHandler
  → IFileRepository.ReplaceImageFileAsync()
  → FileEntity created in core.files
  → ArticleEntity.UpdateCoverImage(fileId, url)
  → Old ArticleImageEntity(Cover) removed
```

Body images remain unchanged — they still go through `ICloudinaryService` → `ArticleImageEntity`.

---

## Handler Changes

**File:** `src/Modules/Content/Content/Application/Editorial/UseCases/Admin/Commands/UploadArticleImage/AdminUploadArticleImageHandler.cs`

### New dependency

Add `IFileRepository fileRepository` to the constructor.

### Updated cover image logic

```csharp
if (isCover)
{
    // Remove old ArticleImageEntity(Cover) if exists
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

    // Upload via FileRepository → creates FileEntity
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

    // Return result (ArticleImageEntity is no longer created for covers)
    return new AdminUploadArticleImageResult(Image: new ArticleImageDto(
        Id: fileEntity.Id,
        StorageKey: fileEntity.StorageKey!,
        Url: fileEntity.StorageUrl,
        ImageType: EnumArticleImageType.Cover
    ));
}
```

Body image logic (the `else` branch) stays exactly the same — continues using `ICloudinaryService` directly and creating `ArticleImageEntity`.

---

## ArticleEntity Changes

See [06-entity-changes.md](06-entity-changes.md) for full details.

Summary:
- Add `CoverImageFileId` (Guid?) property
- Update `UpdateCoverImage()` to accept `(Guid? coverImageFileId, string? coverImageUrl)`
- Keep `CoverImageUrl` for fast reads (denormalized)

---

## Impact on Other Handlers

### `AdminUpdateArticleHandler`

The image diff algorithm for body images stays unchanged. The cover image is now tracked separately via `CoverImageFileId`, so the diff algorithm should skip the cover URL when comparing body images.

No change needed — the diff compares `ArticleImageEntity` records, and we stop creating `ArticleImageEntity` for covers.

### `AdminDeleteArticleHandler`

Must also soft-delete the cover `FileEntity` — see [08-delete-handler-changes.md](08-delete-handler-changes.md).

### `AbandonedDraftCleanupJob`

Same — must soft-delete cover `FileEntity` — see [08-delete-handler-changes.md](08-delete-handler-changes.md).

---

## Files Changed

| File | Change |
|------|--------|
| `AdminUploadArticleImageHandler.cs` | Add `IFileRepository`, use for cover uploads |
| `ArticleEntity.cs` | Add `CoverImageFileId`, update `UpdateCoverImage()` |
| `ArticleConfiguration.cs` | Add FK column |
| `AdminDeleteArticleHandler.cs` | Soft-delete cover FileEntity |
| `AbandonedDraftCleanupJob.cs` | Soft-delete cover FileEntity |
