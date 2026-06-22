# Assertions — Identity / User

## Key response types
- Profile get/update → `UserResponseDto`-bearing response (assert echoed
  firstName/lastName/phone, etc.).
- Avatar upload → response with the new avatar URL (stubbed Cloudinary returns a
  deterministic `https://res.cloudinary.com/test-cloud/...` URL — assert it).
- Assign/remove role, get user roles → role list / side-effect.

## After (update profile — echo + side-effect)
```csharp
var request = UpdateOwnProfileRequestBuilder.Valid();
var response = await Client.PatchAsJsonAsync(Routes.Public.Me.Profile(), request);
response.StatusCode.Should().Be(HttpStatusCode.OK);

var body = await response.ReadAsAsync<PublicUpdateOwnProfileResponse>();
body.User.FirstName.Should().Be(request.FirstName);

await using var db = CreateDbContext<IdentityDbContext>();
var user = await db.Users.FindAsync(TestUser.VisitorId);
user!.FirstName.Should().Be(request.FirstName);
```

## After (assign role — side-effect, currently status-only)
```csharp
// after POST .../{id}/roles
await using var db = CreateDbContext<IdentityDbContext>();
(await db.UserRoles.AnyAsync(ur => ur.UserId == TestUser.AdminId && ur.RoleId == role.Id))
    .Should().BeTrue();
```

Avatar upload uses multipart; assert the returned URL and that the user's avatar
column was updated. Validation (no file/oversized/wrong type) → `ShouldBeProblem`.

## TODO checklist
- [ ] AdminAssignRoleToUserEndpointV1Tests.cs
- [ ] AdminGetOwnProfileEndpointV1Tests.cs
- [ ] AdminGetUserRolesEndpointV1Tests.cs
- [ ] AdminRemoveRoleFromUserEndpointV1Tests.cs
- [ ] AdminUpdateAvatarEndpointV1Tests.cs
- [ ] AdminUpdateOwnProfileEndpointV1Tests.cs
- [ ] PublicGetOwnProfileEndpointV1Tests.cs
- [ ] PublicUpdateAvatarEndpointV1Tests.cs
- [ ] PublicUpdateOwnProfileEndpointV1Tests.cs

## Acceptance
- Profile/role mutations verify DB state; avatar tests assert the returned URL.
