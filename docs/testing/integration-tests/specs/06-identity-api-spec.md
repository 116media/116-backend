# Phase 4: Identity Module — API Tests Spec

## Tasks

### Auth — Admin Commands (8)
- [ ] `AdminLoginEndpointTests.cs`
  - [ ] Post_WithValidCredentials_ShouldReturn200WithTokens
  - [ ] Post_WithInvalidPassword_ShouldReturn401
  - [ ] Post_WithNonExistentEmail_ShouldReturn401
- [ ] `AdminChangePasswordEndpointTests.cs`
  - [ ] Post_AsAdmin_WithCorrectOldPassword_ShouldReturn200
  - [ ] Post_AsAdmin_WithWrongOldPassword_ShouldReturn400
  - [ ] Post_WithoutAuth_ShouldReturn401
- [ ] `AdminForgotPasswordEndpointTests.cs`
  - [ ] Post_WithExistingEmail_ShouldReturn200
  - [ ] Post_WithNonExistentEmail_ShouldReturn200 (no information leak)
- [ ] `AdminResetPasswordEndpointTests.cs`
  - [ ] Post_WithValidOtp_ShouldReturn200
  - [ ] Post_WithExpiredOtp_ShouldReturn400
- [ ] `AdminResendOtpEndpointTests.cs`
  - [ ] Post_WithPendingOtp_ShouldReturn200
- [ ] `AdminVerifyOtpEndpointTests.cs`
  - [ ] Post_WithValidOtp_ShouldReturn200
  - [ ] Post_WithInvalidOtp_ShouldReturn400
- [ ] `AdminSignOutEndpointTests.cs`
  - [ ] Post_AsAdmin_ShouldReturn200
  - [ ] Post_WithoutAuth_ShouldReturn401
- [ ] `AdminSignOutFromAllDevicesEndpointTests.cs`
  - [ ] Post_AsAdmin_ShouldReturn200

### Auth — Public Commands (11)
- [ ] `PublicLoginEndpointTests.cs`
  - [ ] Post_WithValidCredentials_ShouldReturn200WithTokens
  - [ ] Post_WithInvalidPassword_ShouldReturn401
  - [ ] Post_WithUnverifiedAccount_ShouldReturn403
- [ ] `PublicSignUpEndpointTests.cs`
  - [ ] Post_WithValidData_ShouldReturn201
  - [ ] Post_WithExistingEmail_ShouldReturn409
  - [ ] Post_WithInvalidEmail_ShouldReturn422
- [ ] `PublicChangePasswordEndpointTests.cs`
- [ ] `PublicForgotPasswordEndpointTests.cs`
- [ ] `PublicResetPasswordEndpointTests.cs`
- [ ] `PublicResendOtpEndpointTests.cs`
- [ ] `PublicVerifyOtpEndpointTests.cs`
- [ ] `PublicSetPasswordEndpointTests.cs`
- [ ] `PublicSignOutEndpointTests.cs`
- [ ] `PublicSignOutFromAllDevicesEndpointTests.cs`
- [ ] `PublicSocialLoginEndpointTests.cs`

### Roles — Admin Commands (14)
- [ ] `AdminCreateRoleEndpointTests.cs`
  - [ ] Post_AsSuperAdmin_ShouldReturn201
  - [ ] Post_AsAdmin_ShouldReturn403
  - [ ] Post_WithDuplicateName_ShouldReturn409
  - [ ] Post_WithInvalidData_ShouldReturn422
- [ ] `AdminUpdateRoleEndpointTests.cs`
- [ ] `AdminActivateRoleEndpointTests.cs`
- [ ] `AdminDeactivateRoleEndpointTests.cs`
- [ ] `AdminSoftDeleteRoleEndpointTests.cs`
- [ ] `AdminRestoreRoleEndpointTests.cs`
- [ ] `AdminHardDeleteRoleEndpointTests.cs`
- [ ] `AdminCreatePermissionEndpointTests.cs`
- [ ] `AdminUpdatePermissionEndpointTests.cs`
- [ ] `AdminActivatePermissionEndpointTests.cs`
- [ ] `AdminDeactivatePermissionEndpointTests.cs`
- [ ] `AdminSoftDeletePermissionEndpointTests.cs`
- [ ] `AdminRestorePermissionEndpointTests.cs`
- [ ] `AdminHardDeletePermissionEndpointTests.cs`
- [ ] `AdminAssignPermissionToRoleEndpointTests.cs`
- [ ] `AdminRemovePermissionFromRoleEndpointTests.cs`
- [ ] `AdminBulkUpdateRolePermissionsEndpointTests.cs`

### Roles — Admin Queries (8)
- [ ] `AdminGetAllRolesEndpointTests.cs`
  - [ ] Get_AsSuperAdmin_ShouldReturn200WithPaginatedRoles
  - [ ] Get_AsVisitor_ShouldReturn403
- [ ] `AdminGetRoleByIdEndpointTests.cs`
- [ ] `AdminGetOwnRolesEndpointTests.cs`
- [ ] `AdminGetAllPermissionsEndpointTests.cs`
- [ ] `AdminGetPermissionByIdEndpointTests.cs`

### Roles — Public Queries (2)
- [ ] `PublicGetOwnRolesEndpointTests.cs`

### Session — Admin Commands (4)
- [ ] `AdminRefreshTokenEndpointTests.cs`
- [ ] `AdminRevokeSessionEndpointTests.cs`
- [ ] `AdminCleanupExpiredSessionsEndpointTests.cs`
- [ ] `AdminForceLogoutUserEndpointTests.cs`

### Session — Admin Queries (5)
- [ ] `AdminGetAllSessionsEndpointTests.cs`
- [ ] `AdminGetOwnSessionsEndpointTests.cs`
- [ ] `AdminGetOwnSessionByIdEndpointTests.cs`
- [ ] `AdminExportSessionDataEndpointTests.cs`
- [ ] `AdminGetSessionMetricsEndpointTests.cs`

### Session — Public Commands (2)
- [ ] `PublicRefreshTokenEndpointTests.cs`
- [ ] `PublicRevokeSessionEndpointTests.cs`

### Session — Public Queries (2)
- [ ] `PublicGetOwnSessionsEndpointTests.cs`
- [ ] `PublicGetOwnSessionByIdEndpointTests.cs`

### User — Admin Commands (4)
- [ ] `AdminAssignRoleToUserEndpointTests.cs`
- [ ] `AdminRemoveRoleFromUserEndpointTests.cs`
- [ ] `AdminUpdateAvatarEndpointTests.cs`
- [ ] `AdminUpdateOwnProfileEndpointTests.cs`

### User — Admin Queries (2)
- [ ] `AdminGetOwnProfileEndpointTests.cs`
- [ ] `AdminGetUserRolesEndpointTests.cs`

### User — Public Commands (2)
- [ ] `PublicUpdateAvatarEndpointTests.cs`
- [ ] `PublicUpdateOwnProfileEndpointTests.cs`

### User — Public Queries (1)
- [ ] `PublicGetOwnProfileEndpointTests.cs`

## Test Pattern

Every API test class follows this structure:

```csharp
[Collection("Database")]
public class AdminCreateRoleEndpointTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task Post_AsSuperAdmin_WithValidData_ShouldReturn201()
    {
        // Arrange
        Client.AuthenticateAsSuperAdmin();
        var request = new CreateRoleRequestBuilder().Build();

        // Act
        HttpResponseMessage response = await Client.PostAsJsonAsync(
            $"{ApiRoutes.Admin.Roles}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var context = CreateDbContext<IdentityDbContext>();
        var role = await context.Roles.FirstAsync(r => r.Name == request.Name);
        role.Should().NotBeNull();
        role.Name.Should().Be(request.Name);
    }
}
```

Each endpoint must test:
1. **Happy path** — correct status code + correct DB state
2. **Auth** — 401 without token, 403 with wrong role
3. **Validation** — 422 with invalid payload
4. **Business rules** — 409 for duplicates, 404 for missing entities

## File Locations

```
tests/_116.Integration.Tests/Identity/Api/
├── Auth/
│   ├── AdminLoginEndpointTests.cs
│   ├── PublicLoginEndpointTests.cs
│   ├── PublicSignUpEndpointTests.cs
│   └── ... (19 total)
├── Roles/
│   ├── AdminCreateRoleEndpointTests.cs
│   ├── AdminGetAllRolesEndpointTests.cs
│   └── ... (24 total)
├── Session/
│   ├── AdminRefreshTokenEndpointTests.cs
│   ├── PublicRefreshTokenEndpointTests.cs
│   └── ... (13 total)
└── User/
    ├── AdminAssignRoleToUserEndpointTests.cs
    ├── PublicGetOwnProfileEndpointTests.cs
    └── ... (9 total)
```

## Acceptance Criteria

1. Every Identity endpoint has at least one integration test
2. Auth matrix (Anonymous/Visitor/Admin/SuperAdmin) is verified for each endpoint
3. All tests use synchronous auth helpers
4. `./scripts/run-tests-with-coverage.sh integration` passes
