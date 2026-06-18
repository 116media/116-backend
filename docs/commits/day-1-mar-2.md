# Day 1 — March 2, 2026 (57 commits)
## Identity source changes

**Start time:** 08:30
**Gap between commits:** random 10–20 min + random seconds
**Env vars:** `GIT_AUTHOR_DATE` and `GIT_COMMITTER_DATE` per commit
**Co-authored-by:** never

---

## Commits in order

### 1
**File:** `src/Modules/Identity/Identity/Domain/Constants/IdentityConstants.cs`
```
feat(identity): add Me route segment constant
```

### 2
**File:** `src/Modules/Identity/Identity/Application/User/Constants/UserRouteConstants.cs`
```
refactor(identity): update UserRouteConstants with me-prefixed paths
```

### 3
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/SignUp/V1/PublicSignUpEndpointV1.cs`
```
refactor(identity): rename endpoint version segment to uppercase V1
```

### 4
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Admin/Commands/ForgotPassword/V1/AdminForgotPasswordEndpointV1.cs`
```
refactor(identity): align admin forgot-password endpoint with versioning convention
```

### 5
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/ForgotPassword/V1/PublicForgotPasswordEndpointV1.cs`
```
refactor(identity): align public forgot-password endpoint with versioning convention
```

### 6
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/ResendOtp/V1/PublicResendOtpEndpointV1.cs`
```
refactor(identity): align public resend-otp endpoint with versioning convention
```

### 7
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/SignOut/V1/PublicSignOutEndpointV1.cs`
```
refactor(identity): align public sign-out endpoint with versioning convention
```

### 8
**File:** `src/Modules/Identity/Identity/Application/Auth/UseCases/Public/Commands/SignOutFromAllDevices/V1/PublicSignOutFromAllDevicesEndpointV1.cs`
```
refactor(identity): align sign-out-all-devices endpoint with versioning convention
```

### 9
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/BulkUpdateRolePermissions/AdminBulkUpdateRolePermissionsHandler.cs`
```
refactor(identity): use pattern matching and IsSuccess in bulk-update handler:

- Replace explicit boolean checks with switch expression pattern matching
- Rename result property to IsSuccess for consistency
```

### 10
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeletePermission/AdminHardDeletePermissionCommand.cs`
```
refactor(identity): rename result field to IsSuccess in HardDeletePermission command
```

### 11
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeletePermission/AdminHardDeletePermissionHandler.cs`
```
refactor(identity): use pattern matching in HardDeletePermission handler:

- Switch to switch expression on the permission entity result
- Guard against deleting system-protected permissions permanently
```

### 12
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeletePermission/V1/AdminHardDeletePermissionEndpointV1.cs`
```
refactor(identity): update hard-delete-permission endpoint route metadata
```

### 13
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeleteRole/AdminHardDeleteRoleCommand.cs`
```
refactor(identity): rename result field to IsSuccess in HardDeleteRole command
```

### 14
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeleteRole/AdminHardDeleteRoleHandler.cs`
```
refactor(identity): use pattern matching in HardDeleteRole handler:

- Switch to switch expression on role entity result
- Guard core roles from permanent deletion
- Improve guard clause readability
```

### 15
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/HardDeleteRole/V1/AdminHardDeleteRoleEndpointV1.cs`
```
refactor(identity): update hard-delete-role endpoint route metadata
```

### 16
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/RemovePermissionFromRole/AdminRemovePermissionFromRoleCommand.cs`
```
refactor(identity): rename result field to IsSuccess in RemovePermissionFromRole command
```

### 17
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/RemovePermissionFromRole/AdminRemovePermissionFromRoleHandler.cs`
```
refactor(identity): use pattern matching in RemovePermissionFromRole handler:

- Switch to switch expression on role-permission removal result
- Guard against removing permissions from core protected roles
```

### 18
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/RemovePermissionFromRole/V1/AdminRemovePermissionFromRoleEndpointV1.cs`
```
refactor(identity): update remove-permission-from-role endpoint route metadata
```

### 19
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeletePermission/AdminSoftDeletePermissionCommand.cs`
```
refactor(identity): rename result field to IsSuccess in SoftDeletePermission command
```

### 20
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeletePermission/AdminSoftDeletePermissionHandler.cs`
```
refactor(identity): use pattern matching in SoftDeletePermission handler:

- Switch to switch expression on permission soft-delete result
- Improve guard clause for already-deleted permissions
```

### 21
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeletePermission/V1/AdminSoftDeletePermissionEndpointV1.cs`
```
refactor(identity): update soft-delete-permission endpoint route metadata
```

### 22
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeleteRole/AdminSoftDeleteRoleCommand.cs`
```
refactor(identity): rename result field to IsSuccess in SoftDeleteRole command
```

### 23
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeleteRole/AdminSoftDeleteRoleHandler.cs`
```
refactor(identity): use pattern matching in SoftDeleteRole handler:

- Switch to switch expression on role soft-delete result
- Protect core roles from soft deletion
```

### 24
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Commands/SoftDeleteRole/V1/AdminSoftDeleteRoleEndpointV1.cs`
```
refactor(identity): update soft-delete-role endpoint route metadata
```

### 25
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetAllRoles/V1/AdminGetAllRolesEndpointV1.cs`
```
refactor(identity): update get-all-roles endpoint with versioning convention
```

### 26
**File:** `src/Modules/Identity/Identity/Application/Session/UseCases/Public/Commands/RevokeSession/V1/PublicRevokeSessionEndpointV1.cs`
```
refactor(identity): align revoke-session endpoint with versioning convention
```

### 27
**File:** `src/Modules/Identity/Identity/Application/Session/UseCases/Public/Queries/GetOwnSessionById/V1/PublicGetOwnSessionByIdEndpointV1.cs`
```
refactor(identity): align get-own-session-by-id endpoint with versioning convention
```

### 28
**File:** `src/Modules/Identity/Identity/Application/Session/UseCases/Public/Queries/GetOwnSessions/V1/PublicGetOwnSessionsEndpointV1.cs`
```
refactor(identity): align get-own-sessions endpoint with versioning convention
```

### 29
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/RemoveRoleFromUser/AdminRemoveRoleFromUserCommand.cs`
```
refactor(identity): rename result field to IsSuccess in RemoveRoleFromUser command
```

### 30
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/RemoveRoleFromUser/AdminRemoveRoleFromUserHandler.cs`
```
refactor(identity): use pattern matching in RemoveRoleFromUser handler:

- Switch to switch expression on user-role removal result
- Guard against removing roles from protected accounts
```

### 31
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/RemoveRoleFromUser/V1/AdminRemoveRoleFromUserEndpointV1.cs`
```
refactor(identity): update remove-role-from-user endpoint route metadata
```

### 32
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/UpdateAvatar/V1/AdminUpdateAvatarEndpointV1.cs`
```
refactor(identity): align admin update-avatar endpoint with Me route prefix
```

### 33
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Commands/UpdateOwnProfile/V1/AdminUpdateOwnProfileEndpointV1.cs`
```
refactor(identity): align admin update-own-profile endpoint with Me route prefix
```

### 34
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Admin/Queries/GetOwnProfile/V1/AdminGetOwnProfileEndpointV1.cs`
```
refactor(identity): align admin get-own-profile endpoint with Me route prefix
```

### 35
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Public/Commands/UpdateAvatar/V1/PublicUpdateAvatarEndpointV1.cs`
```
refactor(identity): align public update-avatar endpoint with Me route prefix
```

### 36
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Public/Commands/UpdateOwnProfile/V1/PublicUpdateOwnProfileEndpointV1.cs`
```
refactor(identity): align public update-own-profile endpoint with Me route prefix
```

### 37
**File:** `src/Modules/Identity/Identity/Application/User/UseCases/Public/Queries/GetOwnProfile/V1/PublicGetOwnProfileEndpointV1.cs`
```
refactor(identity): align public get-own-profile endpoint with Me route prefix
```

### 38
**File:** `src/Modules/Identity/Identity/IdentityModule.cs`
```
refactor(identity): disable migrations in Testing env alongside seeding:

- Change EnableMigrations to follow the same enableSeeding flag
- Prevents InvalidOperationException from MigrateAsync on InMemory DB
```

### 39
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesQuery.cs`
```
feat(identity): add AdminGetOwnRoles query record
```

### 40
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesHandler.cs`
```
feat(identity): add AdminGetOwnRoles query handler:

- Read current admin user id from JWT claims in HTTP context
- Query role repository using the extracted user identifier
- Return list of RoleDtos mapped via Mapster
```

### 41
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/AdminGetOwnRolesMetaField.cs`
```
feat(identity): add AdminGetOwnRoles route metadata
```

### 42
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Admin/Queries/GetOwnRoles/V1/AdminGetOwnRolesEndpointV1.cs`
```
feat(identity): add GET /api/v1/admin/me/roles endpoint
```

### 43
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesQuery.cs`
```
feat(identity): add PublicGetOwnRoles query record
```

### 44
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesHandler.cs`
```
feat(identity): add PublicGetOwnRoles query handler:

- Extract authenticated user id from JWT claims
- Query role repository for roles assigned to that user
- Return list of RoleDtos mapped via Mapster
```

### 45
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/PublicGetOwnRolesMetaField.cs`
```
feat(identity): add PublicGetOwnRoles route metadata
```

### 46
**File:** `src/Modules/Identity/Identity/Application/Roles/UseCases/Public/Queries/GetOwnRoles/V1/PublicGetOwnRolesEndpointV1.cs`
```
feat(identity): add GET /api/v1/public/me/roles endpoint
```

### 47
**File:** `tests/Fixtures/Constants/TestConstants.cs`
```
test(fixture): extend TestConstants with content catalog test data:

- Add nested Content static class with category, customer, package constants
- Add CategoryPricing and PackageSlot sub-classes for catalog unit tests
```

### 48
**File:** `tests/Fixtures/_116.Tests.Fixtures.csproj`
```
build(fixture): add content module project reference to fixtures
```

### 49
**File:** `tests/Unit/Modules/Identity/Application/Roles/Specifications/UserRoleSpecificationsTests.cs`
```
test(identity): add tests for UserRole specification predicates
```

### 50
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/HardDeletePermission/AdminHardDeletePermissionHandlerTests.cs`
```
test(identity): update HardDeletePermission handler tests for IsSuccess rename:

- Adjust assertions to use renamed IsSuccess property
- Add test cases for protected-permission guard behaviour
```

### 51
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/HardDeleteRole/AdminHardDeleteRoleHandlerTests.cs`
```
test(identity): update HardDeleteRole handler tests for IsSuccess rename:

- Adjust assertions to use renamed IsSuccess property
- Add coverage for core-role guard and not-found scenarios
```

### 52
**File:** `tests/Unit/Modules/Identity/Application/Roles/UseCases/Admin/Commands/UpdatePermission/AdminUpdatePermissionHandlerTests.cs`
```
test(identity): add missing coverage for UpdatePermission handler edge cases
```

### 53
**File:** `tests/Unit/Modules/Identity/Domain/Entities/RoleEntityTests.cs`
```
test(identity): expand RoleEntity tests with SoftDelete and Restore coverage
```

### 54
**File:** `tests/Unit/Modules/Identity/Domain/Entities/UserEntityTests.cs`
```
test(identity): add UserEntity tests for profile update and avatar change
```

### 55
**File:** `tests/Unit/Modules/Identity/Domain/Entities/UserRoleEntityTests.cs`
```
test(identity): add UserRoleEntity creation and equality tests
```

### 56
**File:** `tests/Unit/Modules/Identity/IdentityModuleTests.cs`
```
test(identity): add UseIdentityModule test for Testing env early-return:

- Mock IApplicationBuilder with Moq
- Assert module returns early without calling MigrateAsync
- Verify returned builder is the same instance
```

### 57
**File:** `tests/Unit/Modules/Identity/Infrastructure/Repositories/AuthRepositoryTests.cs`
```
test(identity): expand AuthRepository tests with session lookup scenarios
```