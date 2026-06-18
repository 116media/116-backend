# Phase 3 — EF Core Migrations

---

## Overview

Two migrations are needed — one per `DbContext`:

1. **Core migration** — adds `StorageKey` column to `core.files`
2. **Content migration** — adds `*FileId` FK columns to content entities, removes `VideoStorageKey` / `ThumbnailStorageKey` from `ShortVideoEntity`

---

## Migration 1: Core Module

### Command

```bash
dotnet ef migrations add AddStorageKeyToFileEntity \
  --project src/Modules/Core/Core/Infrastructure \
  --startup-project src/Api \
  --context CoreDbContext
```

### Schema Changes

| Table | Column | Type | Nullable | FK |
|-------|--------|------|----------|-----|
| `core.files` | `storage_key` | `varchar(500)` | YES | — |

### EF Configuration

**File:** `src/Modules/Core/Core/Infrastructure/Persistence/Configurations/FileConfiguration.cs`

```csharp
builder.Property(x => x.StorageKey)
    .HasMaxLength(FileConstants.MaxStorageKeyLength)
    .IsRequired(false);
```

---

## Migration 2: Content Module

### Command

```bash
dotnet ef migrations add AddFileIdColumnsToContentEntities \
  --project src/Modules/Content/Content/Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

### Schema Changes

| Table | Column | Type | Nullable | FK Target |
|-------|--------|------|----------|-----------|
| `content.articles` | `cover_image_file_id` | `uuid` | YES | `core.files(id)` |
| `content.videos` | `thumbnail_file_id` | `uuid` | YES | `core.files(id)` |
| `content.short_videos` | `video_file_id` | `uuid` | NO | `core.files(id)` |
| `content.short_videos` | `thumbnail_file_id` | `uuid` | YES | `core.files(id)` |

### Dropped Columns

| Table | Column | Reason |
|-------|--------|--------|
| `content.short_videos` | `video_storage_key` | Tracked by `FileEntity.StorageKey` |
| `content.short_videos` | `thumbnail_storage_key` | Tracked by `FileEntity.StorageKey` |

---

## Cross-Schema FK Considerations

The FK columns in `content.*` tables reference `core.files(id)`. This works because:
- Both schemas are in the same PostgreSQL database
- EF Core supports cross-schema FKs via `.HasOne().WithMany().HasForeignKey()`
- The `ContentDbContext` does NOT own the `FileEntity` — it only declares the FK column

### EF Configuration Pattern

**Do NOT add a `DbSet<FileEntity>` to `ContentDbContext`.** Instead, configure the FK as a shadow property or use `HasOne()` without navigation:

```csharp
// ArticleConfiguration.cs
builder.Property(x => x.CoverImageFileId);
// FK constraint is enforced at the database level via migration SQL
```

Or if you want EF to manage the FK:

```csharp
builder.HasOne<FileEntity>()
    .WithMany()
    .HasForeignKey(x => x.CoverImageFileId)
    .OnDelete(DeleteBehavior.SetNull);
```

This requires `ContentDbContext` to reference the `Core.Domain` project (which it likely already does via `IFileRepository`).

---

## Data Migration for Existing Records

### `short_videos` — `video_file_id` is NOT NULL

Existing short videos have `VideoUrl` and `VideoStorageKey` but no `FileEntity`. The migration must:

1. For each existing `short_videos` row, create a `FileEntity` in `core.files` with:
   - `FileName` = `VideoStorageKey`
   - `OriginalFileName` = extracted from URL or use storage key
   - `MimeType` = `"video/mp4"` (default assumption)
   - `StorageUrl` = `VideoUrl`
   - `StorageKey` = `VideoStorageKey`
   - `SizeInBytes` = 0 (unknown, acceptable for legacy data)
2. Set `short_videos.video_file_id` = new `FileEntity.Id`
3. Then apply the NOT NULL constraint

### Migration SQL sketch

```sql
-- Step 1: Add column as nullable first
ALTER TABLE content.short_videos ADD COLUMN video_file_id uuid NULL;
ALTER TABLE content.short_videos ADD COLUMN thumbnail_file_id uuid NULL;

-- Step 2: Create FileEntity records for existing short videos
-- (This should be done in a C# data migration or a seed script)

-- Step 3: Apply NOT NULL constraint after data is populated
ALTER TABLE content.short_videos ALTER COLUMN video_file_id SET NOT NULL;

-- Step 4: Add FK constraints
ALTER TABLE content.short_videos
    ADD CONSTRAINT fk_short_videos_video_file
    FOREIGN KEY (video_file_id) REFERENCES core.files(id);

-- Step 5: Drop old columns
ALTER TABLE content.short_videos DROP COLUMN video_storage_key;
ALTER TABLE content.short_videos DROP COLUMN thumbnail_storage_key;
```

### Nullable FK columns (articles, videos)

`cover_image_file_id` and `thumbnail_file_id` are nullable — existing rows simply have `NULL`. No data migration needed for these.

---

## Rollback Strategy

If the migration needs to be reverted:

1. The `Down()` method should re-add `video_storage_key` and `thumbnail_storage_key`
2. Populate them from the linked `FileEntity.StorageKey`
3. Drop the FK columns
4. Drop `storage_key` from `core.files`

---

## Verification

After running both migrations:

```bash
dotnet ef database update \
  --project src/Modules/Core/Core/Infrastructure \
  --startup-project src/Api \
  --context CoreDbContext

dotnet ef database update \
  --project src/Modules/Content/Content/Infrastructure \
  --startup-project src/Api \
  --context ContentDbContext
```

Verify:
```sql
-- Check core.files has storage_key
\d core.files

-- Check content tables have new FK columns
\d content.articles
\d content.videos
\d content.short_videos
```
