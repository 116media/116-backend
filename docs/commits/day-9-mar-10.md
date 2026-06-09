# Day 9 — March 10, 2026 (49 commits)
## Content shared/domain/infra tests + identity auth V1 endpoint tests + GetOwnRoles + identity user V1

**Start time:** 08:30
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `tests/Unit/Modules/Content/Application/Shared/Validators/SharedValidatorsTests.cs`
```
test(content): add SharedValidators tests for catalog-wide validation rules:

- Verify shared name and slug validators enforce non-empty and max-length rules
- Assert shared Guid validators reject empty Guids with appropriate error messages
```

### 2
**File:** `tests/Unit/Modules/Content/ContentModuleTests.cs`
```
test(content): add ContentModule tests for service registration and seeder wiring:

- Verify ICategoryRepository, ICustomerRepository, IPackageRepository are registered
- Assert ContentTypeSeeder is added to the seeder collection on startup
- Verify migrations are skipped when running in Testing environment
```

### 3
**File:** `tests/Unit/Modules/Content/Domain/Entities/CategoryEntityTests.cs`
```
test(content): add CategoryEntity tests for Create, Update, Activate, and Deactivate:

- Assert Create throws ArgumentException on empty name or slug
- Verify Update changes Name and Slug to provided values correctly
- Assert Activate returns true and Deactivate sets IsActive to false
```

### 4
**File:** `tests/Unit/Modules/Content/Domain/Entities/CategoryPricingEntityTests.cs`
```
test(content): add CategoryPricingEntity tests for Create and Update:

- Assert Create sets CategoryId, PricingTierId, and PriceUsd fields correctly
- Verify Update changes PricingTierId and PriceUsd to provided values
- Assert created entity has correct composite key fields for uniqueness
```

### 5
**File:** `tests/Unit/Modules/Content/Domain/Entities/ContentTypeEntityTests.cs`
```
test(content): add ContentTypeEntity tests for Create, Activate, and Deactivate:

- Assert Create throws ArgumentException on empty or whitespace name
- Verify Activate sets IsActive to true and returns true on success
- Assert Deactivate sets IsActive to false and returns true on success
```

### 6
**File:** `tests/Unit/Modules/Content/Domain/Entities/CustomerEntityTests.cs`
```
test(content): add CustomerEntity tests for Create, Update and validation guards:

- Assert Create throws ArgumentException on empty name or invalid email
- Verify Update changes Name, Email, and Phone to provided values
- Assert created entity has non-empty Id and correct initial field values
```

### 7
**File:** `tests/Unit/Modules/Content/Domain/Entities/PackageEntityTests.cs`
```
test(content): add PackageEntity tests for Create, Update and Activate/Deactivate:

- Assert Create throws ArgumentException on empty name or description
- Verify Update changes Name and Description to provided values
- Assert Activate returns true and Deactivate sets IsActive to false correctly
```

### 8
**File:** `tests/Unit/Modules/Content/Domain/Entities/PackageSlotEntityTests.cs`
```
test(content): add PackageSlotEntity tests for Create and validation guards:

- Assert Create throws ArgumentException on empty slot name or type
- Verify PackageId is correctly assigned on entity creation
- Assert created entity has non-empty Id with expected initial field values
```

### 9
**File:** `tests/Unit/Modules/Content/Domain/Entities/PricingTierEntityTests.cs`
```
test(content): add PricingTierEntity tests for Create, Activate, and Deactivate:

- Assert Create throws ArgumentException on empty or whitespace name
- Verify Activate sets IsActive to true and returns true on success
- Assert Deactivate sets IsActive to false and returns true on success
```

### 10
**File:** `tests/Unit/Modules/Content/Domain/Entities/PromotionLevelEntityTests.cs`
```
test(content): add PromotionLevelEntity tests for Create, Activate, and Deactivate:

- Assert Create throws ArgumentException on empty or whitespace name
- Verify Activate sets IsActive to true and returns true on success
- Assert Deactivate sets IsActive to false and returns true on success
```

### 11
**File:** `tests/Unit/Modules/Content/Domain/Entities/TagEntityTests.cs`
```
test(content): add TagEntity tests for Create and slug validation:

- Assert Create throws ArgumentException on empty or whitespace slug
- Verify Activate sets IsActive to true and returns true on success
- Assert Deactivate sets IsActive to false and slug casing is preserved
```

### 12
**File:** `tests/Unit/Modules/Content/Infrastructure/Persistence/ContentDbContextTests.cs`
```
test(content): add ContentDbContext tests for schema and DbSet registration:

- Verify database schema is set to "content"
- Assert Categories, Customers, Packages, PackageSlots DbSets are configured
- Verify EF Core configurations are applied for all catalog entities
```

### 13
**File:** `tests/Unit/Modules/Content/Infrastructure/Persistence/ContentUnitOfWorkTests.cs`
```
test(content): add ContentUnitOfWork tests for SaveChangesAsync behaviour:

- Verify SaveChangesAsync delegates to ContentDbContext.SaveChangesAsync
- Assert CancellationToken is forwarded correctly on save
```

### 14
**File:** `tests/Unit/Modules/Content/Infrastructure/Repositories/CategoryRepositoryTests.cs`
```
test(content): add CategoryRepository tests for CRUD and ILike search:

- Verify AddAsync persists new category and GetByIdOrThrowAsync retrieves it
- Assert ExistsByNameAsync returns true on case-insensitive ILike name match
- Verify GetAllAsync returns paginated results respecting IsActive filter
```

### 15
**File:** `tests/Unit/Modules/Content/Infrastructure/Repositories/CustomerRepositoryTests.cs`
```
test(content): add CustomerRepository tests for CRUD and ILike email search:

- Verify AddAsync persists new customer and GetByIdOrThrowAsync retrieves it
- Assert ExistsByEmailAsync returns true on case-insensitive ILike email match
- Verify GetAllAsync returns paginated results with correct total count
```

### 16
**File:** `tests/Unit/Modules/Content/Infrastructure/Repositories/LookupRepositoryTests.cs`
```
test(content): add LookupRepository tests for ILike search and active-filter paths:

- Verify GetActiveContentTypesAsync returns only IsActive = true entities
- Assert GetTagByIdOrThrowAsync throws NotFoundException for unknown slug
- Verify ILike name search is case-insensitive across content types and tags
```

### 17
**File:** `tests/Unit/Modules/Content/Infrastructure/Repositories/PackageRepositoryTests.cs`
```
test(content): add PackageRepository tests for CRUD and slot eager-load:

- Verify AddAsync persists new package and GetByIdOrThrowAsync retrieves it
- Assert ExistsByNameAsync returns true on case-insensitive ILike name match
- Verify GetByIdOrThrowAsync includes PackageSlots navigation collection
```

### 18
**File:** `tests/Unit/Modules/Content/Infrastructure/Seeds/ContentTypeSeederTests.cs`
```
test(content): add ContentTypeSeeder tests for idempotent seeding behaviour:

- Verify seeder skips creation when content types already exist
- Assert seeder inserts all predefined content types on first run
- Verify GetActiveContentTypesAsync is called to check existing state
```

### 19
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/ChangePassword/V1/AdminChangePasswordEndpointV1Tests.cs`
```
test(identity): add AdminChangePassword endpoint v1 tests
```

### 20
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/ForgotPassword/V1/AdminForgotPasswordEndpointV1Tests.cs`
```
test(identity): add AdminForgotPassword endpoint v1 tests
```

### 21
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/Login/V1/AdminLoginEndpointV1Tests.cs`
```
test(identity): add AdminLogin endpoint v1 tests
```

### 22
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/ResendOtp/V1/AdminResendOtpEndpointV1Tests.cs`
```
test(identity): add AdminResendOtp endpoint v1 tests
```

### 23
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/ResetPassword/V1/AdminResetPasswordEndpointV1Tests.cs`
```
test(identity): add AdminResetPassword endpoint v1 tests
```

### 24
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/SignOut/V1/AdminSignOutEndpointV1Tests.cs`
```
test(identity): add AdminSignOut endpoint v1 tests
```

### 25
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/SignOutFromAllDevices/V1/AdminSignOutFromAllDevicesEndpointV1Tests.cs`
```
test(identity): add AdminSignOutFromAllDevices endpoint v1 tests
```

### 26
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Admin/Commands/VerifyOtp/V1/AdminVerifyOtpEndpointV1Tests.cs`
```
test(identity): add AdminVerifyOtp endpoint v1 tests
```

### 27
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/ChangePassword/V1/PublicChangePasswordEndpointV1Tests.cs`
```
test(identity): add PublicChangePassword endpoint v1 tests
```

### 28
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/ForgotPassword/V1/PublicForgotPasswordEndpointV1Tests.cs`
```
test(identity): add PublicForgotPassword endpoint v1 tests
```

### 29
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/Login/V1/PublicLoginEndpointV1Tests.cs`
```
test(identity): add PublicLogin endpoint v1 tests
```

### 30
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/ResendOtp/V1/PublicResendOtpEndpointV1Tests.cs`
```
test(identity): add PublicResendOtp endpoint v1 tests
```

### 31
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/ResetPassword/V1/PublicResetPasswordEndpointV1Tests.cs`
```
test(identity): add PublicResetPassword endpoint v1 tests
```

### 32
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/SetPassword/V1/PublicSetPasswordEndpointV1Tests.cs`
```
test(identity): add PublicSetPassword endpoint v1 tests
```

### 33
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/SignOut/V1/PublicSignOutEndpointV1Tests.cs`
```
test(identity): add PublicSignOut endpoint v1 tests
```

### 34
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/SignOutFromAllDevices/V1/PublicSignOutFromAllDevicesEndpointV1Tests.cs`
```
test(identity): add PublicSignOutFromAllDevices endpoint v1 tests
```

### 35
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/SignUp/V1/PublicSignUpEndpointV1Tests.cs`
```
test(identity): add PublicSignUp endpoint v1 tests
```

### 36
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/SocialLogin/V1/PublicSocialLoginEndpointV1Tests.cs`
```
test(identity): add PublicSocialLogin endpoint v1 tests
```

### 37
**File:** `tests/Unit/Modules/Identity/Application/Auth/UseCases/Public/Commands/VerifyOtp/V1/PublicVerifyOtpEndpointV1Tests.cs`
```
test(identity): add PublicVerifyOtp endpoint v1 tests
```

### 38
**File:** `tests/Unit/Modules/Identity/Application/Roles/MetaFields/GetOwnRolesMetaFieldTests.cs`
```
test(identity): add GetOwnRolesMetaField tests for endpoint metadata coverage:

- Assert admin and public GetOwnRoles MetaFields return the expected EndpointNames
- Verify naming conventions are consistent between admin and public route variants
```

### 39
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesHandlerTests.cs`
```
test(identity): add AdminGetOwnRoles handler tests for JWT claims and role mapping:

- Assert user id is extracted from JWT ClaimTypes.NameIdentifier claim
- Assert NotFoundException when extracted user id does not exist
- Verify returned list of RoleDtos maps Name and Description from all assigned roles
```

### 40
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/V1/AdminGetOwnRolesEndpointV1Tests.cs`
```
test(identity): add AdminGetOwnRoles endpoint v1 tests
```

### 41
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesHandlerTests.cs`
```
test(identity): add PublicGetOwnRoles handler tests for JWT claims and role mapping:

- Assert user id is extracted from JWT ClaimTypes.NameIdentifier claim
- Assert NotFoundException when extracted user id does not exist
- Verify returned list of RoleDtos maps Name and Description from all assigned roles
```

### 42
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Commands/RemoveRoleFromUser/V1/AdminRemoveRoleFromUserEndpointV1Tests.cs`
```
test(identity): add AdminRemoveRoleFromUser endpoint v1 tests
```

### 43
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Commands/UpdateAvatar/V1/AdminUpdateAvatarEndpointV1Tests.cs`
```
test(identity): add AdminUpdateAvatar endpoint v1 tests
```

### 44
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Commands/UpdateOwnProfile/V1/AdminUpdateOwnProfileEndpointV1Tests.cs`
```
test(identity): add AdminUpdateOwnProfile endpoint v1 tests
```

### 45
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Queries/GetOwnProfile/V1/AdminGetOwnProfileEndpointV1Tests.cs`
```
test(identity): add AdminGetOwnProfile endpoint v1 tests
```

### 46
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Admin/Queries/GetUserRoles/V1/AdminGetUserRolesEndpointV1Tests.cs`
```
test(identity): add AdminGetUserRoles endpoint v1 tests
```

### 47
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Public/Commands/UpdateAvatar/V1/PublicUpdateAvatarEndpointV1Tests.cs`
```
test(identity): add PublicUpdateAvatar endpoint v1 tests
```

### 48
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Public/Commands/UpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1Tests.cs`
```
test(identity): add PublicUpdateOwnProfile endpoint v1 tests
```

### 49
**File:** `tests/Unit/Modules/Identity/Application/User/UseCases/Public/Queries/GetOwnProfile/V1/PublicGetOwnProfileEndpointV1Tests.cs`
```
test(identity): add PublicGetOwnProfile endpoint v1 tests
```
