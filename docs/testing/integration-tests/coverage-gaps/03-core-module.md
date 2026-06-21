# Core Module - Integration Test Coverage Specifications

**Current Coverage:** 38.1% (110 / 288 lines) | Branch: 6.7%
**Uncovered Lines:** 178
**Target:** 100% on error messages; ~42% overall (structurally limited)

## 1. Structurally Uncoverable Code (~146 lines)

| Class | Lines | Reason |
|-------|-------|--------|
| `CloudinaryService` | ~80 | Stubbed — makes HTTP calls to Cloudinary API |
| `FileService` (4%) | ~20 | Depends on stubbed CloudinaryService |
| `CoreUnitOfWork` | ~10 | Thin DI wrapper, used internally |
| `SlugHelper` | ~8 | Utility used internally by stubbed services |
| `FileDownloadResult` | ~5 | DTO used by stubbed FileService |

### File Specifications at 0% (all structurally blocked)

| Specification | Lines | Used By |
|---------------|-------|---------|
| `FileByFileNameSpecification` | ~3 | FileService |
| `FileByMimeTypeSpecification` | ~3 | FileService |
| `FileByOriginalFileNameSpecification` | ~3 | FileService |
| `FileBySizeRangeSpecification` | ~3 | FileService |
| `FileIsDeletedSpecification` | ~3 | FileService |
| `FileIsImageSpecification` | ~3 | FileService |
| `FileIsValidAvatarSpecification` | ~3 | FileService |

All 7 specifications are only exercised through the stubbed CloudinaryService/FileService pipeline. No integration test can reach them.

## 2. Error Classes — Target 100%

### FileErrors.cs (17.2%) — 24 methods

All are thin factories returning exception instances, called through `CoreI18n.File.*`.

| Method | Exception Type | Called By | Coverable? |
|--------|---------------|-----------|-----------|
| `FileNotFound(id)` | NotFoundException | FileRepository lookup | Yes — any handler referencing non-existent file |
| `FileNotFoundByName(name)` | NotFoundException | FileRepository.GetByNameAsync | Yes — via content handler referencing non-existent file name |
| `FileRequired()` | BadRequestException | FileEntity validation | Yes — upload endpoint without file |
| `InvalidFileType(type)` | BadRequestException | FileEntity/FileService validation | Yes — upload with unsupported MIME type |
| `InvalidFileExtension(ext)` | BadRequestException | FileEntity/FileService validation | Yes — upload with wrong extension |
| `FileTooLarge(size, max)` | BadRequestException | FileService size check | Partially — depends on stubbed FileService |
| `FileTooLarge(max)` | BadRequestException | FileService size check | Same as above |
| `UnsupportedFileType(type)` | BadRequestException | FileService | No — behind stub |
| `CorruptedFile()` | BadRequestException | FileService | No — behind stub |
| `FileNameRequired()` | BadRequestException | FileEntity.Create | Yes — entity validation |
| `OriginalFileNameRequired()` | BadRequestException | FileEntity.Create | Yes — entity validation |
| `MimeTypeRequired()` | BadRequestException | FileEntity.Create | Yes — entity validation |
| `StorageUrlRequired()` | BadRequestException | FileEntity.Create | Yes — entity validation |
| `FileSizeMustBeGreaterThanZero()` | BadRequestException | FileEntity.Create | Yes — entity validation |
| `FileUploadFailed(fileName)` | InternalServerException | CloudinaryService | No — behind stub |
| `FileUploadFailed(fileName, reason)` | InternalServerException | CloudinaryService | No — behind stub |
| `InvalidFileUrl(url)` | BadRequestException | FileService | No — behind stub |
| `FileStorageFailed(reason)` | InternalServerException | FileService | No — behind stub |
| `FileDownloadFailed(reason)` | InternalServerException | FileService | No — behind stub |
| `FileUrlRequired()` | BadRequestException | FileEntity validation | Yes — entity validation |
| `InvalidConfiguration(detail)` | InternalServerException | CloudinaryService | No — behind stub |
| `ServiceUnavailable()` | InternalServerException | CloudinaryService | No — behind stub |
| `DatabaseConnectionFailed()` | InternalServerException | Infrastructure | No — requires DB failure |
| `BadRequest(detail)` | BadRequestException | Various | Yes — generic error factory |

**Coverable methods (integration tests that trigger Content/Identity file operations):**

| # | Test Scenario | Endpoint | Seed Data | Expected | Covers |
|---|--------------|----------|-----------|----------|--------|
| 1 | Upload avatar with invalid MIME type | `PATCH /api/v1/public/auth/update-avatar` | Verified active user | 400 | `InvalidFileType()` |
| 2 | Upload avatar with wrong extension | `PATCH /api/v1/public/auth/update-avatar` | Verified active user | 400 | `InvalidFileExtension()` |
| 3 | Upload category poster without file | `PUT /api/v1/admin/categories/{id}/poster` | Active category | 400 | `FileRequired()` |

**Blocked methods (behind CloudinaryService stub):** `UnsupportedFileType`, `CorruptedFile`, `FileUploadFailed` (both), `InvalidFileUrl`, `FileStorageFailed`, `FileDownloadFailed`, `InvalidConfiguration`, `ServiceUnavailable`, `DatabaseConnectionFailed`, `FileTooLarge` (both). These require unit tests or a real Cloudinary test double.

### Error Messages

| Class | Coverage | Notes |
|-------|----------|-------|
| `ConflictErrorMessage` | 33.3% | `FileUploadFailed(fileName, reason)` — behind stub. Covered transitively by Content conflict tests. |
| `InternalServerErrorMessage` | 16.6% | `ServiceUnavailable`, `DatabaseConnectionFailed`, `FileDownloadFailed`, `FileStorageFailed` — all behind stub or require infra failure. Structurally blocked. |
| `ValidationErrorMessage` | 5.2% | 19 methods. `StorageUrlCannotBeEmpty` is **dead code** (zero callers). Others are called by FileErrors methods — covered transitively when FileErrors are triggered. |

### CoreI18n Facade (50%)

`CoreI18n.File` property — covered automatically when any Content/Identity test triggers a Core error path. No dedicated test needed.

### FileEntity (76.6%)

Covered transitively when file upload/operations run through handlers. The uncovered entity methods are behind the Cloudinary stub.

## 3. Dead Code

| Code | Reason |
|------|--------|
| `ValidationErrorMessage.StorageUrlCannotBeEmpty` | Zero callers in entire codebase |

## 4. Realistic Coverage Target

| Category | Current Lines | Achievable | Blocked |
|----------|--------------|-----------|---------|
| CloudinaryService + FileService | ~100 lines at 0-4% | +0 | ~100 |
| File Specifications | 0% | +0 | ~21 |
| FileErrors | 17.2% | ~35% (+4 methods) | ~12 methods behind stub |
| Error messages | 5-33% | ~20% (transitively) | Most behind stub |
| CoreI18n | 50% | 100% (transitively) | — |
| FileEntity | 76.6% | ~80% | Some behind stub |
| **Module total** | **38.1%** | **~42%** | **~146 lines** |

**Conclusion:** Core module coverage is structurally capped at ~42% by the Cloudinary stub. The 3 coverable FileErrors methods are covered transitively by Identity/Content file upload tests. No dedicated Core integration tests needed — all gains come from Identity (PublicUpdateAvatar) and Content (UploadCategoryPoster) tests.
