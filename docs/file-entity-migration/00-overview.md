# File Entity Migration — Overview

This migration normalizes 4 media upload flows to use the centralized `FileEntity` tracking system from the Core module. Currently, only user avatars and payment proofs go through `IFileRepository` → `FileEntity`. All other uploads bypass it and store URLs directly on domain entities.

---

## Goal

Every user-uploaded file (`IFormFile`) should be tracked by a `FileEntity` record in the `core.files` table. This gives us:

- Centralized audit trail for all uploaded media
- Consistent soft-delete semantics
- Single source of truth for Cloudinary storage keys
- Uniform file metadata (size, MIME type, original name)

---

## What Changes

| Upload | Current Flow | New Flow |
|--------|-------------|----------|
| Article cover image | `ICloudinaryService` → `ArticleImageEntity` | `IFileRepository` → `FileEntity` → `ArticleEntity.CoverImageFileId` |
| Video thumbnail | `ICloudinaryService` → `VideoEntity.ThumbnailUrl` | `IFileRepository` → `FileEntity` → `VideoEntity.ThumbnailFileId` |
| Short video thumbnail | `ICloudinaryService` → `ShortVideoEntity.ThumbnailUrl` | `IFileRepository` → `FileEntity` → `ShortVideoEntity.ThumbnailFileId` |
| Short video file | `ICloudinaryService` → `ShortVideoEntity.VideoUrl` | `IFileRepository` → `FileEntity` → `ShortVideoEntity.VideoFileId` |

## What Does NOT Change

| Upload | Reason |
|--------|--------|
| Article body images | Multiple per article, managed by image diff algorithm — stays in `ArticleImageEntity` |
| YouTube auto-thumbnails | No user-uploaded file — stays as URL on `VideoEntity` |
| User avatars | Already uses `FileEntity` ✓ |
| Payment proofs | Already uses `FileEntity` ✓ |

---

## Documents in This Folder

| Doc | Content |
|-----|---------|
| [01-file-entity-changes.md](01-file-entity-changes.md) | `FileEntity` and `IFileRepository` infrastructure changes |
| [02-article-cover-image.md](02-article-cover-image.md) | Article cover image migration |
| [03-video-thumbnail.md](03-video-thumbnail.md) | Video thumbnail migration |
| [04-short-video-thumbnail.md](04-short-video-thumbnail.md) | Short video thumbnail migration |
| [05-short-video-file.md](05-short-video-file.md) | Short video file migration |
| [06-entity-changes.md](06-entity-changes.md) | All domain entity property changes |
| [07-ef-migrations.md](07-ef-migrations.md) | EF Core migration plan |
| [08-delete-handler-changes.md](08-delete-handler-changes.md) | Delete handler updates for FileEntity cleanup |
| [09-tests.md](09-tests.md) | Full test plan following existing test conventions |

---

## Execution Order

1. **Phase 1 — Infrastructure** (doc 01): Add `StorageKey` to `FileEntity`, add generic upload methods to `IFileRepository`
2. **Phase 2 — Entity changes** (doc 06): Add `*FileId` FK properties to content entities
3. **Phase 3 — EF migration** (doc 07): Create migration for all schema changes
4. **Phase 4 — Handler migration** (docs 02–05): Update upload handlers one at a time
5. **Phase 5 — Delete handlers** (doc 08): Update delete handlers to soft-delete `FileEntity`
6. **Phase 6 — Tests** (doc 09): Add/update tests for all changed code
