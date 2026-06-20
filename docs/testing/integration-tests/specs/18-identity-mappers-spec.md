# Phase 16: Identity Module — Mapper Tests Spec

## Tasks

### RoleMapper
- [ ] `RoleMapperTests.cs`
  - [ ] ToRoleDtoAsync_ShouldMapAllFields
  - [ ] ToRoleDtoAsync_WithPermissions_ShouldIncludePermissions
  - [ ] ToRoleSummaryDtosAsync_ShouldMapCollection

### SessionMapper
- [ ] `SessionMapperTests.cs`
  - [ ] ToSessionDtoAsync_ShouldMapAllFields
  - [ ] ToSessionDtoAsync_WithMetadata_ShouldIncludeDeviceInfo
  - [ ] ToSessionSummaryDtosAsync_ShouldMapCollection

### UserMapper
- [ ] `UserMapperTests.cs`
  - [ ] ToUserDtoAsync_ShouldMapAllFields
  - [ ] ToUserDtoAsync_WithRoles_ShouldIncludeRoles
  - [ ] ToUserDtoAsync_WithAvatar_ShouldIncludeAvatarUrl
  - [ ] ToProfileDtoAsync_ShouldMapPublicFields

## Test Approach

Same as Content mappers — use `BaseApiTest`, resolve mapper from DI, seed entities, verify mapping.

```csharp
[Collection("Database")]
public class UserMapperTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ToUserDtoAsync_WithRoles_ShouldIncludeRoles()
    {
        // Arrange — seed user with role
        await using var context = CreateDbContext<IdentityDbContext>();
        var role = RoleEntity.Create(/* ... */);
        context.Roles.Add(role);
        var user = new UserEntityBuilder().Build();
        context.Users.Add(user);
        var userRole = UserRoleEntity.Create(user.Id, role.Id);
        context.UserRoles.Add(userRole);
        await context.SaveChangesAsync();

        // Reload with includes
        await using var queryContext = CreateDbContext<IdentityDbContext>();
        var loaded = await queryContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id);

        // Act
        using var scope = Api.Services.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<UserMapper>();
        var dto = await mapper.ToUserDtoAsync(loaded);

        // Assert
        dto.Id.Should().Be(user.Id);
        dto.Roles.Should().ContainSingle();
        dto.Roles.First().Name.Should().Be(role.Name);
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Identity/Mappers/
├── RoleMapperTests.cs
├── SessionMapperTests.cs
└── UserMapperTests.cs
```

## Acceptance Criteria

1. Every mapper method has at least one integration test
2. Junction table navigation (UserRoles, RolePermissions) verified
3. File URL resolution verified for avatar
4. `./scripts/run-tests-with-coverage.sh integration` passes
