# Test Mocks Documentation

## Overview

Mock classes are located in `tests/Unit/Common/Mocks/`.
They provide reusable setup helpers via extension methods on `Mock<T>`.

---

## MockLookupRepository

File: `tests/Unit/Common/Mocks/Repositories/MockLookupRepository.cs`

Interface mocked: `ILookupRepository`

### Factory

| Method | Description |
|--------|-------------|
| `MockLookupRepository.Create()` | Creates a `Mock<ILookupRepository>` with all defaults set |

### Defaults (set automatically on `Create()`)

- `ContentTypeExistsByNameAsync` → returns `false`
- `AddContentTypeAsync` → completes
- `GetAllContentTypesAsync` → returns empty list
- `PricingTierExistsByNameAsync` → returns `false`
- `AddPricingTierAsync` → completes
- `GetAllPricingTiersAsync` → returns empty list
- `PromotionLevelExistsByNameAsync` → returns `false`
- `AddPromotionLevelAsync` → completes
- `GetAllPromotionLevelsAsync` → returns empty list
- `GetActivePromotionLevelsAsync` → returns empty list
- `GetTagBySlugAsync` → returns `null`
- `AddTagAsync` → completes
- `GetAllTagsAsync` → returns empty list

### ContentType Setup Methods

| Method | Description |
|--------|-------------|
| `SetupContentTypeExistsByName(string name, bool exists)` | Configures `ContentTypeExistsByNameAsync` |
| `SetupGetContentTypeByIdOrThrow(ContentTypeEntity entity)` | Returns entity for `entity.Id` |
| `SetupGetContentTypeByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` for given ID |
| `SetupGetAllContentTypes(IReadOnlyList<ContentTypeEntity> list)` | Returns given list |

### ContentType Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddContentTypeCalled()` | Verifies `AddContentTypeAsync` was called once |
| `VerifyAddContentTypeNotCalled()` | Verifies `AddContentTypeAsync` was never called |

### PricingTier Setup Methods

| Method | Description |
|--------|-------------|
| `SetupPricingTierExistsByName(string name, bool exists)` | Configures `PricingTierExistsByNameAsync` |
| `SetupGetPricingTierByIdOrThrow(PricingTierEntity entity)` | Returns entity for `entity.Id` |
| `SetupGetPricingTierByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` for given ID |
| `SetupGetAllPricingTiers(IReadOnlyList<PricingTierEntity> list)` | Returns given list |

### PricingTier Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddPricingTierCalled()` | Verifies `AddPricingTierAsync` was called once |
| `VerifyAddPricingTierNotCalled()` | Verifies `AddPricingTierAsync` was never called |

### PromotionLevel Setup Methods

| Method | Description |
|--------|-------------|
| `SetupPromotionLevelExistsByName(string name, bool exists)` | Configures `PromotionLevelExistsByNameAsync` |
| `SetupGetPromotionLevelByIdOrThrow(PromotionLevelEntity entity)` | Returns entity for `entity.Id` |
| `SetupGetPromotionLevelByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` for given ID |
| `SetupGetAllPromotionLevels(IReadOnlyList<PromotionLevelEntity> list)` | Returns given list |
| `SetupGetActivePromotionLevels(IReadOnlyList<PromotionLevelEntity> list)` | Returns given list |

### PromotionLevel Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddPromotionLevelCalled()` | Verifies `AddPromotionLevelAsync` was called once |
| `VerifyAddPromotionLevelNotCalled()` | Verifies `AddPromotionLevelAsync` was never called |

### Tag Setup Methods

| Method | Description |
|--------|-------------|
| `SetupGetTagBySlug(string slug, TagEntity? tag)` | Returns tag (or null) for given slug |
| `SetupGetAllTags(IReadOnlyList<TagEntity> list)` | Returns given list for any search term |

### Tag Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddTagCalled()` | Verifies `AddTagAsync` was called once |
| `VerifyAddTagNotCalled()` | Verifies `AddTagAsync` was never called |

---

## MockContentUnitOfWork

File: `tests/Unit/Common/Mocks/Infrastructure/MockContentUnitOfWork.cs`

Interface mocked: `IContentUnitOfWork`

### Factory

| Method | Description |
|--------|-------------|
| `MockContentUnitOfWork.Create()` | Creates a `Mock<IContentUnitOfWork>` with `CommitAsync` returning `1` |

### Setup Methods

| Method | Description |
|--------|-------------|
| `SetupCommit(int result = 1)` | Configures `CommitAsync` to return given result |
| `SetupCommitThrows(Exception exception)` | Configures `CommitAsync` to throw given exception |

### Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyCommitCalled()` | Verifies `CommitAsync` was called exactly once |
| `VerifyCommitNotCalled()` | Verifies `CommitAsync` was never called |
| `VerifyCommitCalled(int times)` | Verifies `CommitAsync` was called exactly `times` times |

---

## MockCategoryRepository

File: `tests/Unit/Common/Mocks/Repositories/MockCategoryRepository.cs`

Interface mocked: `ICategoryRepository`

### Factory

| Method | Description |
|--------|-------------|
| `MockCategoryRepository.Create()` | Creates mock with defaults |

### Setup Methods

| Method | Description |
|--------|-------------|
| `SetupCategoryExistsByName(string name, bool exists)` | Configures name existence check |
| `SetupCategoryExistsBySlug(string slug, bool exists)` | Configures slug existence check |
| `SetupGetCategoryByIdOrThrow(CategoryEntity entity)` | Returns entity for given ID |
| `SetupGetCategoryByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` |
| `SetupGetAllCategories(IReadOnlyList<CategoryEntity> list)` | Returns given list |

### Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddCategoryCalled()` | Verifies add was called once |
| `VerifyAddCategoryNotCalled()` | Verifies add was never called |

---

## MockCustomerRepository

File: `tests/Unit/Common/Mocks/Repositories/MockCustomerRepository.cs`

Interface mocked: `ICustomerRepository`

### Factory

| Method | Description |
|--------|-------------|
| `MockCustomerRepository.Create()` | Creates mock with defaults |

### Setup Methods

| Method | Description |
|--------|-------------|
| `SetupCustomerExistsByEmail(string email, bool exists)` | Configures email existence check |
| `SetupGetCustomerByIdOrThrow(CustomerEntity entity)` | Returns entity for given ID |
| `SetupGetCustomerByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` |
| `SetupGetAllCustomers(IReadOnlyList<CustomerEntity> list)` | Returns given list |

### Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddCustomerCalled()` | Verifies add was called once |
| `VerifyAddCustomerNotCalled()` | Verifies add was never called |

---

## MockPackageRepository

File: `tests/Unit/Common/Mocks/Repositories/MockPackageRepository.cs`

Interface mocked: `IPackageRepository`

### Factory

| Method | Description |
|--------|-------------|
| `MockPackageRepository.Create()` | Creates mock with defaults |

### Setup Methods

| Method | Description |
|--------|-------------|
| `SetupGetPackageByIdOrThrow(PackageEntity entity)` | Returns entity for given ID |
| `SetupGetPackageByIdOrThrowNotFound(Guid id)` | Throws `NotFoundException` |
| `SetupGetAllPackages(IReadOnlyList<PackageEntity> list)` | Returns given list |

### Verify Methods

| Method | Description |
|--------|-------------|
| `VerifyAddPackageCalled()` | Verifies add was called once |
| `VerifyAddPackageNotCalled()` | Verifies add was never called |

---

## BaseContentHandlerTest

File: `tests/Unit/Common/BaseContentHandlerTest.cs`

Base class for all Content module handler tests that need the Mapster mapper.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Mapper` | `IMapper` | Pre-configured Mapster mapper using `MappingRegistration.CreateConfiguration()` |

### Usage

```csharp
public class MyHandlerTests : BaseContentHandlerTest
{
    public MyHandlerTests()
    {
        _handler = new MyHandler(_repositoryMock.Object, _unitOfWorkMock.Object, Mapper);
    }
}
```

Validator tests do NOT extend `BaseContentHandlerTest` — they are standalone classes.
