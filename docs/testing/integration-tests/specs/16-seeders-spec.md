# Phase 14: Seeders Tests Spec

## Tasks

### ContentTypeSeeder
- [ ] `ContentTypeSeederTests.cs`
  - [ ] SeedAllAsync_ShouldCreateAllContentTypes
  - [ ] SeedAllAsync_ShouldBeIdempotent (run twice, no duplicates)
  - [ ] SeedAllAsync_ShouldCreate4Types (Article, Video, ShortVideo, Lyrics)

### SuperAdminSeeder
- [ ] `SuperAdminSeederTests.cs`
  - [ ] SeedAllAsync_ShouldCreateSuperAdminUser
  - [ ] SeedAllAsync_ShouldCreateAdminRole
  - [ ] SeedAllAsync_ShouldAssignAllPermissions
  - [ ] SeedAllAsync_ShouldBeIdempotent
  - [ ] SeedAllAsync_RolesShouldHaveCorrectPermissionCount (28)

### VisitorRoleSeeder
- [ ] `VisitorRoleSeederTests.cs`
  - [ ] SeedAllAsync_ShouldCreateVisitorRole
  - [ ] SeedAllAsync_ShouldAssignVisitorPermissions
  - [ ] SeedAllAsync_ShouldBeIdempotent

## Test Approach

Seeder tests use `BaseApiTest` to resolve seeders from DI. They need `IConfiguration` with required settings.

```csharp
[Collection("Database")]
public class SuperAdminSeederTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task SeedAllAsync_ShouldCreateSuperAdminUser()
    {
        // Act
        using var scope = Api.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();
        await seeder.SeedAllAsync();

        // Assert
        await using var context = CreateDbContext<IdentityDbContext>();
        var superAdmin = await context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.Value == "superadmin@116.com");

        superAdmin.Should().NotBeNull();
        superAdmin!.UserRoles.Should().ContainSingle();
        superAdmin.UserRoles.First().Role.Name.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task SeedAllAsync_ShouldBeIdempotent()
    {
        using var scope = Api.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<SuperAdminSeeder>();

        await seeder.SeedAllAsync();
        await seeder.SeedAllAsync(); // second run should not throw

        await using var context = CreateDbContext<IdentityDbContext>();
        var admins = await context.Users
            .Where(u => u.Email.Value == "superadmin@116.com")
            .ToListAsync();
        admins.Should().ContainSingle();
    }
}
```

## File Locations

```
tests/_116.Integration.Tests/Content/Seeders/
└── ContentTypeSeederTests.cs

tests/_116.Integration.Tests/Identity/Seeders/
├── SuperAdminSeederTests.cs
└── VisitorRoleSeederTests.cs
```

## Acceptance Criteria

1. Every seeder's `SeedAllAsync` verified against real database
2. Idempotency verified (double-run produces no duplicates)
3. Navigation property queries use junction table pattern
4. Exact entity counts verified
5. `./scripts/run-tests-with-coverage.sh integration` passes
