# 09 — Test Plan

## Entity Tests

**File:** `tests/Unit/Content/Domain/Entities/CategoryEntityTests.cs`

| Test | Description |
|------|-------------|
| `Create_WithIsExclusive_SetsProperty` | Verify `IsExclusive = true` when passed to `Create()` |
| `Create_DefaultIsExclusive_IsFalse` | Verify `IsExclusive` defaults to `false` when not passed |
| `Update_SetsIsExclusive` | Verify `Update()` sets `IsExclusive` correctly |
| `SetExclusive_SetsToTrue` | Verify `SetExclusive()` sets `IsExclusive = true` |
| `ClearExclusive_SetsToFalse` | Verify `ClearExclusive()` sets `IsExclusive = false` |
| `SetPosterFileId_SetsValue` | Verify `SetPosterFileId(id)` sets `PosterFileId` |
| `SetPosterFileId_WithNull_ClearsValue` | Verify `SetPosterFileId(null)` clears `PosterFileId` |

## Handler Tests

### AdminCreateCategoryHandlerTests

**File:** `tests/Unit/Content/Application/Catalog/Admin/Commands/CreateCategory/AdminCreateCategoryHandlerTests.cs`

| Test | Description |
|------|-------------|
| `Handle_WithIsExclusive_UnsetsCurrentExclusive` | When `IsExclusive = true`, calls `GetExclusiveCategoryAsync`, verifies `ClearExclusive()` on the returned entity |
| `Handle_WithIsExclusiveFalse_DoesNotQueryExclusive` | When `IsExclusive = false`, does not call `GetExclusiveCategoryAsync` |
| `Handle_WithIsExclusive_NoCurrentExclusive_Succeeds` | When `IsExclusive = true` but no current exclusive exists, creates without error |

Changes:
- Add `IFileRepository` mock to constructor
- Add `IsExclusive` param to all `AdminCreateCategoryCommand` constructions
- Update mapper call expectations (sync → async)

### AdminUpdateCategoryHandlerTests

**File:** `tests/Unit/Content/Application/Catalog/Admin/Commands/UpdateCategory/AdminUpdateCategoryHandlerTests.cs`

| Test | Description |
|------|-------------|
| `Handle_WithIsExclusive_UnsetsCurrentExclusive` | Mutex enforcement: old exclusive gets `ClearExclusive()` |
| `Handle_WithIsExclusive_SameCategory_DoesNotClearSelf` | When the category being updated IS the current exclusive, skip `ClearExclusive()` |
| `Handle_WithIsExclusiveFalse_DoesNotQueryExclusive` | No mutex query when not setting exclusive |

Changes:
- Add `IFileRepository` mock to constructor
- Add `IsExclusive` param to all `AdminUpdateCategoryCommand` constructions
- Update mapper call expectations (sync → async)

### AdminUploadCategoryPosterHandlerTests (New)

**File:** `tests/Unit/Content/Application/Catalog/Admin/Commands/UploadCategoryPoster/AdminUploadCategoryPosterHandlerTests.cs`

| Test | Description |
|------|-------------|
| `Handle_UploadsPoster_SetsPosterFileId` | Verify `ReplaceImageFileAsync` called, `SetPosterFileId` called |
| `Handle_ReplacesExistingPoster_SoftDeletesOld` | Verify `ReplaceImageFileAsync` receives current `PosterFileId` |
| `Handle_CategoryNotFound_Throws` | Verify `NotFoundException` when category ID invalid |

### AdminSetExclusiveCategoryHandlerTests (New)

**File:** `tests/Unit/Content/Application/Catalog/Admin/Commands/SetExclusiveCategory/AdminSetExclusiveCategoryHandlerTests.cs`

| Test | Description |
| ---- | ----------- |
| `Handle_SetsExclusive_UnsetsCurrentExclusive` | Calls `GetExclusiveCategoryAsync`, verifies `ClearExclusive()` on old, `SetExclusive()` on new |
| `Handle_SameCategory_DoesNotClearSelf` | When the category is already the exclusive, skip `ClearExclusive()` |
| `Handle_NoCurrentExclusive_Succeeds` | When no category is currently exclusive, sets without error |
| `Handle_CategoryNotFound_Throws` | Invalid ID returns `NotFoundException` |

### AdminSetExclusiveCategoryEndpointV1Tests (New)

**File:** `tests/Unit/Content/Application/Catalog/Admin/Commands/SetExclusiveCategory/V1/AdminSetExclusiveCategoryEndpointV1Tests.cs`

| Test | Description |
| ---- | ----------- |
| `ReturnsOk_WhenExclusiveSet` | Happy path — 200 with updated `CategoryDto` |
| `ReturnsNotFound_WhenCategoryDoesNotExist` | Invalid ID returns 404 |

## Query Handler Tests

For each query handler, add `IFileRepository` mock to the constructor:

| Handler Test File | Change |
|-------------------|--------|
| `AdminGetAllCategoriesHandlerTests` | Add `IFileRepository` mock, update mapper expectations |
| `AdminGetCategoryByIdHandlerTests` | Add `IFileRepository` mock, update mapper expectations |
| `PublicGetActiveCategoriesHandlerTests` | Add `IFileRepository` mock, update mapper expectations |

## Endpoint Tests

### AdminCreateCategoryEndpointV1Tests

Add `IsExclusive` to all request DTO constructions.

### AdminUpdateCategoryEndpointV1Tests

Add `IsExclusive` to all request DTO constructions.

### AdminUploadCategoryPosterEndpointV1Tests (New)

| Test | Description |
|------|-------------|
| `ReturnsOk_WhenPosterUploaded` | Happy path — upload file, get 200 |
| `ReturnsBadRequest_WhenNoFile` | Missing file returns 400 |
| `ReturnsNotFound_WhenCategoryDoesNotExist` | Invalid category ID returns 404 |

## Validator Tests

### AdminCreateCategoryValidatorTests

Add `IsExclusive` to command constructions (pass-through, no special validation).

### AdminUpdateCategoryValidatorTests

Add `IsExclusive` to command constructions.

### AdminUploadCategoryPosterValidatorTests (New)

| Test | Description |
|------|-------------|
| `Valid_WhenIdAndFileProvided` | Standard valid case |
| `Invalid_WhenIdEmpty` | Empty ID fails |
| `Invalid_WhenFileNull` | Null file fails |

## Specification Tests

**File:** `tests/Unit/Content/Application/Catalog/Specifications/CategorySpecificationTests.cs`

| Test | Description |
|------|-------------|
| `ExclusiveCategorySpecification_MatchesActiveExclusive` | `IsExclusive = true && IsActive = true` → matches |
| `ExclusiveCategorySpecification_DoesNotMatchInactive` | `IsExclusive = true && IsActive = false` → no match |
| `ExclusiveCategorySpecification_DoesNotMatchNonExclusive` | `IsExclusive = false && IsActive = true` → no match |

## Mapper Tests

**File:** `tests/Unit/Content/Application/Shared/Mappers/CategoryMapperTests.cs`

| Test | Description |
|------|-------------|
| `ToCategoryDtoAsync_WithPosterFileId_ResolvesPosterUrl` | Sets `PosterUrl` from `FileEntity.StorageUrl` |
| `ToCategoryDtoAsync_WithNoPosterFileId_PosterUrlIsNull` | `PosterUrl` is null when `PosterFileId` is null |
| `ToCategoryDtoAsync_MapsIsExclusive` | `IsExclusive` maps through correctly |
| `ToCategoryDtosAsync_ResolvesAllPosterUrls` | Collection mapping resolves each poster |

Changes:
- Convert sync test methods to async
- Add `IFileRepository` mock setup returning `FileEntity` with `StorageUrl`

## Test Fixtures

### CategoryBuilder

**File:** `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs`

Add:

```csharp
private bool _isExclusive;
private Guid? _posterFileId;

public CategoryBuilder WithIsExclusive(bool isExclusive = true)
{
    _isExclusive = isExclusive;
    return this;
}

public CategoryBuilder WithPosterFileId(Guid? posterFileId = null)
{
    _posterFileId = posterFileId ?? Guid.NewGuid();
    return this;
}
```

Update `Build()` to pass `isExclusive` to `CategoryEntity.Create()` and call `SetPosterFileId()` if set.

### CategoryFactory

**File:** `tests/Fixtures/Factories/Content/CategoryFactory.cs`

Add `isExclusive` parameter to factory methods, defaulting to `false`.

### MockCategoryRepository

**File:** `tests/Fixtures/Mocks/Content/MockCategoryRepository.cs`

Add mock setup for `GetExclusiveCategoryAsync()`.
