# Assertions — Identity / Roles & Permissions

## Key response types
- List: `AdminGetAllRolesResponse` / `AdminGetAllPermissionsResponse`
  (`PaginatedResult<RoleDto>` / `PaginatedResult<PermissionDto>`).
- Get-by-id, create, update → typed response records per endpoint.

## After (create + side-effect)
```csharp
var request = CreateRoleRequestBuilder.Valid();
var response = await Client.PostAsJsonAsync(ApiRoutes.Admin.Roles, request);
response.StatusCode.Should().Be(HttpStatusCode.Created);

var body = await response.ReadAsAsync<AdminCreateRoleResponse>();
body.Role.Name.Should().Be(request.Name);
body.Role.Id.Should().NotBeEmpty();

await using var db = CreateDbContext<IdentityDbContext>();
(await db.Roles.AnyAsync(r => r.Id == body.Role.Id)).Should().BeTrue();
```

## After (list + filter)
```csharp
var body = await response.ReadAsAsync<AdminGetAllRolesResponse>();
body.Roles.Items.Should().Contain(r => r.Id == seeded.Id);
body.Roles.PageIndex.Should().Be(0);
// filter test: every item matches
body.Roles.Items.Should().OnlyContain(r => r.IsActive);
```

State transitions (activate/deactivate/restore/soft-delete/hard-delete,
assign/remove permission) must re-query the DB and assert the flag/relationship
changed. Conflicts (already active, already deleted, duplicate name) →
`ShouldBeProblem(HttpStatusCode.Conflict)`.

## TODO checklist
- [ ] AdminActivatePermissionEndpointV1Tests.cs
- [ ] AdminActivateRoleEndpointV1Tests.cs
- [ ] AdminAssignPermissionToRoleEndpointV1Tests.cs
- [ ] AdminBulkUpdateRolePermissionsEndpointV1Tests.cs
- [ ] AdminCreatePermissionEndpointV1Tests.cs
- [ ] AdminCreateRoleEndpointV1Tests.cs
- [ ] AdminDeactivatePermissionEndpointV1Tests.cs
- [ ] AdminDeactivateRoleEndpointV1Tests.cs
- [ ] AdminGetAllPermissionsEndpointV1Tests.cs
- [ ] AdminGetAllRolesEndpointV1Tests.cs
- [ ] AdminGetOwnRolesEndpointV1Tests.cs
- [ ] AdminGetPermissionByIdEndpointV1Tests.cs
- [ ] AdminGetRoleByIdEndpointV1Tests.cs
- [ ] AdminHardDeletePermissionEndpointV1Tests.cs (already strong — convert to typed)
- [ ] AdminHardDeleteRoleEndpointV1Tests.cs
- [ ] AdminRemovePermissionFromRoleEndpointV1Tests.cs
- [ ] AdminRestorePermissionEndpointV1Tests.cs
- [ ] AdminRestoreRoleEndpointV1Tests.cs
- [ ] AdminSoftDeletePermissionEndpointV1Tests.cs
- [ ] AdminSoftDeleteRoleEndpointV1Tests.cs
- [ ] AdminUpdatePermissionEndpointV1Tests.cs
- [ ] AdminUpdateRoleEndpointV1Tests.cs
- [ ] PublicGetOwnRolesEndpointV1Tests.cs

## Acceptance
- Every mutation re-queries the DB; every list asserts the seeded item + pagination;
  every conflict uses `ShouldBeProblem`.
