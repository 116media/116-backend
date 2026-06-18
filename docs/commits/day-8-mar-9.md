# Day 8 — March 9, 2026 (44 commits)
## Lookup command tests completion + lookup query tests + content shared errors/mapper + identity session V1

**Start time:** 08:15
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivateContentType/DeactivateContentTypeHandlerTests.cs`
```
test(content): add DeactivateContentType handler tests for success and conflict paths:

- Assert NotFoundException when content type id does not exist
- Assert ConflictException when content type is already inactive
- Verify Deactivate() is called and SaveChangesAsync is invoked on success
```

### 2
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivateContentType/DeactivateContentTypeValidatorTests.cs`
```
test(content): add DeactivateContentType validator tests:

- Assert validation fails when ContentTypeId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 3
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivateContentType/V1/DeactivateContentTypeEndpointV1Tests.cs`
```
test(content): add DeactivateContentType endpoint v1 tests
```

### 4
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePricingTier/DeactivatePricingTierHandlerTests.cs`
```
test(content): add DeactivatePricingTier handler tests for success and conflict paths:

- Assert NotFoundException when pricing tier id does not exist
- Assert ConflictException when pricing tier is already inactive
- Verify Deactivate() is called and SaveChangesAsync is invoked on success
```

### 5
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePricingTier/DeactivatePricingTierValidatorTests.cs`
```
test(content): add DeactivatePricingTier validator tests:

- Assert validation fails when PricingTierId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 6
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePricingTier/V1/DeactivatePricingTierEndpointV1Tests.cs`
```
test(content): add DeactivatePricingTier endpoint v1 tests
```

### 7
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/DeactivatePromotionLevelHandlerTests.cs`
```
test(content): add DeactivatePromotionLevel handler tests for success and conflict paths:

- Assert NotFoundException when promotion level id does not exist
- Assert ConflictException when promotion level is already inactive
- Verify Deactivate() is called and SaveChangesAsync is invoked on success
```

### 8
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/DeactivatePromotionLevelValidatorTests.cs`
```
test(content): add DeactivatePromotionLevel validator tests:

- Assert validation fails when PromotionLevelId is an empty Guid
- Assert validation passes for a valid non-empty Guid input
```

### 9
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/V1/DeactivatePromotionLevelEndpointV1Tests.cs`
```
test(content): add DeactivatePromotionLevel endpoint v1 tests
```

### 10
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdateContentType/UpdateContentTypeHandlerTests.cs`
```
test(content): add UpdateContentType handler tests for success and duplicate-name paths:

- Assert NotFoundException when content type id does not exist
- Assert ConflictException when new name conflicts with another via ILike check
- Verify entity.Update is called and unit of work SaveChangesAsync is committed
```

### 11
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdateContentType/UpdateContentTypeValidatorTests.cs`
```
test(content): add UpdateContentType validator tests:

- Assert ContentTypeId must be a non-empty Guid
- Assert Name is required and within max length limit
```

### 12
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdateContentType/V1/UpdateContentTypeEndpointV1Tests.cs`
```
test(content): add UpdateContentType endpoint v1 tests
```

### 13
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePricingTier/UpdatePricingTierHandlerTests.cs`
```
test(content): add UpdatePricingTier handler tests for success and duplicate-name paths:

- Assert NotFoundException when pricing tier id does not exist
- Assert ConflictException when new name conflicts with another via ILike check
- Verify entity.Update is called and unit of work SaveChangesAsync is committed
```

### 14
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePricingTier/UpdatePricingTierValidatorTests.cs`
```
test(content): add UpdatePricingTier validator tests:

- Assert PricingTierId must be a non-empty Guid
- Assert Name is required and within max length
```

### 15
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePricingTier/V1/UpdatePricingTierEndpointV1Tests.cs`
```
test(content): add UpdatePricingTier endpoint v1 tests
```

### 16
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/UpdatePromotionLevelHandlerTests.cs`
```
test(content): add UpdatePromotionLevel handler tests for success and duplicate-name paths:

- Assert NotFoundException when promotion level id does not exist
- Assert ConflictException when new name conflicts with another via ILike check
- Verify entity.Update is called and unit of work SaveChangesAsync is committed
```

### 17
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/UpdatePromotionLevelValidatorTests.cs`
```
test(content): add UpdatePromotionLevel validator tests:

- Assert PromotionLevelId must be a non-empty Guid
- Assert Name is required and within max length
```

### 18
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/V1/UpdatePromotionLevelEndpointV1Tests.cs`
```
test(content): add UpdatePromotionLevel endpoint v1 tests
```

### 19
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllContentTypes/GetAllContentTypesHandlerTests.cs`
```
test(content): add GetAllContentTypes handler tests with filter assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert IsActive filter limits results to active content types only
```

### 20
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllContentTypes/V1/GetAllContentTypesEndpointV1Tests.cs`
```
test(content): add GetAllContentTypes endpoint v1 tests
```

### 21
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPricingTiers/GetAllPricingTiersHandlerTests.cs`
```
test(content): add GetAllPricingTiers handler tests with filter assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert IsActive filter limits results to active pricing tiers only
```

### 22
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPricingTiers/V1/GetAllPricingTiersEndpointV1Tests.cs`
```
test(content): add GetAllPricingTiers endpoint v1 tests
```

### 23
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPromotionLevels/GetAllPromotionLevelsHandlerTests.cs`
```
test(content): add GetAllPromotionLevels handler tests with filter assertions:

- Verify PagedResponse is returned with correct total count and items
- Assert IsActive filter limits results to active promotion levels only
```

### 24
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Admin/Queries/GetAllPromotionLevels/V1/GetAllPromotionLevelsEndpointV1Tests.cs`
```
test(content): add GetAllPromotionLevels endpoint v1 tests
```

### 25
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Public/Queries/GetActivePromotionLevels/GetActivePromotionLevelsHandlerTests.cs`
```
test(content): add GetActivePromotionLevels handler tests for active-only filter:

- Verify only active promotion levels are returned via IsActive specification
- Assert PagedResponse total count reflects active-only filtered results
```

### 26
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Public/Queries/GetActivePromotionLevels/V1/GetActivePromotionLevelsEndpointV1Tests.cs`
```
test(content): add GetActivePromotionLevels endpoint v1 tests
```

### 27
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/GetAllTagsHandlerTests.cs`
```
test(content): add GetAllTags handler tests with active-filter and pagination assertions:

- Verify only active tags are returned via IsActive specification
- Assert PagedResponse total count reflects active-only filtered results
```

### 28
**File:** `tests/Unit/Modules/Content/Application/Lookup/UseCases/Public/Queries/GetAllTags/V1/GetAllTagsEndpointV1Tests.cs`
```
test(content): add GetAllTags endpoint v1 tests
```

### 29
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/CategoryErrorsTests.cs`
```
test(content): add CategoryErrors tests for AlreadyExists and NotFound factories:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 30
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/ContentTypeErrorsTests.cs`
```
test(content): add ContentTypeErrors tests for error factory methods:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 31
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/CustomerErrorsTests.cs`
```
test(content): add CustomerErrors tests for AlreadyExists and NotFound factories:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 32
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/PackageErrorsTests.cs`
```
test(content): add PackageErrors tests for AlreadyExists and NotFound factories:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 33
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/PricingTierErrorsTests.cs`
```
test(content): add PricingTierErrors tests for error factory methods:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 34
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/PromotionLevelErrorsTests.cs`
```
test(content): add PromotionLevelErrors tests for error factory methods:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundById returns NotFoundException with expected error message
```

### 35
**File:** `tests/Unit/Modules/Content/Application/Shared/Errors/TagErrorsTests.cs`
```
test(content): add TagErrors tests for error factory methods:

- Assert AlreadyExists returns ConflictException with expected error message
- Assert NotFoundBySlug returns NotFoundException with expected error message
```

### 36
**File:** `tests/Unit/Modules/Content/Application/Shared/Mappers/MapperExtensionTests.cs`
```
test(content): add MapperExtension tests for Mapster mapping correctness:

- Verify CategoryEntity maps to CategoryDto with all fields including Pricings list
- Verify CustomerEntity maps to CustomerDto preserving Name, Email, Phone
- Verify PackageEntity maps to PackageDto with nested PackageSlotDto list
```

### 37
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Queries/ExportSessionData/V1/AdminExportSessionDataEndpointV1Tests.cs`
```
test(identity): add AdminExportSessionData endpoint v1 tests
```

### 38
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs`
```
test(identity): add AdminGetAllSessions endpoint v1 tests
```

### 39
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Admin/Queries/GetSessionMetrics/V1/AdminGetSessionMetricsEndpointV1Tests.cs`
```
test(identity): add AdminGetSessionMetrics endpoint v1 tests
```

### 40
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Public/Commands/RefreshToken/V1/PublicRefreshTokenEndpointV1Tests.cs`
```
test(identity): add PublicRefreshToken endpoint v1 tests
```

### 41
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Public/Commands/RevokeSession/V1/PublicRevokeSessionEndpointV1Tests.cs`
```
test(identity): add PublicRevokeSession endpoint v1 tests
```

### 42
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Public/Queries/GetOwnSessionById/V1/PublicGetOwnSessionByIdEndpointV1Tests.cs`
```
test(identity): add PublicGetOwnSessionById endpoint v1 tests
```

### 43
**File:** `tests/Unit/Modules/Identity/Application/Session/UseCases/Public/Queries/GetOwnSessions/V1/PublicGetOwnSessionsEndpointV1Tests.cs`
```
test(identity): add PublicGetOwnSessions endpoint v1 tests
```

### 44
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Commands/AssignRoleToUser/V1/AdminAssignRoleToUserEndpointV1Tests.cs`
```
test(identity): add AdminAssignRoleToUser endpoint v1 tests
```
