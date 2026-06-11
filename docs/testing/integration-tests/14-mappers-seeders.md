# Mappers & Seeders Integration Tests

## Why These Need Integration Tests

### Mappers (11 untested out of 15)

Mappers convert EF Core entities (with navigation properties, file references, nested collections) into DTOs. Unit tests for 4 mappers exist, but most are untested because:

- Navigation properties must be loaded via `Include()` to avoid null references
- File URL resolution may depend on data from the `core.files` table (cross-schema)
- Async mappers (`ToCategoryDtoAsync`, `ToVideoSummaryDtosAsync`) call `IFileRepository` to resolve poster/thumbnail URLs
- Round-trip correctness (entity → DTO → verify all fields) needs real data with realistic relationships

### Seeders (2 untested out of 3)

- `SuperAdminSeeder` — creates the initial SuperAdmin user with role, password hash, and claims. Unit test was **skipped** due to EF Core change tracking issues.
- `VisitorRoleSeeder` — creates the Visitor role with default permissions. Unit test was **skipped** for the same reason.
- `ContentTypeSeeder` — has 1 unit test, but integration test verifies idempotency and correct DB state.

## Mapper Integration Tests

### Approach

1. Seed a real entity with all navigation properties into PostgreSQL
2. Load it back with `Include()` chains (same as the repository does)
3. Call the mapper method
4. Assert every DTO field matches the source entity

### Base Pattern

```csharp
[Collection("Database")]
public class CategoryMapperTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task ToCategoryDto_ShouldMapAllFields()
    {
        // Arrange — seed a category with content type and poster
        await using var seedContext = CreateDbContext<ContentDbContext>();

        var contentType = new ContentTypeEntityBuilder()
            .WithName("Video")
            .Build();
        seedContext.ContentTypes.Add(contentType);

        var category = new CategoryEntityBuilder()
            .WithContentType(contentType)
            .WithIsActive(true)
            .Build();
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act — load with includes (same as repository)
        await using var readContext = CreateDbContext<ContentDbContext>();
        CategoryEntity loaded = await readContext.Categories
            .Include(c => c.ContentType)
            .FirstAsync(c => c.Id == category.Id);

        CategoryDto dto = loaded.ToCategoryDto();

        // Assert
        dto.Id.Should().Be(loaded.Id);
        dto.Name.Should().Be(loaded.Name);
        dto.Slug.Should().Be(loaded.Slug);
        dto.Description.Should().Be(loaded.Description);
        dto.IsActive.Should().Be(loaded.IsActive);
        dto.ContentTypeName.Should().Be("Video");
    }

    [Fact]
    public async Task ToCategoryDto_WithNullPoster_ShouldMapWithoutError()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();

        var contentType = new ContentTypeEntityBuilder().Build();
        seedContext.ContentTypes.Add(contentType);

        var category = new CategoryEntityBuilder()
            .WithContentType(contentType)
            .WithPoster(null)
            .Build();
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act
        await using var readContext = CreateDbContext<ContentDbContext>();
        CategoryEntity loaded = await readContext.Categories
            .Include(c => c.ContentType)
            .FirstAsync(c => c.Id == category.Id);

        CategoryDto dto = loaded.ToCategoryDto();

        // Assert
        dto.PosterUrl.Should().BeNull();
    }
}
```

### Async Mapper Tests (File Resolution)

Some mappers call `IFileRepository` to resolve file URLs. These need the full DI container via `BaseApiTest`:

```csharp
[Collection("Database")]
public class VideoMapperTests(PostgresFixture db) : BaseApiTest(db)
{
    [Fact]
    public async Task ToVideoSummaryDtosAsync_ShouldResolveFileUrls()
    {
        // Arrange — seed video with associated file
        await using var context = CreateDbContext<ContentDbContext>();
        var video = new VideoEntityBuilder()
            .WithThumbnailFileId(Guid.NewGuid())
            .Build();
        context.Videos.Add(video);
        await context.SaveChangesAsync();

        // Act — call the async mapper with IFileRepository from DI
        // Assert — verify thumbnail URL is resolved
    }
}
```

### Full Mapper Coverage List

| Mapper | Method(s) | Key Assertions |
|--------|----------|----------------|
| `CategoryMapper` | `ToCategoryDto`, `ToCategoryDtoAsync` | Content type name, poster URL, active flag |
| `VideoMapper` | `ToVideoDto`, `ToVideoSummaryDto`, `ToVideoSummaryDtosAsync` | Category, tags, thumbnail, duration, YouTube URL |
| `ArticleMapper` | `ToArticleDto`, `ToArticleSummaryDto` | Tags, poster, reading time, content body |
| `CustomerMapper` | `ToCustomerDto` | User email (cross-schema from Identity), display name |
| `ContentTypeMapper` | `ToContentTypeDto` | Name, slug |
| `TagMapper` | `ToTagDto` | Name, usage count |
| `PackageMapper` | `ToPackageDto`, `ToPackageSummaryDto` | Items collection, pricing tier |
| `PlaylistMapper` | `ToPlaylistDto` | Videos collection, video count |
| `LyricsMapper` | `ToLyricsDto` | Content body, associated video |
| `ShortVideoMapper` | `ToShortVideoDto` | Duration, thumbnail |
| `OrderMapper` | `ToOrderDto` | Order items, payment status, customer reference |

## Seeder Integration Tests

### SuperAdminSeeder

```csharp
[Collection("Database")]
public class SuperAdminSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task Seed_ShouldCreateSuperAdminUser()
    {
        // Arrange
        await using var context = CreateDbContext<IdentityDbContext>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SUPER_ADMIN_EMAIL"] = "superadmin@test.com",
                ["SUPER_ADMIN_PASSWORD"] = "SuperAdmin123!",
                ["SUPER_ADMIN_USERNAME"] = "superadmin"
            })
            .Build();
        var seeder = new SuperAdminSeeder(context, configuration);

        // Act
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        AuthUserEntity? superAdmin = await verifyContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        superAdmin.Should().NotBeNull();
        superAdmin!.Email.Should().Be("superadmin@test.com");
        superAdmin.IsActive.Should().BeTrue();
        superAdmin.IsVerified.Should().BeTrue();
        superAdmin.UserRoles.Should().ContainSingle(ur => ur.Role.Name == "SuperAdmin");
    }

    [Fact]
    public async Task Seed_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        await using var context = CreateDbContext<IdentityDbContext>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SUPER_ADMIN_EMAIL"] = "superadmin@test.com",
                ["SUPER_ADMIN_PASSWORD"] = "SuperAdmin123!",
                ["SUPER_ADMIN_USERNAME"] = "superadmin"
            })
            .Build();
        var seeder = new SuperAdminSeeder(context, configuration);

        // Act
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        // Assert — still exactly one SuperAdmin
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        int count = await verifyContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .CountAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        count.Should().Be(1);
    }

    [Fact]
    public async Task Seed_ShouldHashPasswordFromConfig()
    {
        // Arrange
        await using var context = CreateDbContext<IdentityDbContext>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SUPER_ADMIN_EMAIL"] = "superadmin@test.com",
                ["SUPER_ADMIN_PASSWORD"] = "SuperAdmin123!",
                ["SUPER_ADMIN_USERNAME"] = "superadmin"
            })
            .Build();
        var seeder = new SuperAdminSeeder(context, configuration);

        // Act
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        AuthUserEntity superAdmin = await verifyContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.UserRoles.Any(ur => ur.Role.Name == "SuperAdmin"));

        superAdmin.PasswordHash.Should().NotBeNullOrEmpty();
        superAdmin.PasswordHash.Should().NotBe("TestPassword123!");
    }
}
```

### VisitorRoleSeeder

```csharp
[Collection("Database")]
public class VisitorRoleSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task Seed_ShouldCreateVisitorRole()
    {
        // Arrange
        await using var context = CreateDbContext<IdentityDbContext>();
        var seeder = new VisitorRoleSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        RoleEntity? visitor = await verifyContext.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == "Visitor");

        visitor.Should().NotBeNull();
        visitor!.IsActive.Should().BeTrue();
        visitor.RolePermissions.Should().HaveCount(28);
    }

    [Fact]
    public async Task Seed_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        await using var context = CreateDbContext<IdentityDbContext>();
        var seeder = new VisitorRoleSeeder(context);

        // Act
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        int count = await verifyContext.Roles.CountAsync(r => r.Name == "Visitor");

        count.Should().Be(1);
    }
}
```

### ContentTypeSeeder

```csharp
[Collection("Database")]
public class ContentTypeSeederTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task Seed_ShouldCreateAllContentTypes()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();
        var seeder = new ContentTypeSeeder(context);

        // Act
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<ContentDbContext>();
        List<ContentTypeEntity> types = await verifyContext.ContentTypes.ToListAsync();

        string[] expectedNames = Enum.GetNames<EnumCoreContentType>();
        types.Should().HaveCount(expectedNames.Length);
        types.Select(t => t.Name).Should().BeEquivalentTo(expectedNames);
    }

    [Fact]
    public async Task Seed_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        await using var context = CreateDbContext<ContentDbContext>();
        var seeder = new ContentTypeSeeder(context);

        // Act
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        // Assert
        await using var verifyContext = CreateDbContext<ContentDbContext>();
        int count = await verifyContext.ContentTypes.CountAsync();

        string[] expectedNames = Enum.GetNames<EnumCoreContentType>();
        count.Should().Be(expectedNames.Length);
    }
}
```

## Test File Locations

```
tests/Integration/
├── Modules/
│   ├── Identity/
│   │   └── Seeders/
│   │       ├── SuperAdminSeederTests.cs
│   │       └── VisitorRoleSeederTests.cs
│   └── Content/
│       ├── Mappers/
│       │   ├── CategoryMapperTests.cs
│       │   ├── VideoMapperTests.cs
│       │   ├── ArticleMapperTests.cs
│       │   ├── CustomerMapperTests.cs
│       │   ├── ContentTypeMapperTests.cs
│       │   ├── TagMapperTests.cs
│       │   ├── PackageMapperTests.cs
│       │   ├── PlaylistMapperTests.cs
│       │   ├── LyricsMapperTests.cs
│       │   ├── ShortVideoMapperTests.cs
│       │   └── OrderMapperTests.cs
│       └── Seeders/
│           └── ContentTypeSeederTests.cs
```
