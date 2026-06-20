# Phase 3: Identity Module — Repository Tests Spec

## Tasks

### AuthRepository
- [ ] `AuthRepositoryTests.cs`
  - [ ] GetByEmailAsync_ExistingUser_ShouldReturnUser
  - [ ] GetByEmailAsync_NonExistent_ShouldReturnNull
  - [ ] GetByIdOrThrowAsync_ExistingUser_ShouldReturnUser
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrowNotFoundException
  - [ ] CreateAsync_ValidUser_ShouldPersist
  - [ ] UpdateAsync_ExistingUser_ShouldUpdateFields
  - [ ] ExistsByEmailAsync_ExistingEmail_ShouldReturnTrue
  - [ ] ExistsByEmailAsync_NonExistent_ShouldReturnFalse
  - [ ] ExistsByUserNameAsync_ExistingUserName_ShouldReturnTrue

### OtpRepository
- [ ] `OtpRepositoryTests.cs`
  - [ ] CreateAsync_ValidOtp_ShouldPersist
  - [ ] GetActiveByUserIdAsync_WithActiveOtp_ShouldReturn
  - [ ] GetActiveByUserIdAsync_WithExpiredOtp_ShouldReturnNull
  - [ ] InvalidateAsync_ShouldMarkOtpAsUsed
  - [ ] GetAttemptCountAsync_ShouldReturnCorrectCount

### PermissionRepository
- [ ] `PermissionRepositoryTests.cs`
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] CreateAsync_ValidPermission_ShouldPersist
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] SoftDeleteAsync_ShouldSetDeletedAt
  - [ ] RestoreAsync_ShouldClearDeletedAt
  - [ ] HardDeleteAsync_ShouldRemoveFromDatabase
  - [ ] ExistsByNameAsync_ShouldReturnCorrectResult
  - [ ] ActivateAsync_ShouldSetIsActiveTrue
  - [ ] DeactivateAsync_ShouldSetIsActiveFalse

### RolePermissionRepository
- [ ] `RolePermissionRepositoryTests.cs`
  - [ ] AssignAsync_ShouldCreateJunctionRecord
  - [ ] RemoveAsync_ShouldDeleteJunctionRecord
  - [ ] GetPermissionsByRoleIdAsync_ShouldReturnPermissions
  - [ ] BulkUpdateAsync_ShouldReplaceAllPermissions
  - [ ] ExistsAsync_ShouldReturnCorrectResult

### RoleRepository
- [ ] `RoleRepositoryTests.cs`
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] CreateAsync_ValidRole_ShouldPersist
  - [ ] UpdateAsync_ShouldUpdateFields
  - [ ] SoftDeleteAsync_ShouldSetDeletedAt
  - [ ] RestoreAsync_ShouldClearDeletedAt
  - [ ] HardDeleteAsync_ShouldRemoveFromDatabase
  - [ ] ExistsByNameAsync_ShouldReturnCorrectResult
  - [ ] ActivateAsync_ShouldSetIsActiveTrue
  - [ ] DeactivateAsync_ShouldSetIsActiveFalse
  - [ ] GetByIdWithPermissionsAsync_ShouldIncludeRolePermissions

### SessionRepository
- [ ] `SessionRepositoryTests.cs`
  - [ ] CreateAsync_ValidSession_ShouldPersist
  - [ ] GetByIdOrThrowAsync_Existing_ShouldReturn
  - [ ] GetByIdOrThrowAsync_NonExistent_ShouldThrow
  - [ ] GetByUserIdAsync_ShouldReturnUserSessions
  - [ ] GetAllAsync_ShouldReturnPaginated
  - [ ] RevokeAsync_ShouldSetRevokedAt
  - [ ] CleanupExpiredAsync_ShouldDeleteExpiredSessions
  - [ ] GetMetricsAsync_ShouldReturnCorrectCounts

### UserRoleRepository
- [ ] `UserRoleRepositoryTests.cs`
  - [ ] AssignAsync_ShouldCreateJunctionRecord
  - [ ] RemoveAsync_ShouldDeleteJunctionRecord
  - [ ] GetRolesByUserIdAsync_ShouldReturnRoles
  - [ ] ExistsAsync_ShouldReturnCorrectResult

## Test Approach

All repository tests use `BaseRepositoryTest` with `CreateDbContext<IdentityDbContext>()`.

Seeding pattern:
```csharp
await using var context = CreateDbContext<IdentityDbContext>();
var user = new UserEntityBuilder().Build();
context.Users.Add(user);
await context.SaveChangesAsync();
```

Query with separate context to avoid EF cache:
```csharp
await using var verifyContext = CreateDbContext<IdentityDbContext>();
var result = await verifyContext.Users.FindAsync(user.Id);
```

## File Locations

```
tests/_116.Integration.Tests/Identity/Repositories/
├── AuthRepositoryTests.cs
├── OtpRepositoryTests.cs
├── PermissionRepositoryTests.cs
├── RolePermissionRepositoryTests.cs
├── RoleRepositoryTests.cs
├── SessionRepositoryTests.cs
└── UserRoleRepositoryTests.cs
```

## Acceptance Criteria

1. Every public method on each repository has at least one happy-path and one error-path test
2. Navigation property queries use `.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)` pattern
3. All assertions use exact values, not `.NotBeNull()` alone
4. `./scripts/run-tests-with-coverage.sh integration` passes
