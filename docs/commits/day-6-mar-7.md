# Day 6 — March 7, 2026 (48 commits)
## Catalog command tests + identity roles admin commands V1

**Start time:** 08:20
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivateCategory/ActivateCategoryHandlerTests.cs`
```
test(content): add ActivateCategory handler tests for success and conflict paths:

- Assert NotFoundException is thrown when category id does not exist
- Assert ConflictException is thrown when category is already active
- Verify Activate() is called and SaveChangesAsync is invoked on success
```

### 2
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivateCategory/ActivateCategoryValidatorTests.cs`
```
test(content): add ActivateCategory validator tests:

- Assert validation fails when CategoryId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 3
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivateCategory/V1/ActivateCategoryEndpointV1Tests.cs`
```
test(content): add ActivateCategory endpoint v1 tests
```

### 4
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivatePackage/ActivatePackageHandlerTests.cs`
```
test(content): add ActivatePackage handler tests for success and conflict paths:

- Assert NotFoundException is thrown when package id does not exist
- Assert ConflictException is thrown when package is already active
- Verify Activate() is called and SaveChangesAsync is invoked on success
```

### 5
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivatePackage/ActivatePackageValidatorTests.cs`
```
test(content): add ActivatePackage validator tests:

- Assert validation fails when PackageId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 6
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/ActivatePackage/V1/ActivatePackageEndpointV1Tests.cs`
```
test(content): add ActivatePackage endpoint v1 tests
```

### 7
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddCategoryPricing/AddCategoryPricingHandlerTests.cs`
```
test(content): add AddCategoryPricing handler tests for duplicate and success paths:

- Assert NotFoundException when category or pricing tier does not exist
- Assert ConflictException when the category-tier combination already exists
- Verify CategoryPricingEntity is created and persisted via unit of work
```

### 8
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddCategoryPricing/AddCategoryPricingValidatorTests.cs`
```
test(content): add AddCategoryPricing validator tests:

- Assert validation fails on empty CategoryId or PricingTierId
- Assert PriceUsd must be greater than zero
```

### 9
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddCategoryPricing/V1/AddCategoryPricingEndpointV1Tests.cs`
```
test(content): add AddCategoryPricing endpoint v1 tests
```

### 10
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddPackageSlot/AddPackageSlotHandlerTests.cs`
```
test(content): add AddPackageSlot handler tests for duplicate and success paths:

- Assert NotFoundException when package id does not exist
- Assert ConflictException when slot name already exists for that package
- Verify PackageSlotEntity.Create is called and persisted via unit of work
```

### 11
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddPackageSlot/AddPackageSlotValidatorTests.cs`
```
test(content): add AddPackageSlot validator tests:

- Assert PackageId must be a non-empty Guid
- Assert SlotName and SlotType must be non-empty and within max length
```

### 12
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/AddPackageSlot/V1/AddPackageSlotEndpointV1Tests.cs`
```
test(content): add AddPackageSlot endpoint v1 tests
```

### 13
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/CreateCategoryHandlerTests.cs`
```
test(content): add CreateCategory handler tests for duplicate and success paths:

- Assert ConflictException when name already exists via ILike case-insensitive check
- Verify CategoryEntity.Create is called with correct name and derived slug
- Assert returned CategoryDto maps Name, Slug, and IsActive correctly
```

### 14
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/CreateCategoryValidatorTests.cs`
```
test(content): add CreateCategory validator tests:

- Assert Name is required and within max length
- Assert validation passes with a valid non-empty name string
```

### 15
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCategory/V1/CreateCategoryEndpointV1Tests.cs`
```
test(content): add CreateCategory endpoint v1 tests
```

### 16
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCustomer/CreateCustomerHandlerTests.cs`
```
test(content): add CreateCustomer handler tests for duplicate and success paths:

- Assert ConflictException when email already exists via ILike case-insensitive check
- Verify CustomerEntity.Create is called with name, email, and phone values
- Assert returned CustomerDto maps all fields correctly via Mapster
```

### 17
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCustomer/CreateCustomerValidatorTests.cs`
```
test(content): add CreateCustomer validator tests:

- Assert Name and Email are required and within max length limits
- Assert Email must conform to valid email format
- Assert Phone is optional but constrained to max length
```

### 18
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreateCustomer/V1/CreateCustomerEndpointV1Tests.cs`
```
test(content): add CreateCustomer endpoint v1 tests
```

### 19
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageHandlerTests.cs`
```
test(content): add CreatePackage handler tests for duplicate and success paths:

- Assert ConflictException when package name already exists via ILike case-insensitive check
- Verify PackageEntity.Create is called with correct name and description
- Assert returned PackageDto maps Name, Description, and IsActive correctly
```

### 20
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageValidatorTests.cs`
```
test(content): add CreatePackage validator tests:

- Assert Name is required and within max length
- Assert Description is optional but constrained to max length
```

### 21
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/CreatePackage/V1/CreatePackageEndpointV1Tests.cs`
```
test(content): add CreatePackage endpoint v1 tests
```

### 22
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryHandlerTests.cs`
```
test(content): add DeactivateCategory handler tests for success and conflict paths:

- Assert NotFoundException when category id does not exist
- Assert ConflictException when category is already inactive
- Verify Deactivate() is called and SaveChangesAsync is invoked on success
```

### 23
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryValidatorTests.cs`
```
test(content): add DeactivateCategory validator tests:

- Assert validation fails when CategoryId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 24
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivateCategory/V1/DeactivateCategoryEndpointV1Tests.cs`
```
test(content): add DeactivateCategory endpoint v1 tests
```

### 25
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageHandlerTests.cs`
```
test(content): add DeactivatePackage handler tests for success and conflict paths:

- Assert NotFoundException when package id does not exist
- Assert ConflictException when package is already inactive
- Verify Deactivate() is called and SaveChangesAsync is invoked on success
```

### 26
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageValidatorTests.cs`
```
test(content): add DeactivatePackage validator tests:

- Assert validation fails when PackageId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 27
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/DeactivatePackage/V1/DeactivatePackageEndpointV1Tests.cs`
```
test(content): add DeactivatePackage endpoint v1 tests
```

### 28
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/RemoveCategoryPricingHandlerTests.cs`
```
test(content): add RemoveCategoryPricing handler tests for success and not-found paths:

- Assert NotFoundException when category-pricing combination does not exist
- Verify entity is removed from context and SaveChangesAsync is invoked
```

### 29
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/V1/RemoveCategoryPricingEndpointV1Tests.cs`
```
test(content): add RemoveCategoryPricing endpoint v1 tests
```

### 30
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/RemovePackageSlotHandlerTests.cs`
```
test(content): add RemovePackageSlot handler tests for success and not-found paths:

- Assert NotFoundException when package-slot combination does not exist
- Verify entity is removed from context and SaveChangesAsync is invoked
```

### 31
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/RemovePackageSlot/V1/RemovePackageSlotEndpointV1Tests.cs`
```
test(content): add RemovePackageSlot endpoint v1 tests
```

### 32
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryHandlerTests.cs`
```
test(content): add UpdateCategory handler tests for success and duplicate-name paths:

- Assert NotFoundException when category id does not exist
- Assert ConflictException when new name conflicts with another category via ILike
- Verify entity.Update is called and unit of work SaveChangesAsync is committed
```

### 33
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryValidatorTests.cs`
```
test(content): add UpdateCategory validator tests:

- Assert CategoryId must be a non-empty Guid
- Assert Name is required and within max length limit
```

### 34
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategory/V1/UpdateCategoryEndpointV1Tests.cs`
```
test(content): add UpdateCategory endpoint v1 tests
```

### 35
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingHandlerTests.cs`
```
test(content): add UpdateCategoryPricing handler tests for success and not-found paths:

- Assert NotFoundException for missing category-pricing or replacement pricing tier
- Verify entity.Update is called with new PricingTierId and persisted via unit of work
```

### 36
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingValidatorTests.cs`
```
test(content): add UpdateCategoryPricing validator tests:

- Assert CategoryId, PricingTierId, and NewPricingTierId must be non-empty Guids
- Assert PriceUsd must be greater than zero when provided
```

### 37
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/V1/UpdateCategoryPricingEndpointV1Tests.cs`
```
test(content): add UpdateCategoryPricing endpoint v1 tests
```

### 38
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerHandlerTests.cs`
```
test(content): add UpdateCustomer handler tests for success and duplicate-email paths:

- Assert NotFoundException when customer id does not exist
- Assert ConflictException when new email already belongs to another customer
- Verify entity.Update is called with new name, email, phone and unit of work commits
```

### 39
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerValidatorTests.cs`
```
test(content): add UpdateCustomer validator tests:

- Assert CustomerId must be a non-empty Guid
- Assert Name and Email are within max length and Email conforms to valid format
- Assert Phone is optional but constrained to max length
```

### 40
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Commands/UpdateCustomer/V1/UpdateCustomerEndpointV1Tests.cs`
```
test(content): add UpdateCustomer endpoint v1 tests
```

### 41
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/DeactivateRole/V1/AdminDeactivateRoleEndpointV1Tests.cs`
```
test(identity): add AdminDeactivateRole endpoint v1 tests
```

### 42
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/HardDeletePermission/V1/AdminHardDeletePermissionEndpointV1Tests.cs`
```
test(identity): add AdminHardDeletePermission endpoint v1 tests
```

### 43
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/HardDeleteRole/V1/AdminHardDeleteRoleEndpointV1Tests.cs`
```
test(identity): add AdminHardDeleteRole endpoint v1 tests
```

### 44
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/RemovePermissionFromRole/V1/AdminRemovePermissionFromRoleEndpointV1Tests.cs`
```
test(identity): add AdminRemovePermissionFromRole endpoint v1 tests
```

### 45
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/RestorePermission/V1/AdminRestorePermissionEndpointV1Tests.cs`
```
test(identity): add AdminRestorePermission endpoint v1 tests
```

### 46
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/RestoreRole/V1/AdminRestoreRoleEndpointV1Tests.cs`
```
test(identity): add AdminRestoreRole endpoint v1 tests
```

### 47
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeletePermission/V1/AdminSoftDeletePermissionEndpointV1Tests.cs`
```
test(identity): add AdminSoftDeletePermission endpoint v1 tests
```

### 48
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeleteRole/V1/AdminSoftDeleteRoleEndpointV1Tests.cs`
```
test(identity): add AdminSoftDeleteRole endpoint v1 tests
```
