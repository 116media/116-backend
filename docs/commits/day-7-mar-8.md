# Day 7 — March 8, 2026 (45 commits)
## Catalog query tests + lookup meta/spec + lookup command tests start + identity roles/session V1

**Start time:** 08:45
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/GetAllCategoriesHandlerTests.cs`
```
test(content): add GetAllCategories handler tests with pagination and filter assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert IsActive filter narrows results to active categories only
- Assert name search applies case-insensitive ILike matching
```

### 2
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCategories/V1/GetAllCategoriesEndpointV1Tests.cs`
```
test(content): add GetAllCategories endpoint v1 tests
```

### 3
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/GetAllCustomersHandlerTests.cs`
```
test(content): add GetAllCustomers handler tests with pagination assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert name and email search parameters are forwarded to the repository
```

### 4
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllCustomers/V1/GetAllCustomersEndpointV1Tests.cs`
```
test(content): add GetAllCustomers endpoint v1 tests
```

### 5
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/GetAllPackagesHandlerTests.cs`
```
test(content): add GetAllPackages handler tests with IsActive filter assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert IsActive filter when supplied limits results to active packages only
```

### 6
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetAllPackages/V1/GetAllPackagesEndpointV1Tests.cs`
```
test(content): add GetAllPackages endpoint v1 tests
```

### 7
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/GetCategoryByIdHandlerTests.cs`
```
test(content): add GetCategoryById handler tests with pricing eager load assertion:

- Assert NotFoundException is thrown when category id does not exist
- Verify CategoryPricings collection is included in the returned CategoryDto
```

### 8
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetCategoryById/V1/GetCategoryByIdEndpointV1Tests.cs`
```
test(content): add GetCategoryById endpoint v1 tests
```

### 9
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/GetCustomerByIdHandlerTests.cs`
```
test(content): add GetCustomerById handler tests for success and not-found paths:

- Assert NotFoundException is thrown when customer id does not exist
- Verify returned CustomerDto maps Name, Email, and Phone correctly
```

### 10
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/V1/GetCustomerByIdEndpointV1Tests.cs`
```
test(content): add GetCustomerById endpoint v1 tests
```

### 11
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/GetPackageByIdHandlerTests.cs`
```
test(content): add GetPackageById handler tests with slot load assertions:

- Assert NotFoundException is thrown when package id does not exist
- Verify PackageSlots collection is included in the returned PackageDto
```

### 12
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/V1/GetPackageByIdEndpointV1Tests.cs`
```
test(content): add GetPackageById endpoint v1 tests
```

### 13
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/GetPublicCategoriesHandlerTests.cs`
```
test(content): add GetPublicCategories handler tests with active-filter assertions:

- Verify only active categories are returned via IsActive specification
- Assert PagedResponse total count reflects active-only filtered results
```

### 14
**File:** `tests/Unit/Modules/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/V1/GetPublicCategoriesEndpointV1Tests.cs`
```
test(content): add GetPublicCategories endpoint v1 tests
```

### 15
**File:** `tests/Unit/Modules/Content/Application/Lookup/MetaFields/LookupMetaFieldTests.cs`
```
test(content): add LookupMetaField tests for endpoint metadata coverage:

- Assert each lookup use case MetaField returns the expected EndpointName
- Verify naming conventions are consistent across commands and queries
```

### 16
**File:** `tests/Unit/Modules/Content/Application/Lookup/Specifications/LookupSpecificationsTests.cs`
```
test(content): add LookupSpecifications tests for predicate correctness:

- Verify ContentTypeByNameSpecification uses ILike case-insensitive matching
- Verify PricingTierByNameSpecification and TagBySlugSpecification behavior
- Verify IsActiveSpecification filters inactive lookup entities correctly
```

### 17
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivateContentType/ActivateContentTypeHandlerTests.cs`
```
test(content): add ActivateContentType handler tests for success and conflict paths:

- Assert NotFoundException when content type id does not exist
- Assert ConflictException when content type is already active
- Verify Activate() is called and SaveChangesAsync is invoked on success
```

### 18
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivateContentType/ActivateContentTypeValidatorTests.cs`
```
test(content): add ActivateContentType validator tests:

- Assert validation fails when ContentTypeId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 19
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivateContentType/V1/ActivateContentTypeEndpointV1Tests.cs`
```
test(content): add ActivateContentType endpoint v1 tests
```

### 20
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePricingTier/ActivatePricingTierHandlerTests.cs`
```
test(content): add ActivatePricingTier handler tests for success and conflict paths:

- Assert NotFoundException when pricing tier id does not exist
- Assert ConflictException when pricing tier is already active
- Verify Activate() is called and SaveChangesAsync is invoked on success
```

### 21
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePricingTier/ActivatePricingTierValidatorTests.cs`
```
test(content): add ActivatePricingTier validator tests:

- Assert validation fails when PricingTierId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 22
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePricingTier/V1/ActivatePricingTierEndpointV1Tests.cs`
```
test(content): add ActivatePricingTier endpoint v1 tests
```

### 23
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/ActivatePromotionLevelHandlerTests.cs`
```
test(content): add ActivatePromotionLevel handler tests for success and conflict paths:

- Assert NotFoundException when promotion level id does not exist
- Assert ConflictException when promotion level is already active
- Verify Activate() is called and SaveChangesAsync is invoked on success
```

### 24
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/ActivatePromotionLevelValidatorTests.cs`
```
test(content): add ActivatePromotionLevel validator tests:

- Assert validation fails when PromotionLevelId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 25
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/V1/ActivatePromotionLevelEndpointV1Tests.cs`
```
test(content): add ActivatePromotionLevel endpoint v1 tests
```

### 26
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateContentType/CreateContentTypeHandlerTests.cs`
```
test(content): add CreateContentType handler tests for duplicate and success paths:

- Assert ConflictException when name already exists via ILike check
- Verify ContentTypeEntity.Create is called with validated name
- Assert returned ContentTypeDto maps Id, Name, and IsActive correctly
```

### 27
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateContentType/CreateContentTypeValidatorTests.cs`
```
test(content): add CreateContentType validator tests:

- Assert Name is required and within max length
- Assert validation passes with a valid non-empty name string
```

### 28
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateContentType/V1/CreateContentTypeEndpointV1Tests.cs`
```
test(content): add CreateContentType endpoint v1 tests
```

### 29
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePricingTier/CreatePricingTierHandlerTests.cs`
```
test(content): add CreatePricingTier handler tests for duplicate and success paths:

- Assert ConflictException when name already exists via ILike check
- Verify PricingTierEntity.Create is called with validated name
- Assert returned PricingTierDto maps Id, Name, and IsActive correctly
```

### 30
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePricingTier/CreatePricingTierValidatorTests.cs`
```
test(content): add CreatePricingTier validator tests:

- Assert Name is required and within max length
- Assert validation passes with a valid non-empty name string
```

### 31
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePricingTier/V1/CreatePricingTierEndpointV1Tests.cs`
```
test(content): add CreatePricingTier endpoint v1 tests
```

### 32
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePromotionLevel/CreatePromotionLevelHandlerTests.cs`
```
test(content): add CreatePromotionLevel handler tests for duplicate and success paths:

- Assert ConflictException when name already exists via ILike check
- Verify PromotionLevelEntity.Create is called with validated name
- Assert returned PromotionLevelDto maps Id, Name, and IsActive correctly
```

### 33
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePromotionLevel/CreatePromotionLevelValidatorTests.cs`
```
test(content): add CreatePromotionLevel validator tests:

- Assert Name is required and within max length
- Assert validation passes with a valid name string
```

### 34
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreatePromotionLevel/V1/CreatePromotionLevelEndpointV1Tests.cs`
```
test(content): add CreatePromotionLevel endpoint v1 tests
```

### 35
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateTag/CreateTagHandlerTests.cs`
```
test(content): add CreateTag handler tests for duplicate and success paths:

- Assert ConflictException when slug already exists via ILike case-insensitive check
- Verify TagEntity.Create is called with validated slug value
- Assert returned TagDto maps Id, Slug, and IsActive correctly
```

### 36
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateTag/CreateTagValidatorTests.cs`
```
test(content): add CreateTag validator tests:

- Assert Slug is required and within max length
- Assert validation passes with a valid non-empty slug string
```

### 37
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/CreateTag/V1/CreateTagEndpointV1Tests.cs`
```
test(content): add CreateTag endpoint v1 tests
```

### 38
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/UpdatePermission/V1/AdminUpdatePermissionEndpointV1Tests.cs`
```
test(identity): add AdminUpdatePermission endpoint v1 tests
```

### 39
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/UpdateRole/V1/AdminUpdateRoleEndpointV1Tests.cs`
```
test(identity): add AdminUpdateRole endpoint v1 tests
```

### 40
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetAllPermissions/V1/AdminGetAllPermissionsEndpointV1Tests.cs`
```
test(identity): add AdminGetAllPermissions endpoint v1 tests
```

### 41
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetAllRoles/V1/AdminGetAllRolesEndpointV1Tests.cs`
```
test(identity): add AdminGetAllRoles endpoint v1 tests
```

### 42
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetPermissionById/V1/AdminGetPermissionByIdEndpointV1Tests.cs`
```
test(identity): add AdminGetPermissionById endpoint v1 tests
```

### 43
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetRoleById/V1/AdminGetRoleByIdEndpointV1Tests.cs`
```
test(identity): add AdminGetRoleById endpoint v1 tests
```

### 44
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Commands/CleanupExpiredSessions/V1/AdminCleanupExpiredSessionsEndpointV1Tests.cs`
```
test(identity): add AdminCleanupExpiredSessions endpoint v1 tests
```

### 45
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Commands/ForceLogoutUser/V1/AdminForceLogoutUserEndpointV1Tests.cs`
```
test(identity): add AdminForceLogoutUser endpoint v1 tests
```
