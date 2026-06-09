# Day 2 — March 3, 2026 (57 commits)
## Remaining Identity tests + Content source changes

**Start time:** 08:15
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/PermissionRepositoryTests.cs`
```
test(identity): add PermissionRepository tests for ILike name search
```

### 2
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/RolePermissionRepositoryTests.cs`
```
test(identity): add RolePermissionRepository tests for bulk-update paths
```

### 3
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/RoleRepositoryTests.cs`
```
test(identity): expand RoleRepository tests with soft-delete and restore
```

### 4
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/SessionRepositoryTests.cs`
```
test(identity): add SessionRepository tests for expiry and revoke flows
```

### 5
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/UserRoleRepositoryTests.cs`
```
test(identity): add UserRoleRepository tests for assignment and removal
```

### 6
**File:** `tests/Unit/_116.Unit.Tests.csproj`
```
build(test): add content module project reference to unit test project
```

### 7
**File:** `src/Modules/Content/Content/Application/Lookup/Specifications/ContentTypeSpecifications.cs`
```
refactor(content): use ILike for case-insensitive name matching in ContentType spec:

- Replace == comparison with EF.Functions.ILike
- Ensures case-insensitive uniqueness checks on PostgreSQL
```

### 8
**File:** `src/Modules/Content/Content/Application/Lookup/Specifications/PricingTierSpecifications.cs`
```
refactor(content): use ILike for case-insensitive name matching in PricingTier spec:

- Replace == comparison with EF.Functions.ILike
- Ensures case-insensitive uniqueness checks on PostgreSQL
```

### 9
**File:** `src/Modules/Content/Content/Application/Lookup/Specifications/PromotionLevelSpecifications.cs`
```
refactor(content): use ILike for case-insensitive name matching in PromotionLevel spec:

- Replace == comparison with EF.Functions.ILike
- Ensures case-insensitive uniqueness checks on PostgreSQL
```

### 10
**File:** `src/Modules/Content/Content/Application/Lookup/Specifications/TagSpecifications.cs`
```
refactor(content): use ILike for case-insensitive slug matching in Tag spec:

- Replace == comparison with EF.Functions.ILike
- Ensures case-insensitive uniqueness checks on PostgreSQL
```

### 11
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/ActivateContentType/V1/ActivateContentTypeEndpointV1.cs`
```
refactor(content): update activate-content-type endpoint versioning convention
```

### 12
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePricingTier/V1/ActivatePricingTierEndpointV1.cs`
```
refactor(content): update activate-pricing-tier endpoint versioning convention
```

### 13
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/V1/ActivatePromotionLevelEndpointV1.cs`
```
refactor(content): update activate-promotion-level endpoint versioning convention
```

### 14
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/CreateContentType/V1/CreateContentTypeEndpointV1.cs`
```
refactor(content): update create-content-type endpoint versioning convention
```

### 15
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/CreatePricingTier/V1/CreatePricingTierEndpointV1.cs`
```
refactor(content): update create-pricing-tier endpoint versioning convention
```

### 16
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/CreatePromotionLevel/V1/CreatePromotionLevelEndpointV1.cs`
```
refactor(content): update create-promotion-level endpoint versioning convention
```

### 17
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/CreateTag/V1/CreateTagEndpointV1.cs`
```
refactor(content): update create-tag endpoint versioning convention
```

### 18
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/DeactivateContentType/V1/DeactivateContentTypeEndpointV1.cs`
```
refactor(content): update deactivate-content-type endpoint versioning convention
```

### 19
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePricingTier/V1/DeactivatePricingTierEndpointV1.cs`
```
refactor(content): update deactivate-pricing-tier endpoint versioning convention
```

### 20
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/V1/DeactivatePromotionLevelEndpointV1.cs`
```
refactor(content): update deactivate-promotion-level endpoint versioning convention
```

### 21
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePricingTier/V1/UpdatePricingTierEndpointV1.cs`
```
refactor(content): update update-pricing-tier endpoint versioning convention
```

### 22
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/V1/UpdatePromotionLevelEndpointV1.cs`
```
refactor(content): update update-promotion-level endpoint versioning convention
```

### 23
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Queries/GetAllContentTypes/V1/GetAllContentTypesEndpointV1.cs`
```
refactor(content): update get-all-content-types endpoint versioning convention
```

### 24
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPricingTiers/V1/GetAllPricingTiersEndpointV1.cs`
```
refactor(content): update get-all-pricing-tiers endpoint versioning convention
```

### 25
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPromotionLevels/V1/GetAllPromotionLevelsEndpointV1.cs`
```
refactor(content): update get-all-promotion-levels endpoint versioning convention
```

### 26
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Public/Queries/GetActivePromotionLevels/V1/GetActivePromotionLevelsEndpointV1.cs`
```
refactor(content): update get-active-promotion-levels endpoint versioning convention
```

### 27
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/GetAllTagsHandler.cs`
```
refactor(content): move GetAllTags handler to Public/Queries subfolder
```

### 28
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/GetAllTagsMetaField.cs`
```
refactor(content): move GetAllTags meta field to Public/Queries subfolder
```

### 29
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/GetAllTagsQuery.cs`
```
refactor(content): move GetAllTags query to Public/Queries subfolder
```

### 30
**File:** `src/Modules/Content/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/V1/GetAllTagsEndpointV1.cs`
```
refactor(content): move GetAllTags endpoint to Public/Queries/V1 subfolder
```

### 31
**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/PricingTierErrorMessage.cs`
```
refactor(content): add NotFoundById message to PricingTierErrorMessage
```

### 32
**File:** `src/Modules/Content/Content/Application/Shared/Errors/PricingTierErrors.cs`
```
refactor(content): add NotFoundById factory method to PricingTierErrors
```

### 33
**File:** `src/Modules/Content/Content/Application/Shared/Mappers/MappingRegistration.cs`
```
refactor(content): register catalog entity mappers in MappingRegistration:

- Add Mapster config for Category, CategoryPricing, Customer entities
- Add Mapster config for Package and PackageSlot entities
```

### 34
**File:** `src/Modules/Content/Content/Application/Shared/Repositories/ILookupRepository.cs`
```
refactor(content): extend ILookupRepository with catalog-support methods:

- Add GetActiveContentTypesAsync for public content-type queries
- Add GetTagByIdOrThrowAsync for catalog use cases
```

### 35
**File:** `src/Modules/Content/Content/ContentModule.cs`
```
refactor(content): register catalog repositories and fix Testing env migrations:

- Add ICategoryRepository, ICustomerRepository, IPackageRepository scoped registrations
- Align EnableMigrations with EnableSeeding flag to bypass MigrateAsync on InMemory DB
```

### 36
**File:** `src/Modules/Content/Content/Domain/Constants/ContentConstants.cs`
```
feat(content): add catalog max-length constants to ContentConstants:

- Add CategoryNameMaxLength, CustomerNameMaxLength, PackageNameMaxLength
- Add SlotNameMaxLength and related catalog field length constants
```

### 37
**File:** `src/Modules/Content/Content/Domain/Entities/ContentTypeEntity.cs`
```
feat(content): add Update method to ContentTypeEntity:

- Add Update(string name) factory method with name validation guard
- Reuses same validation logic as Create for consistency
```

### 38
**File:** `src/Modules/Content/Content/Infrastructure/Persistence/ContentDbContext.cs`
```
feat(content): add catalog DbSets to ContentDbContext:

- Register Categories, CategoryPricings, Customers, Packages, PackageSlots
- Apply EF configurations from assembly for all catalog entities
```

### 39
**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Migrations/ContentDbContextModelSnapshot.cs`
```
feat(content): update model snapshot for catalog schema entities:

- Regenerate EF Core snapshot to include Category and CategoryPricing
- Include Customer, Package, and PackageSlot entity mappings
```

### 40
**File:** `src/Modules/Content/Content/Infrastructure/Persistence/Seeds/ContentTypes/ContentTypeSeeder.cs`
```
refactor(content): update ContentTypeSeeder to use GetActiveContentTypesAsync
```

### 41
**File:** `src/Modules/Content/Content/Infrastructure/Repositories/LookupRepository.cs`
```
refactor(content): add GetActiveContentTypesAsync and GetTagByIdOrThrowAsync:

- Implement GetActiveContentTypesAsync using specification and ILike filter
- Implement GetTagByIdOrThrowAsync for catalog use cases
```

### 42
**File:** `src/Modules/Content/Content/Application/Shared/DTOs/CategoryDto.cs`
```
feat(content): add CategoryDto record for catalog responses
```

### 43
**File:** `src/Modules/Content/Content/Application/Shared/DTOs/CategoryPricingDto.cs`
```
feat(content): add CategoryPricingDto record for catalog responses
```

### 44
**File:** `src/Modules/Content/Content/Application/Shared/DTOs/CustomerDto.cs`
```
feat(content): add CustomerDto record for catalog responses
```

### 45
**File:** `src/Modules/Content/Content/Application/Shared/DTOs/PackageDto.cs`
```
feat(content): add PackageDto record for catalog responses
```

### 46
**File:** `src/Modules/Content/Content/Application/Shared/DTOs/PackageSlotDto.cs`
```
feat(content): add PackageSlotDto record for catalog responses
```

### 47
**File:** `src/Modules/Content/Content/Application/Shared/Errors/CategoryErrors.cs`
```
feat(content): add CategoryErrors factory class with AlreadyExists and NotFound
```

### 48
**File:** `src/Modules/Content/Content/Application/Shared/Errors/CustomerErrors.cs`
```
feat(content): add CustomerErrors factory class with AlreadyExists and NotFound
```

### 49
**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/CategoryErrorMessage.cs`
```
feat(content): add CategoryErrorMessage with domain error strings
```

### 50
**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/CustomerErrorMessage.cs`
```
feat(content): add CustomerErrorMessage with domain error strings
```

### 51
**File:** `src/Modules/Content/Content/Application/Shared/Errors/Messages/PackageErrorMessage.cs`
```
feat(content): add PackageErrorMessage with domain error strings
```

### 52
**File:** `src/Modules/Content/Content/Application/Shared/Errors/PackageErrors.cs`
```
feat(content): add PackageErrors factory class with AlreadyExists and NotFound
```

### 53
**File:** `src/Modules/Content/Content/Application/Shared/Mappers/CategoryMapper.cs`
```
feat(content): add Mapster mapping config for CategoryEntity to CategoryDto
```

### 54
**File:** `src/Modules/Content/Content/Application/Shared/Mappers/CustomerMapper.cs`
```
feat(content): add Mapster mapping config for CustomerEntity to CustomerDto
```

### 55
**File:** `src/Modules/Content/Content/Application/Shared/Mappers/PackageMapper.cs`
```
feat(content): add Mapster mapping config for PackageEntity to PackageDto
```

### 56
**File:** `src/Modules/Content/Content/Application/Shared/Repositories/ICategoryRepository.cs`
```
feat(content): add ICategoryRepository interface:

- Define AddAsync, GetByIdOrThrowAsync, ExistsByNameAsync methods
- Add GetAllAsync and GetActiveAsync for query use cases
```

### 57
**File:** `src/Modules/Content/Content/Application/Shared/Repositories/ICustomerRepository.cs`
```
feat(content): add ICustomerRepository interface:

- Define AddAsync, GetByIdOrThrowAsync, ExistsByNameAsync methods
- Add GetAllAsync for query use cases
```