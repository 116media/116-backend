# Day 5 — March 6, 2026 (44 commits)
## Catalog source completion + test fixtures + catalog meta/spec + identity roles V1 start

**Start time:** 08:30
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/GetCustomerByIdQuery.cs`
```
feat(content): add GetCustomerById query record
```

### 2
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetCustomerById/V1/GetCustomerByIdEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/customers/{id} endpoint
```

### 3
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/GetPackageByIdHandler.cs`
```
feat(content): add GetPackageById query handler with slot eager load:

- Fetch package by id using GetByIdOrThrowAsync with NotFoundException guard
- Include PackageSlots collection via eager loading for complete response
- Map result to PackageDto with nested PackageSlotDto list via Mapster
```

### 4
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/GetPackageByIdMetaField.cs`
```
feat(content): add GetPackageById route metadata
```

### 5
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/GetPackageByIdQuery.cs`
```
feat(content): add GetPackageById query record
```

### 6
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Admin/Queries/GetPackageById/V1/GetPackageByIdEndpointV1.cs`
```
feat(content): add GET /api/v1/admin/packages/{id} endpoint
```

### 7
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/GetPublicCategoriesHandler.cs`
```
feat(content): add GetPublicCategories query handler with active filter:

- Query only active categories using IsActive specification predicate
- Return paginated CategoryDto list with IsActive = true via Mapster
```

### 8
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/GetPublicCategoriesMetaField.cs`
```
feat(content): add GetPublicCategories route metadata
```

### 9
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/GetPublicCategoriesQuery.cs`
```
feat(content): add GetPublicCategories query record
```

### 10
**File:** `src/Modules/Content/Content/Application/Catalog/UseCases/Public/Queries/GetPublicCategories/V1/GetPublicCategoriesEndpointV1.cs`
```
feat(content): add GET /api/v1/public/categories endpoint
```

### 11
**File:** `tests/Fixtures/Builders/Entities/Content/CategoryBuilder.cs`
```
build(fixture): add CategoryBuilder for content catalog tests:

- Build CategoryEntity with configurable Name, Slug, and IsActive
- Default to active state with test-safe deterministic values
```

### 12
**File:** `tests/Fixtures/Builders/Entities/Content/CategoryPricingBuilder.cs`
```
build(fixture): add CategoryPricingBuilder for content catalog tests:

- Build CategoryPricingEntity with CategoryId, PricingTierId, and PriceUsd
- Support composite-key configuration for uniqueness constraint tests
```

### 13
**File:** `tests/Fixtures/Builders/Entities/Content/ContentTypeBuilder.cs`
```
build(fixture): add ContentTypeBuilder for content lookup tests:

- Build ContentTypeEntity with configurable Name and IsActive
- Default to active state for standard lookup test scenarios
```

### 14
**File:** `tests/Fixtures/Builders/Entities/Content/CustomerBuilder.cs`
```
build(fixture): add CustomerBuilder for content catalog tests:

- Build CustomerEntity with configurable Name, Email, and Phone
- Support email uniqueness test cases via overridable email field
```

### 15
**File:** `tests/Fixtures/Builders/Entities/Content/PackageBuilder.cs`
```
build(fixture): add PackageBuilder for content catalog tests:

- Build PackageEntity with Name, Description, and IsActive
- Include PackageSlots collection for slot-relationship handler tests
```

### 16
**File:** `tests/Fixtures/Builders/Entities/Content/PackageSlotBuilder.cs`
```
build(fixture): add PackageSlotBuilder for content catalog tests:

- Build PackageSlotEntity with configurable SlotName, SlotType, and PackageId
- Support composite-key configuration for duplicate-slot guard tests
```

### 17
**File:** `tests/Fixtures/Builders/Entities/Content/PricingTierBuilder.cs`
```
build(fixture): add PricingTierBuilder for content lookup tests:

- Build PricingTierEntity with configurable Name and IsActive
- Default to active state for standard pricing handler tests
```

### 18
**File:** `tests/Fixtures/Builders/Entities/Content/PromotionLevelBuilder.cs`
```
build(fixture): add PromotionLevelBuilder for content lookup tests:

- Build PromotionLevelEntity with configurable Name and IsActive
- Default to active state for promotion handler and filter tests
```

### 19
**File:** `tests/Fixtures/Builders/Entities/Content/TagBuilder.cs`
```
build(fixture): add TagBuilder for content lookup tests:

- Build TagEntity with configurable Slug and IsActive
- Support case-insensitive slug matching tests via configurable Slug field
```

### 20
**File:** `tests/Fixtures/Factories/Content/CategoryFactory.cs`
```
build(fixture): add CategoryFactory for catalog test data creation:

- Create CategoryEntity instances with seeded deterministic ids
- Expose single-entity and collection creation helpers
- Seed IsActive and Slug fields for handler and spec tests
```

### 21
**File:** `tests/Fixtures/Factories/Content/CategoryPricingFactory.cs`
```
build(fixture): add CategoryPricingFactory for catalog test data creation:

- Create CategoryPricingEntity instances with seeded composite keys
- Expose PriceUsd and PricingTierId fields for UpdateCategoryPricing tests
```

### 22
**File:** `tests/Fixtures/Factories/Content/ContentTypeFactory.cs`
```
build(fixture): add ContentTypeFactory for lookup test data creation:

- Create ContentTypeEntity instances with seeded ids and names
- Expose IsActive field to cover both active and inactive test branches
```

### 23
**File:** `tests/Fixtures/Factories/Content/CustomerFactory.cs`
```
build(fixture): add CustomerFactory for catalog test data creation:

- Create CustomerEntity instances with seeded name and email values
- Expose email field to test ILike duplicate-guard scenarios
```

### 24
**File:** `tests/Fixtures/Factories/Content/PackageFactory.cs`
```
build(fixture): add PackageFactory for catalog test data creation:

- Create PackageEntity instances with name, description, and IsActive
- Include pre-seeded PackageSlots list for slot-relationship tests
```

### 25
**File:** `tests/Fixtures/Factories/Content/PackageSlotFactory.cs`
```
build(fixture): add PackageSlotFactory for catalog test data creation:

- Create PackageSlotEntity instances with seeded SlotName and SlotType
- Expose PackageId field for slot-to-package relationship handler tests
```

### 26
**File:** `tests/Fixtures/Factories/Content/PricingTierFactory.cs`
```
build(fixture): add PricingTierFactory for lookup test data creation:

- Create PricingTierEntity instances with seeded ids and names
- Expose IsActive field for activate/deactivate handler test scenarios
```

### 27
**File:** `tests/Fixtures/Factories/Content/PromotionLevelFactory.cs`
```
build(fixture): add PromotionLevelFactory for lookup test data creation:

- Create PromotionLevelEntity instances with seeded ids and names
- Expose IsActive field for active-filter query handler tests
```

### 28
**File:** `tests/Fixtures/Factories/Content/TagFactory.cs`
```
build(fixture): add TagFactory for lookup test data creation:

- Create TagEntity instances with seeded Slug and IsActive values
- Support case-insensitive duplicate-slug test cases
```

### 29
**File:** `tests/Unit/Common/BaseContentHandlerTest.cs`
```
test(content): add BaseContentHandlerTest base class for content unit tests:

- Initialize shared MockContentUnitOfWork and MockLookupRepository
- Wire MockCategoryRepository, MockCustomerRepository, MockPackageRepository
- Provide consistent test setup across all catalog handler test classes
```

### 30
**File:** `tests/Unit/Common/Mocks/Infrastructure/MockContentUnitOfWork.cs`
```
test(content): add MockContentUnitOfWork for content handler tests:

- NSubstitute-based IContentUnitOfWork stub
- SaveChangesAsync returns completed Task by default
- Expose substitute for assertion verification in handler tests
```

### 31
**File:** `tests/Unit/Common/Mocks/Repositories/MockCategoryRepository.cs`
```
test(content): add MockCategoryRepository for catalog handler tests:

- NSubstitute stubs for AddAsync and GetByIdOrThrowAsync
- ExistsByNameAsync mock supports duplicate-guard scenario configuration
- GetAllAsync stub returns configurable PagedResponse for query tests
```

### 32
**File:** `tests/Unit/Common/Mocks/Repositories/MockCustomerRepository.cs`
```
test(content): add MockCustomerRepository for catalog handler tests:

- NSubstitute stubs for AddAsync and GetByIdOrThrowAsync
- ExistsByEmailAsync mock supports ILike duplicate-guard test scenarios
- GetAllAsync stub returns configurable PagedResponse for query tests
```

### 33
**File:** `tests/Unit/Common/Mocks/Repositories/MockLookupRepository.cs`
```
test(content): add MockLookupRepository for lookup handler tests:

- NSubstitute stubs for all ILookupRepository methods
- GetTagByIdOrThrowAsync mock supports not-found scenario configuration
- GetActiveContentTypesAsync stub for seeder and query handler tests
```

### 34
**File:** `tests/Unit/Common/Mocks/Repositories/MockPackageRepository.cs`
```
test(content): add MockPackageRepository for catalog handler tests:

- NSubstitute stubs for AddAsync, GetByIdOrThrowAsync, and ExistsByNameAsync
- GetAllAsync stub returns configurable PagedResponse for query tests
- Slot eager-load behavior configurable via GetByIdOrThrowAsync return value
```

### 35
**File:** `tests/Unit/Modules/Content/Application/Catalog/MetaFields/CatalogMetaFieldTests.cs`
```
test(content): add CatalogMetaField tests for endpoint metadata coverage:

- Assert each catalog use case MetaField returns the expected EndpointName
- Verify naming conventions are consistent across commands and queries
```

### 36
**File:** `tests/Unit/Modules/Content/Application/Catalog/Specifications/CatalogSpecificationsTests.cs`
```
test(content): add CatalogSpecifications tests for predicate correctness:

- Verify CategoryByNameSpecification matches case-insensitive ILike patterns
- Verify IsActiveSpecification filters inactive categories and packages correctly
- Verify CustomerByEmailSpecification and PackageByNameSpecification behavior
```

### 37
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/V1/PublicGetOwnRolesEndpointV1Tests.cs`
```
test(identity): add PublicGetOwnRoles endpoint v1 tests
```

### 38
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/ActivatePermission/V1/AdminActivatePermissionEndpointV1Tests.cs`
```
test(identity): add AdminActivatePermission endpoint v1 tests
```

### 39
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/ActivateRole/V1/AdminActivateRoleEndpointV1Tests.cs`
```
test(identity): add AdminActivateRole endpoint v1 tests
```

### 40
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/AssignPermissionToRole/V1/AdminAssignPermissionToRoleEndpointV1Tests.cs`
```
test(identity): add AdminAssignPermissionToRole endpoint v1 tests
```

### 41
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/BulkUpdateRolePermissions/V1/AdminBulkUpdateRolePermissionsEndpointV1Tests.cs`
```
test(identity): add AdminBulkUpdateRolePermissions endpoint v1 tests
```

### 42
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/CreatePermission/V1/AdminCreatePermissionEndpointV1Tests.cs`
```
test(identity): add AdminCreatePermission endpoint v1 tests
```

### 43
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/CreateRole/V1/AdminCreateRoleEndpointV1Tests.cs`
```
test(identity): add AdminCreateRole endpoint v1 tests
```

### 44
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/DeactivatePermission/V1/AdminDeactivatePermissionEndpointV1Tests.cs`
```
test(identity): add AdminDeactivatePermission endpoint v1 tests
```
