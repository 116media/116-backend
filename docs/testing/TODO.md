# Content Module — Unit Tests TODO

> Last updated: 2026-03-16 (Session 4 — Editorial started)
> Status: **Catalog/Lookup/Identity DONE** — 199 test files, ~3806 passing, 20 skipped. Editorial IN PROGRESS.

## Legend
- `[x]` = Done and passing
- `[s]` = Done but skipped (ILike not supported by InMemoryDatabase)
- `[ ]` = Pending

---

## Infrastructure Setup

- [x] `tests/Unit/_116.Unit.Tests.csproj` — added Content.csproj reference
- [x] `tests/Fixtures/_116.Tests.Fixtures.csproj` — added Content.csproj reference
- [x] `tests/Fixtures/Constants/TestConstants.cs` — added `Content` nested class
- [x] `tests/Unit/Common/BaseContentHandlerTest.cs` — mapper setup for content module

---

## Fixture Builders (`tests/Fixtures/Builders/Entities/Content/`)

- [x] `ContentTypeBuilder.cs`
- [x] `PricingTierBuilder.cs`
- [x] `PromotionLevelBuilder.cs`
- [x] `TagBuilder.cs`
- [x] `CategoryBuilder.cs`
- [x] `CategoryPricingBuilder.cs`
- [x] `CustomerBuilder.cs`
- [x] `PackageBuilder.cs`
- [x] `PackageSlotBuilder.cs`

---

## Fixture Factories (`tests/Fixtures/Factories/Content/`)

- [x] `ContentTypeFactory.cs`
- [x] `PricingTierFactory.cs`
- [x] `PromotionLevelFactory.cs`
- [x] `TagFactory.cs`
- [x] `CategoryFactory.cs`
- [x] `CategoryPricingFactory.cs`
- [x] `CustomerFactory.cs`
- [x] `PackageFactory.cs`
- [x] `PackageSlotFactory.cs`

---

## Mock Repositories & Infrastructure (`tests/Unit/Common/Mocks/`)

- [x] `Repositories/MockLookupRepository.cs`
- [x] `Repositories/MockCategoryRepository.cs`
- [x] `Repositories/MockCustomerRepository.cs`
- [x] `Repositories/MockPackageRepository.cs`
- [x] `Infrastructure/MockContentUnitOfWork.cs`

---

## Domain Entity Tests (`tests/Unit/Modules/Content/Domain/Entities/`)

- [x] `ContentTypeEntityTests.cs`
- [x] `PricingTierEntityTests.cs`
- [x] `PromotionLevelEntityTests.cs`
- [x] `TagEntityTests.cs`
- [x] `CategoryEntityTests.cs`
- [x] `CategoryPricingEntityTests.cs`
- [x] `CustomerEntityTests.cs`
- [x] `PackageEntityTests.cs`
- [x] `PackageSlotEntityTests.cs`

---

## Application — Shared — Errors Tests (`tests/Unit/Modules/Content/Application/Shared/Errors/`)

- [x] `ContentTypeErrorsTests.cs`
- [x] `PricingTierErrorsTests.cs`
- [x] `PromotionLevelErrorsTests.cs`
- [x] `TagErrorsTests.cs`
- [x] `CategoryErrorsTests.cs`
- [x] `CustomerErrorsTests.cs`
- [x] `PackageErrorsTests.cs`

---

## Application — Shared — Validator Branch Tests (NEW — Session 2)

- [x] `Application/Shared/Validators/SharedValidatorsTests.cs` — isRequired=false branches for ContentType, PricingTier, PromotionLevel, Tag, Category shared validators

---

## Application — Shared — Mapper Extension Tests (NEW — Session 2)

- [x] `Application/Shared/Mappers/MapperExtensionTests.cs` — ToCustomerDtos, ToPackageDtos, ToPackageSlotDto, ToCategoryDtos collection extensions

---

## Application — Lookup — MetaField Tests (NEW — Session 2)

- [x] `Application/Lookup/MetaFields/LookupMetaFieldTests.cs` — all 18 Lookup MetaField static fields

---

## Application — Lookup — Specification Tests

- [x] `Application/Lookup/Specifications/LookupSpecificationsTests.cs` — all lookup specs including ByName (ILike ToExpression-only) and TagByName (full eval)

---

## Application — Lookup — Handler Tests — Admin Commands

- [x] `Lookup/UseCases/Admin/Commands/CreateContentType/CreateContentTypeHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdateContentType/UpdateContentTypeHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/ActivateContentType/ActivateContentTypeHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivateContentType/DeactivateContentTypeHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreatePricingTier/CreatePricingTierHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdatePricingTier/UpdatePricingTierHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/ActivatePricingTier/ActivatePricingTierHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivatePricingTier/DeactivatePricingTierHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreatePromotionLevel/CreatePromotionLevelHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/UpdatePromotionLevelHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/ActivatePromotionLevelHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/DeactivatePromotionLevelHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreateTag/CreateTagHandlerTests.cs`

---

## Application — Lookup — Handler Tests — Admin Queries

- [x] `Lookup/UseCases/Admin/Queries/GetAllContentTypes/GetAllContentTypesHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Queries/GetAllPricingTiers/GetAllPricingTiersHandlerTests.cs`
- [x] `Lookup/UseCases/Admin/Queries/GetAllPromotionLevels/GetAllPromotionLevelsHandlerTests.cs`

---

## Application — Lookup — Handler Tests — Public Queries

- [x] `Lookup/UseCases/Public/Queries/GetActivePromotionLevels/GetActivePromotionLevelsHandlerTests.cs`
- [x] `Lookup/UseCases/Public/Queries/GetAllTags/GetAllTagsHandlerTests.cs`

---

## Application — Lookup — Validator Tests

- [x] `Lookup/UseCases/Admin/Commands/CreateContentType/CreateContentTypeValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdateContentType/UpdateContentTypeValidatorTests.cs` ← NEW Session 2
- [x] `Lookup/UseCases/Admin/Commands/ActivateContentType/ActivateContentTypeValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivateContentType/DeactivateContentTypeValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreatePricingTier/CreatePricingTierValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdatePricingTier/UpdatePricingTierValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/ActivatePricingTier/ActivatePricingTierValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivatePricingTier/DeactivatePricingTierValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreatePromotionLevel/CreatePromotionLevelValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/UpdatePromotionLevel/UpdatePromotionLevelValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/ActivatePromotionLevel/ActivatePromotionLevelValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/DeactivatePromotionLevel/DeactivatePromotionLevelValidatorTests.cs`
- [x] `Lookup/UseCases/Admin/Commands/CreateTag/CreateTagValidatorTests.cs`

---

## Application — Catalog — MetaField Tests (NEW — Session 2)

- [x] `Application/Catalog/MetaFields/CatalogMetaFieldTests.cs` — all 21 Catalog MetaField static fields

---

## Application — Catalog — Specification Tests

- [x] `Application/Catalog/Specifications/CatalogSpecificationsTests.cs` — all catalog specs including InactivePackageSpecification (added Session 2)

---

## Application — Catalog — Handler Tests — Admin Commands

- [x] `Catalog/UseCases/Admin/Commands/CreateCategory/CreateCategoryHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/ActivateCategory/ActivateCategoryHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/CreateCustomer/CreateCustomerHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/ActivatePackage/ActivatePackageHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/AddCategoryPricing/AddCategoryPricingHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/RemoveCategoryPricing/RemoveCategoryPricingHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/AddPackageSlot/AddPackageSlotHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/RemovePackageSlot/RemovePackageSlotHandlerTests.cs`

---

## Application — Catalog — Handler Tests — Admin Queries

- [x] `Catalog/UseCases/Admin/Queries/GetAllCategories/GetAllCategoriesHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Queries/GetCategoryById/GetCategoryByIdHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Queries/GetAllCustomers/GetAllCustomersHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Queries/GetCustomerById/GetCustomerByIdHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Queries/GetAllPackages/GetAllPackagesHandlerTests.cs`
- [x] `Catalog/UseCases/Admin/Queries/GetPackageById/GetPackageByIdHandlerTests.cs`

---

## Application — Catalog — Handler Tests — Public Queries

- [x] `Catalog/UseCases/Public/Queries/GetPublicCategories/GetPublicCategoriesHandlerTests.cs`

---

## Application — Catalog — Validator Tests

- [x] `Catalog/UseCases/Admin/Commands/CreateCategory/CreateCategoryValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCategory/UpdateCategoryValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/ActivateCategory/ActivateCategoryValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/DeactivateCategory/DeactivateCategoryValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/CreateCustomer/CreateCustomerValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCustomer/UpdateCustomerValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/CreatePackage/CreatePackageValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/AddPackageSlot/AddPackageSlotValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/AddCategoryPricing/AddCategoryPricingValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/UpdateCategoryPricing/UpdateCategoryPricingValidatorTests.cs`
- [x] `Catalog/UseCases/Admin/Commands/ActivatePackage/ActivatePackageValidatorTests.cs` ← NEW Session 2
- [x] `Catalog/UseCases/Admin/Commands/DeactivatePackage/DeactivatePackageValidatorTests.cs` ← NEW Session 2

---

## Infrastructure Tests (`tests/Unit/Modules/Content/Infrastructure/`)

- [x] `Repositories/LookupRepositoryTests.cs`
- [x] `Repositories/CategoryRepositoryTests.cs` — 1 test skipped (GetBySlugAsync uses ILike)
- [x] `Repositories/CustomerRepositoryTests.cs` — 1 test skipped (GetByEmailAsync uses ILike)
- [x] `Repositories/PackageRepositoryTests.cs`
- [x] `Persistence/ContentDbContextTests.cs`
- [x] `Persistence/ContentUnitOfWorkTests.cs`
- [x] `Seeds/ContentTypeSeederTests.cs` ← NEW Session 2

---

## Module Tests

- [x] `ContentModuleTests.cs` — extended in Session 2 with Testing environment branch test

---

---

## Session 3 — Identity GetOwnRoles (NEW)

- [x] `Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesHandlerTests.cs` — 6 tests
- [x] `Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesHandlerTests.cs` — 6 tests
- [x] `Roles/MetaFields/GetOwnRolesMetaFieldTests.cs` — 2 tests

---

## Session 3 — Endpoint Record Tests (NEW)

Each file named `{EndpointClassName}Tests.cs`, placed in the exact mirrored directory of the source file.

### Identity Auth (`tests/Unit/Modules/Identity/Application/Auth/UseCases/`)
- [x] `Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs`
- [x] `Admin/Commands/ForgotPassword/V1/AdminForgotPasswordEndpointV1Tests.cs`
- [x] `Admin/Commands/ResendOtp/V1/AdminResendOtpEndpointV1Tests.cs`
- [x] `Admin/Commands/ResetPassword/V1/AdminResetPasswordEndpointV1Tests.cs`
- [x] `Admin/Commands/VerifyOtp/V1/AdminVerifyOtpEndpointV1Tests.cs`
- [x] `Admin/Commands/ChangePassword/V1/AdminChangePasswordEndpointV1Tests.cs`
- [x] `Admin/Commands/SignOut/V1/AdminSignOutEndpointV1Tests.cs`
- [x] `Admin/Commands/SignOutFromAllDevices/V1/AdminSignOutFromAllDevicesEndpointV1Tests.cs`
- [x] `Public/Commands/Login/V1/PublicLoginEndpointV1Tests.cs`
- [x] `Public/Commands/SocialLogin/V1/PublicSocialLoginEndpointV1Tests.cs`
- [x] `Public/Commands/SignUp/V1/PublicSignUpEndpointV1Tests.cs`
- [x] `Public/Commands/ForgotPassword/V1/PublicForgotPasswordEndpointV1Tests.cs`
- [x] `Public/Commands/ResendOtp/V1/PublicResendOtpEndpointV1Tests.cs`
- [x] `Public/Commands/ResetPassword/V1/PublicResetPasswordEndpointV1Tests.cs`
- [x] `Public/Commands/VerifyOtp/V1/PublicVerifyOtpEndpointV1Tests.cs`
- [x] `Public/Commands/ChangePassword/V1/PublicChangePasswordEndpointV1Tests.cs`
- [x] `Public/Commands/SetPassword/V1/PublicSetPasswordEndpointV1Tests.cs`
- [x] `Public/Commands/SignOut/V1/PublicSignOutEndpointV1Tests.cs`
- [x] `Public/Commands/SignOutFromAllDevices/V1/PublicSignOutFromAllDevicesEndpointV1Tests.cs`

### Identity Session (`tests/Unit/Modules/Identity/Application/Session/UseCases/`)
- [x] `Public/Commands/RefreshToken/V1/PublicRefreshTokenEndpointV1Tests.cs`
- [x] `Public/Commands/RevokeSession/V1/PublicRevokeSessionEndpointV1Tests.cs`
- [x] `Public/Queries/GetOwnSessions/V1/PublicGetOwnSessionsEndpointV1Tests.cs`
- [x] `Public/Queries/GetOwnSessionById/V1/PublicGetOwnSessionByIdEndpointV1Tests.cs`
- [x] `Admin/Commands/CleanupExpiredSessions/V1/AdminCleanupExpiredSessionsEndpointV1Tests.cs`
- [x] `Admin/Commands/ForceLogoutUser/V1/AdminForceLogoutUserEndpointV1Tests.cs`
- [x] `Admin/Queries/ExportSessionData/V1/AdminExportSessionDataEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllSessions/V1/AdminGetAllSessionsEndpointV1Tests.cs`
- [x] `Admin/Queries/GetSessionMetrics/V1/AdminGetSessionMetricsEndpointV1Tests.cs`

### Identity User (`tests/Unit/Modules/Identity/Application/User/UseCases/`)
- [x] `Public/Queries/GetOwnProfile/V1/PublicGetOwnProfileEndpointV1Tests.cs`
- [x] `Public/Commands/UpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1Tests.cs` ← includes `PublicUpdateOwnProfileRequest` (nullable params)
- [x] `Public/Commands/UpdateAvatar/V1/PublicUpdateAvatarEndpointV1Tests.cs`
- [x] `Admin/Queries/GetOwnProfile/V1/AdminGetOwnProfileEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdateOwnProfile/V1/AdminUpdateOwnProfileEndpointV1Tests.cs` ← includes `AdminUpdateOwnProfileRequest` (nullable params)
- [x] `Admin/Commands/UpdateAvatar/V1/AdminUpdateAvatarEndpointV1Tests.cs`
- [x] `Admin/Queries/GetUserRoles/V1/AdminGetUserRolesEndpointV1Tests.cs`
- [x] `Admin/Commands/AssignRoleToUser/V1/AdminAssignRoleToUserEndpointV1Tests.cs`
- [x] `Admin/Commands/RemoveRoleFromUser/V1/AdminRemoveRoleFromUserEndpointV1Tests.cs`

### Identity Roles/Permissions (`tests/Unit/Modules/Identity/Application/Roles/UseCases/`)
- [x] `Admin/Commands/CreateRole/V1/AdminCreateRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdateRole/V1/AdminUpdateRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivateRole/V1/AdminActivateRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivateRole/V1/AdminDeactivateRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/SoftDeleteRole/V1/AdminSoftDeleteRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/HardDeleteRole/V1/AdminHardDeleteRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/RestoreRole/V1/AdminRestoreRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/CreatePermission/V1/AdminCreatePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdatePermission/V1/AdminUpdatePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivatePermission/V1/AdminActivatePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivatePermission/V1/AdminDeactivatePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/SoftDeletePermission/V1/AdminSoftDeletePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/HardDeletePermission/V1/AdminHardDeletePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/RestorePermission/V1/AdminRestorePermissionEndpointV1Tests.cs`
- [x] `Admin/Commands/AssignPermissionToRole/V1/AdminAssignPermissionToRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/RemovePermissionFromRole/V1/AdminRemovePermissionFromRoleEndpointV1Tests.cs`
- [x] `Admin/Commands/BulkUpdateRolePermissions/V1/AdminBulkUpdateRolePermissionsEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllRoles/V1/AdminGetAllRolesEndpointV1Tests.cs`
- [x] `Admin/Queries/GetRoleById/V1/AdminGetRoleByIdEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllPermissions/V1/AdminGetAllPermissionsEndpointV1Tests.cs`
- [x] `Admin/Queries/GetPermissionById/V1/AdminGetPermissionByIdEndpointV1Tests.cs`
- [x] `Admin/Queries/GetOwnRoles/V1/AdminGetOwnRolesEndpointV1Tests.cs`
- [x] `Public/Queries/GetOwnRoles/V1/PublicGetOwnRolesEndpointV1Tests.cs`

### Content Lookup (`tests/Unit/Modules/Content/Application/Lookup/UseCases/`)
- [x] `Admin/Commands/CreateContentType/V1/CreateContentTypeEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdateContentType/V1/UpdateContentTypeEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivateContentType/V1/ActivateContentTypeEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivateContentType/V1/DeactivateContentTypeEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllContentTypes/V1/GetAllContentTypesEndpointV1Tests.cs`
- [x] `Admin/Commands/CreatePricingTier/V1/CreatePricingTierEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdatePricingTier/V1/UpdatePricingTierEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivatePricingTier/V1/ActivatePricingTierEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivatePricingTier/V1/DeactivatePricingTierEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllPricingTiers/V1/GetAllPricingTiersEndpointV1Tests.cs`
- [x] `Admin/Commands/CreatePromotionLevel/V1/CreatePromotionLevelEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdatePromotionLevel/V1/UpdatePromotionLevelEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivatePromotionLevel/V1/ActivatePromotionLevelEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivatePromotionLevel/V1/DeactivatePromotionLevelEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllPromotionLevels/V1/GetAllPromotionLevelsEndpointV1Tests.cs`
- [x] `Public/Queries/GetActivePromotionLevels/V1/GetActivePromotionLevelsEndpointV1Tests.cs`
- [x] `Admin/Commands/CreateTag/V1/CreateTagEndpointV1Tests.cs`
- [x] `Public/Queries/GetAllTags/V1/GetAllTagsEndpointV1Tests.cs`

### Content Catalog (`tests/Unit/Modules/Content/Application/Catalog/UseCases/`)
- [x] `Admin/Commands/CreateCategory/V1/CreateCategoryEndpointV1Tests.cs` ← includes `CreateCategoryRequest` (nullable `Description`)
- [x] `Admin/Commands/UpdateCategory/V1/UpdateCategoryEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivateCategory/V1/ActivateCategoryEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivateCategory/V1/DeactivateCategoryEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllCategories/V1/GetAllCategoriesEndpointV1Tests.cs`
- [x] `Admin/Queries/GetCategoryById/V1/GetCategoryByIdEndpointV1Tests.cs`
- [x] `Public/Queries/GetPublicCategories/V1/GetPublicCategoriesEndpointV1Tests.cs`
- [x] `Admin/Commands/AddCategoryPricing/V1/AddCategoryPricingEndpointV1Tests.cs`
- [x] `Admin/Commands/UpdateCategoryPricing/V1/UpdateCategoryPricingEndpointV1Tests.cs`
- [x] `Admin/Commands/RemoveCategoryPricing/V1/RemoveCategoryPricingEndpointV1Tests.cs`
- [x] `Admin/Commands/CreateCustomer/V1/CreateCustomerEndpointV1Tests.cs` ← includes `CreateCustomerRequest` (nullable params)
- [x] `Admin/Commands/UpdateCustomer/V1/UpdateCustomerEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllCustomers/V1/GetAllCustomersEndpointV1Tests.cs`
- [x] `Admin/Queries/GetCustomerById/V1/GetCustomerByIdEndpointV1Tests.cs`
- [x] `Admin/Commands/CreatePackage/V1/CreatePackageEndpointV1Tests.cs`
- [x] `Admin/Commands/ActivatePackage/V1/ActivatePackageEndpointV1Tests.cs`
- [x] `Admin/Commands/DeactivatePackage/V1/DeactivatePackageEndpointV1Tests.cs`
- [x] `Admin/Commands/AddPackageSlot/V1/AddPackageSlotEndpointV1Tests.cs`
- [x] `Admin/Commands/RemovePackageSlot/V1/RemovePackageSlotEndpointV1Tests.cs`
- [x] `Admin/Queries/GetAllPackages/V1/GetAllPackagesEndpointV1Tests.cs`
- [x] `Admin/Queries/GetPackageById/V1/GetPackageByIdEndpointV1Tests.cs`

---

## Test Results Summary

| Category | Files | Tests |
|----------|-------|-------|
| Domain Entities | 9 | ~50 |
| Error Factories | 7 | ~35 |
| Lookup Specifications | 1 | ~20 |
| Catalog Specifications | 1 | ~17 |
| Lookup MetaFields | 1 | 18 |
| Catalog MetaFields | 1 | 21 |
| GetOwnRoles MetaFields | 1 | 2 |
| Lookup Handlers | 18 | ~90 |
| Lookup Validators | 13 | ~59 |
| Catalog Handlers | 21 | ~105 |
| Catalog Validators | 12 | ~54 |
| Identity GetOwnRoles Handlers | 2 | 12 |
| Shared Validators | 1 | 24 |
| Mapper Extensions | 1 | 10 |
| Infrastructure | 7 | ~70 |
| Module | 1 | ~9 |
| Identity Endpoint Records | 60 | ~74 |
| Content Endpoint Records | 39 | ~47 |
| **Total** | **199** | **~3806 pass, 20 skip** |

---

## Coverage Progression

| Session | Tests | Coverage |
|---------|-------|----------|
| Session 1 (2026-03-10) | 541 pass, 2 skip | 30.5% |
| Session 2 (2026-03-10) | 640 pass, 2 skip | ~85-90% |
| Session 3 (2026-03-10) | ~3806 pass, 20 skip | ~85-90%+ (endpoint records covered) |

---

## Known Limitations (Integration Tests Needed)

The following cannot be tested with InMemoryDatabase (PostgreSQL-specific):
- `CategoryRepositoryTests.GetBySlugAsync_WhenFound` — `CategoryBySlugSpecification` uses `EF.Functions.ILike`
- `CustomerRepositoryTests.GetByEmailAsync_WhenFound` — `CustomerByEmailSpecification` uses `EF.Functions.ILike`
- `LookupRepositoryTests` — `ContentTypeExistsByNameAsync`, `PricingTierExistsByNameAsync`, `PromotionLevelExistsByNameAsync` all use `ILike`
- `ContentTypeByNameSpecification`, `PricingTierByNameSpecification`, `PromotionLevelByNameSpecification`, `TagSearchSpecification` — ToExpression/Compile tested only; in-memory evaluation not possible

### EndpointV1 `AddRoutes` methods remain at 0% by design
The `*Response` and `*Request` record constructors inside each endpoint file are now covered.
Only the `ICarterModule.AddRoutes(IEndpointRouteBuilder)` implementation still requires a real ASP.NET Core host (integration tests).

---

## Session 4 — Editorial Submodule Tests

---

### Infrastructure

- [ ] `tests/Fixtures/Constants/TestConstants.cs` — add `Editorial` nested class under `Content`
- [ ] `tests/Fixtures/Builders/Entities/Content/ArticleBuilder.cs`
- [ ] `tests/Fixtures/Builders/Entities/Content/VideoBuilder.cs`
- [ ] `tests/Fixtures/Builders/Entities/Content/ShortVideoBuilder.cs`
- [ ] `tests/Fixtures/Builders/Entities/Content/LyricsBuilder.cs`
- [ ] `tests/Fixtures/Builders/Entities/Content/ArticleImageBuilder.cs`
- [ ] `tests/Fixtures/Factories/Content/ArticleFactory.cs`
- [ ] `tests/Fixtures/Factories/Content/VideoFactory.cs`
- [ ] `tests/Fixtures/Factories/Content/ShortVideoFactory.cs`
- [ ] `tests/Fixtures/Factories/Content/LyricsFactory.cs`
- [ ] `tests/Fixtures/Factories/Content/ArticleImageFactory.cs`
- [ ] `tests/Unit/Common/Mocks/Repositories/MockArticleRepository.cs`
- [ ] `tests/Unit/Common/Mocks/Repositories/MockVideoRepository.cs`
- [ ] `tests/Unit/Common/Mocks/Repositories/MockShortVideoRepository.cs`
- [ ] `tests/Unit/Common/Mocks/Repositories/MockLyricsRepository.cs`
- [ ] `tests/Unit/Common/Mocks/Services/MockCloudinaryService.cs`
- [ ] `tests/Unit/Common/Mocks/Services/MockYoutubeThumbnailService.cs`

---

### Domain Entity Tests (`tests/Unit/Modules/Content/Domain/Entities/`)

- [ ] `ArticleEntityTests.cs`
- [ ] `VideoEntityTests.cs`
- [ ] `ShortVideoEntityTests.cs`
- [ ] `LyricsEntityTests.cs`
- [ ] `ArticleImageEntityTests.cs`

---

### Shared Errors Tests (`tests/Unit/Modules/Content/Application/Shared/Errors/`)

- [ ] `ArticleErrorsTests.cs`
- [ ] `VideoErrorsTests.cs`
- [ ] `ShortVideoErrorsTests.cs`
- [ ] `LyricsErrorsTests.cs`

---

### Specification Tests (`tests/Unit/Modules/Content/Application/Editorial/Specifications/`)

- [ ] `ArticleSpecificationsTests.cs`
- [ ] `VideoSpecificationsTests.cs`
- [ ] `ShortVideoSpecificationsTests.cs`
- [ ] `LyricsSpecificationsTests.cs`

---

### MetaField Tests

- [ ] `tests/Unit/Modules/Content/Application/Editorial/MetaFields/EditorialAdminMetaFieldTests.cs`
- [ ] `tests/Unit/Modules/Content/Application/Editorial/MetaFields/EditorialPublicMetaFieldTests.cs`

---

### Handler Tests — Admin Article Commands (`tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/`)

- [ ] `CreateArticle/AdminCreateArticleHandlerTests.cs`
- [ ] `SubmitArticle/AdminSubmitArticleHandlerTests.cs`
- [ ] `ApproveArticle/AdminApproveArticleHandlerTests.cs`
- [ ] `PublishArticle/AdminPublishArticleHandlerTests.cs`
- [ ] `RejectArticle/AdminRejectArticleHandlerTests.cs`
- [ ] `ArchiveArticle/AdminArchiveArticleHandlerTests.cs`
- [ ] `DeleteArticle/AdminDeleteArticleHandlerTests.cs`
- [ ] `UpdateArticle/AdminUpdateArticleHandlerTests.cs`
- [ ] `UpdateArticleSeo/AdminUpdateArticleSeoHandlerTests.cs`
- [ ] `UpdateArticleTags/AdminUpdateArticleTagsHandlerTests.cs`
- [ ] `UploadArticleImage/AdminUploadArticleImageHandlerTests.cs`

---

### Handler Tests — Admin Video Commands

- [ ] `CreateVideo/AdminCreateVideoHandlerTests.cs`
- [ ] `SubmitVideo/AdminSubmitVideoHandlerTests.cs`
- [ ] `ApproveVideo/AdminApproveVideoHandlerTests.cs`
- [ ] `PublishVideo/AdminPublishVideoHandlerTests.cs`
- [ ] `RejectVideo/AdminRejectVideoHandlerTests.cs`
- [ ] `ArchiveVideo/AdminArchiveVideoHandlerTests.cs`
- [ ] `AttachYoutubeId/AdminAttachYoutubeIdHandlerTests.cs`
- [ ] `UploadVideoThumbnail/AdminUploadVideoThumbnailHandlerTests.cs`
- [ ] `UpdateVideo/AdminUpdateVideoHandlerTests.cs`
- [ ] `UpdateVideoSeo/AdminUpdateVideoSeoHandlerTests.cs`
- [ ] `UpdateVideoTags/AdminUpdateVideoTagsHandlerTests.cs`
- [ ] `ScheduleShoot/AdminScheduleShootHandlerTests.cs`
- [ ] `DeleteVideo/AdminDeleteVideoHandlerTests.cs`

---

### Handler Tests — Admin Short Video Commands

- [ ] `CreateShortVideo/AdminCreateShortVideoHandlerTests.cs`
- [ ] `UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailHandlerTests.cs`
- [ ] `ActivateShortVideo/AdminActivateShortVideoHandlerTests.cs`
- [ ] `DeactivateShortVideo/AdminDeactivateShortVideoHandlerTests.cs`
- [ ] `DeleteShortVideo/AdminDeleteShortVideoHandlerTests.cs`

---

### Handler Tests — Admin Lyrics Commands

- [ ] `CreateLyrics/AdminCreateLyricsHandlerTests.cs`
- [ ] `UpdateLyrics/AdminUpdateLyricsHandlerTests.cs`
- [ ] `UpdateLyricsSeo/AdminUpdateLyricsSeoHandlerTests.cs`

---

### Handler Tests — Admin Queries

- [ ] `GetAllArticles/AdminGetAllArticlesHandlerTests.cs`
- [ ] `GetArticleById/AdminGetArticleByIdHandlerTests.cs`
- [ ] `GetAllVideos/AdminGetAllVideosHandlerTests.cs`
- [ ] `GetVideoById/AdminGetVideoByIdHandlerTests.cs`
- [ ] `GetAllShortVideos/AdminGetAllShortVideosHandlerTests.cs`
- [ ] `GetShortVideoById/AdminGetShortVideoByIdHandlerTests.cs`
- [ ] `GetAllLyrics/AdminGetAllLyricsHandlerTests.cs`

---

### Handler Tests — Public Queries

- [ ] `GetPublishedArticles/PublicGetPublishedArticlesHandlerTests.cs`
- [ ] `GetArticleBySlug/PublicGetArticleBySlugHandlerTests.cs`
- [ ] `GetPromotedArticles/PublicGetPromotedArticlesHandlerTests.cs`
- [ ] `GetPublishedVideos/PublicGetPublishedVideosHandlerTests.cs`
- [ ] `GetVideoBySlug/PublicGetVideoBySlugHandlerTests.cs`
- [ ] `GetPromotedVideos/PublicGetPromotedVideosHandlerTests.cs`
- [ ] `GetPublicShortVideos/PublicGetPublicShortVideosHandlerTests.cs`
- [ ] `GetPublicShortVideoBySlug/PublicGetPublicShortVideoBySlugHandlerTests.cs`
- [ ] `GetLyricsBySlug/PublicGetLyricsBySlugHandlerTests.cs`

---

### Validator Tests — Articles

- [ ] `CreateArticle/AdminCreateArticleValidatorTests.cs`
- [ ] `SubmitArticle/AdminSubmitArticleValidatorTests.cs`
- [ ] `ApproveArticle/AdminApproveArticleValidatorTests.cs`
- [ ] `PublishArticle/AdminPublishArticleValidatorTests.cs`
- [ ] `RejectArticle/AdminRejectArticleValidatorTests.cs`
- [ ] `ArchiveArticle/AdminArchiveArticleValidatorTests.cs`
- [ ] `DeleteArticle/AdminDeleteArticleValidatorTests.cs`
- [ ] `UpdateArticle/AdminUpdateArticleValidatorTests.cs`
- [ ] `UpdateArticleSeo/AdminUpdateArticleSeoValidatorTests.cs`
- [ ] `UpdateArticleTags/AdminUpdateArticleTagsValidatorTests.cs`
- [ ] `UploadArticleImage/AdminUploadArticleImageValidatorTests.cs`

---

### Validator Tests — Videos

- [ ] `CreateVideo/AdminCreateVideoValidatorTests.cs`
- [ ] `SubmitVideo/AdminSubmitVideoValidatorTests.cs`
- [ ] `ApproveVideo/AdminApproveVideoValidatorTests.cs`
- [ ] `AttachYoutubeId/AdminAttachYoutubeIdValidatorTests.cs`
- [ ] `RejectVideo/AdminRejectVideoValidatorTests.cs`
- [ ] `PublishVideo/AdminPublishVideoValidatorTests.cs`
- [ ] `UploadVideoThumbnail/AdminUploadVideoThumbnailValidatorTests.cs`
- [ ] `UpdateVideo/AdminUpdateVideoValidatorTests.cs`
- [ ] `UpdateVideoSeo/AdminUpdateVideoSeoValidatorTests.cs`
- [ ] `UpdateVideoTags/AdminUpdateVideoTagsValidatorTests.cs`
- [ ] `ScheduleShoot/AdminScheduleShootValidatorTests.cs`
- [ ] `ArchiveVideo/AdminArchiveVideoValidatorTests.cs`
- [ ] `DeleteVideo/AdminDeleteVideoValidatorTests.cs`

---

### Validator Tests — Short Videos

- [ ] `CreateShortVideo/AdminCreateShortVideoValidatorTests.cs`
- [ ] `UploadShortVideoThumbnail/AdminUploadShortVideoThumbnailValidatorTests.cs`
- [ ] `ActivateShortVideo/AdminActivateShortVideoValidatorTests.cs`
- [ ] `DeactivateShortVideo/AdminDeactivateShortVideoValidatorTests.cs`
- [ ] `DeleteShortVideo/AdminDeleteShortVideoValidatorTests.cs`

---

### Validator Tests — Lyrics

- [ ] `CreateLyrics/AdminCreateLyricsValidatorTests.cs`
- [ ] `UpdateLyrics/AdminUpdateLyricsValidatorTests.cs`
- [ ] `UpdateLyricsSeo/AdminUpdateLyricsSeoValidatorTests.cs`

---

### Shared Editorial Validation Branch Tests

- [ ] `tests/Unit/Modules/Content/Application/Shared/Validators/EditorialValidatorsTests.cs`

---

### Endpoint V1 Record Tests — Admin Article Commands (`tests/Unit/Modules/Content/Application/Editorial/UseCases/Admin/Commands/*/V1/`)

- [ ] `CreateArticle/V1/AdminCreateArticleEndpointV1Tests.cs`
- [ ] `SubmitArticle/V1/AdminSubmitArticleEndpointV1Tests.cs`
- [ ] `ApproveArticle/V1/AdminApproveArticleEndpointV1Tests.cs`
- [ ] `PublishArticle/V1/AdminPublishArticleEndpointV1Tests.cs`
- [ ] `RejectArticle/V1/AdminRejectArticleEndpointV1Tests.cs`
- [ ] `ArchiveArticle/V1/AdminArchiveArticleEndpointV1Tests.cs`
- [ ] `DeleteArticle/V1/AdminDeleteArticleEndpointV1Tests.cs`
- [ ] `UpdateArticle/V1/AdminUpdateArticleEndpointV1Tests.cs`
- [ ] `UpdateArticleSeo/V1/AdminUpdateArticleSeoEndpointV1Tests.cs`
- [ ] `UpdateArticleTags/V1/AdminUpdateArticleTagsEndpointV1Tests.cs`
- [ ] `UploadArticleImage/V1/AdminUploadArticleImageEndpointV1Tests.cs`

---

### Endpoint V1 Record Tests — Admin Video Commands

- [ ] `CreateVideo/V1/AdminCreateVideoEndpointV1Tests.cs`
- [ ] `SubmitVideo/V1/AdminSubmitVideoEndpointV1Tests.cs`
- [ ] `ApproveVideo/V1/AdminApproveVideoEndpointV1Tests.cs`
- [ ] `PublishVideo/V1/AdminPublishVideoEndpointV1Tests.cs`
- [ ] `RejectVideo/V1/AdminRejectVideoEndpointV1Tests.cs`
- [ ] `ArchiveVideo/V1/AdminArchiveVideoEndpointV1Tests.cs`
- [ ] `AttachYoutubeId/V1/AdminAttachYoutubeIdEndpointV1Tests.cs`
- [ ] `UploadVideoThumbnail/V1/AdminUploadVideoThumbnailEndpointV1Tests.cs`
- [ ] `UpdateVideo/V1/AdminUpdateVideoEndpointV1Tests.cs`
- [ ] `UpdateVideoSeo/V1/AdminUpdateVideoSeoEndpointV1Tests.cs`
- [ ] `UpdateVideoTags/V1/AdminUpdateVideoTagsEndpointV1Tests.cs`
- [ ] `ScheduleShoot/V1/AdminScheduleShootEndpointV1Tests.cs`
- [ ] `DeleteVideo/V1/AdminDeleteVideoEndpointV1Tests.cs`

---

### Endpoint V1 Record Tests — Admin Short Video Commands

- [ ] `CreateShortVideo/V1/AdminCreateShortVideoEndpointV1Tests.cs`
- [ ] `UploadShortVideoThumbnail/V1/AdminUploadShortVideoThumbnailEndpointV1Tests.cs`
- [ ] `ActivateShortVideo/V1/AdminActivateShortVideoEndpointV1Tests.cs`
- [ ] `DeactivateShortVideo/V1/AdminDeactivateShortVideoEndpointV1Tests.cs`
- [ ] `DeleteShortVideo/V1/AdminDeleteShortVideoEndpointV1Tests.cs`

---

### Endpoint V1 Record Tests — Admin Lyrics Commands

- [ ] `CreateLyrics/V1/AdminCreateLyricsEndpointV1Tests.cs`
- [ ] `UpdateLyrics/V1/AdminUpdateLyricsEndpointV1Tests.cs`
- [ ] `UpdateLyricsSeo/V1/AdminUpdateLyricsSeoEndpointV1Tests.cs`

---

### Endpoint V1 Record Tests — Admin Queries

- [ ] `GetAllArticles/V1/AdminGetAllArticlesEndpointV1Tests.cs`
- [ ] `GetArticleById/V1/AdminGetArticleByIdEndpointV1Tests.cs`
- [ ] `GetAllVideos/V1/AdminGetAllVideosEndpointV1Tests.cs`
- [ ] `GetVideoById/V1/AdminGetVideoByIdEndpointV1Tests.cs`
- [ ] `GetAllShortVideos/V1/AdminGetAllShortVideosEndpointV1Tests.cs`
- [ ] `GetShortVideoById/V1/AdminGetShortVideoByIdEndpointV1Tests.cs`
- [ ] `GetAllLyrics/V1/AdminGetAllLyricsEndpointV1Tests.cs`

---

### Endpoint V1 Record Tests — Public Queries

- [ ] `GetPublishedArticles/V1/PublicGetPublishedArticlesEndpointV1Tests.cs`
- [ ] `GetArticleBySlug/V1/PublicGetArticleBySlugEndpointV1Tests.cs`
- [ ] `GetPromotedArticles/V1/PublicGetPromotedArticlesEndpointV1Tests.cs`
- [ ] `GetPublishedVideos/V1/PublicGetPublishedVideosEndpointV1Tests.cs`
- [ ] `GetVideoBySlug/V1/PublicGetVideoBySlugEndpointV1Tests.cs`
- [ ] `GetPromotedVideos/V1/PublicGetPromotedVideosEndpointV1Tests.cs`
- [ ] `GetPublicShortVideos/V1/PublicGetPublicShortVideosEndpointV1Tests.cs`
- [ ] `GetPublicShortVideoBySlug/V1/PublicGetPublicShortVideoBySlugEndpointV1Tests.cs`
- [ ] `GetLyricsBySlug/V1/PublicGetLyricsBySlugEndpointV1Tests.cs`
