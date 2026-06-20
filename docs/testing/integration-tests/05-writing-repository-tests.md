# Writing Repository Integration Tests

## What to Test

Repository integration tests cover EF Core queries that cannot be verified with InMemory/SQLite:

| Feature | Why It Needs Real PostgreSQL |
|---------|------------------------------|
| `ILike` (case-insensitive search) | PostgreSQL-specific, throws in InMemory |
| Unique constraints | InMemory does not enforce them |
| Foreign key cascades | InMemory does not enforce FK relationships |
| Schema separation | InMemory has no schema concept |
| Snake_case column mapping | Naming convention only applies to real provider |
| `JSONB` columns | PostgreSQL-specific type |
| Pagination with `COUNT` | Translation differences between providers |
| Complex `Include` chains | Navigation property loading behavior |
| `ORDER BY` with collation | Locale-dependent sorting |

## Test File Structure

```
tests/Integration/Modules/Content/Repositories/
├── CategoryRepositoryTests.cs
├── VideoRepositoryTests.cs
├── ArticleRepositoryTests.cs
├── TagRepositoryTests.cs
├── CustomerRepositoryTests.cs
├── PackageRepositoryTests.cs
├── LookupRepositoryTests.cs
└── ...
```

## Naming Convention

```
{RepositoryMethod}_{Scenario}_{ExpectedResult}
```

Examples:
- `GetBySlugAsync_WhenSlugExistsCaseInsensitive_ShouldReturnCategory`
- `GetAllAsync_WithPagination_ShouldReturnCorrectPage`
- `GetExclusiveCategoryAsync_WhenNoExclusive_ShouldReturnNull`
- `AddAsync_WithDuplicateSlug_ShouldThrowUniqueConstraintViolation`

## Example: CategoryRepositoryTests

```csharp
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Repositories;
using _116.Integration.Tests.Common.Abstractions;
using _116.Integration.Tests.Common.Fixtures;
using AwesomeAssertions;

namespace _116.Integration.Tests.Modules.Content.Repositories;

/// <summary>
/// Integration tests for CategoryRepository against real PostgreSQL.
/// Covers ILike queries, unique constraints, and navigation property loading.
/// </summary>
public class CategoryRepositoryTests : BaseRepositoryTest
{
    public CategoryRepositoryTests(PostgresFixture db) : base(db) { }

    #region GetBySlugAsync (ILike)

    [Fact]
    public async Task GetBySlugAsync_WhenSlugMatchesExactCase_ShouldReturnCategory()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();
        Guid contentTypeId = await SeedContentTypeAsync(seedContext);
        var category = CreateCategory(contentTypeId, slug: "tech-videos");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act
        await using var queryContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(queryContext);
        CategoryEntity? result = await repository.GetBySlugAsync("tech-videos");

        // Assert
        result.Should().NotBeNull();
        result!.Slug.Should().Be("tech-videos");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDiffersInCase_ShouldReturnCategory()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();
        Guid contentTypeId = await SeedContentTypeAsync(seedContext);
        var category = CreateCategory(contentTypeId, slug: "tech-videos");
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act
        await using var queryContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(queryContext);
        CategoryEntity? result = await repository.GetBySlugAsync("TECH-VIDEOS");

        // Assert — ILike makes this case-insensitive
        result.Should().NotBeNull();
        result!.Slug.Should().Be("tech-videos");
    }

    [Fact]
    public async Task GetBySlugAsync_WhenSlugDoesNotExist_ShouldReturnNull()
    {
        // Arrange — empty database after reset

        // Act
        await using var queryContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(queryContext);
        CategoryEntity? result = await repository.GetBySlugAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetExclusiveCategoryAsync

    [Fact]
    public async Task GetExclusiveCategoryAsync_WhenExclusiveExists_ShouldReturnWithContentType()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();
        Guid contentTypeId = await SeedContentTypeAsync(seedContext);
        var category = CreateCategory(contentTypeId);
        category.SetExclusive();
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act
        await using var queryContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(queryContext);
        CategoryEntity? result = await repository.GetExclusiveCategoryAsync();

        // Assert — navigation property should be loaded
        result.Should().NotBeNull();
        result!.IsExclusive.Should().BeTrue();
        result.ContentType.Should().NotBeNull();
        result.ContentType.Name.Should().Be("Video");
    }

    [Fact]
    public async Task GetExclusiveCategoryAsync_WhenNoExclusive_ShouldReturnNull()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();
        Guid contentTypeId = await SeedContentTypeAsync(seedContext);
        var category = CreateCategory(contentTypeId);
        seedContext.Categories.Add(category);
        await seedContext.SaveChangesAsync();

        // Act
        await using var queryContext = CreateDbContext<ContentDbContext>();
        var repository = new CategoryRepository(queryContext);
        CategoryEntity? result = await repository.GetExclusiveCategoryAsync();

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Unique Constraints

    [Fact]
    public async Task AddAsync_WithDuplicateSlug_ShouldThrowDbUpdateException()
    {
        // Arrange
        await using var seedContext = CreateDbContext<ContentDbContext>();
        Guid contentTypeId = await SeedContentTypeAsync(seedContext);
        var category1 = CreateCategory(contentTypeId, slug: "unique-slug");
        seedContext.Categories.Add(category1);
        await seedContext.SaveChangesAsync();

        // Act
        await using var actContext = CreateDbContext<ContentDbContext>();
        var category2 = CreateCategory(contentTypeId, slug: "unique-slug");
        actContext.Categories.Add(category2);
        Func<Task> act = () => actContext.SaveChangesAsync();

        // Assert — PostgreSQL enforces the unique index
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    #endregion

    #region Helpers

    private static CategoryEntity CreateCategory(
        Guid contentTypeId,
        string? slug = null)
    {
        // Use domain factory method to create valid entity
        return CategoryEntity.Create(
            id: Guid.NewGuid(),
            contentTypeId: contentTypeId,
            name: $"Category {Guid.NewGuid().ToString("N")[..8]}",
            slug: slug ?? $"cat-{Guid.NewGuid().ToString("N")[..8]}",
            description: "Test category description",
            isFree: false,
            errors: TestErrorsFactory.CreateCategoryErrors(),
            isGossip: false,
            isExclusive: false
        );
    }

    private static async Task<Guid> SeedContentTypeAsync(ContentDbContext context)
    {
        var contentType = ContentTypeEntity.Create(
            id: Guid.NewGuid(),
            name: "Video",
            description: "Video content type"
        );
        context.ContentTypes.Add(contentType);
        await context.SaveChangesAsync();
        return contentType.Id;
    }

    #endregion
}
```

## Common Patterns

### Pattern 1: Pagination

```csharp
[Fact]
public async Task GetAllAsync_WithPagination_ShouldReturnCorrectPage()
{
    // Arrange — seed 15 categories
    await using var seedContext = CreateDbContext<ContentDbContext>();
    Guid contentTypeId = await SeedContentTypeAsync(seedContext);
    for (int i = 0; i < 15; i++)
    {
        seedContext.Categories.Add(CreateCategory(contentTypeId));
    }
    await seedContext.SaveChangesAsync();

    // Act — request page 2, size 5
    await using var queryContext = CreateDbContext<ContentDbContext>();
    var repository = new CategoryRepository(queryContext);
    var (items, totalCount) = await repository.GetAllAsync(
        page: 2, pageSize: 5, search: null);

    // Assert
    items.Should().HaveCount(5);
    totalCount.Should().Be(15);
}
```

### Pattern 2: Navigation Property Loading

```csharp
[Fact]
public async Task GetByIdOrThrowAsync_ShouldEagerLoadContentType()
{
    // Arrange
    await using var seedContext = CreateDbContext<ContentDbContext>();
    Guid contentTypeId = await SeedContentTypeAsync(seedContext);
    var category = CreateCategory(contentTypeId);
    seedContext.Categories.Add(category);
    await seedContext.SaveChangesAsync();

    // Act
    await using var queryContext = CreateDbContext<ContentDbContext>();
    var repository = new CategoryRepository(queryContext);
    CategoryEntity result = await repository.GetByIdOrThrowAsync(category.Id);

    // Assert — ContentType should be eagerly loaded
    result.ContentType.Should().NotBeNull();
    result.ContentType.Name.Should().Be("Video");
}
```

### Pattern 3: Soft Delete Filtering

```csharp
[Fact]
public async Task GetAllAsync_ShouldExcludeSoftDeletedEntities()
{
    // Arrange
    await using var seedContext = CreateDbContext<ContentDbContext>();
    // ... seed active and soft-deleted categories

    // Act — query should exclude soft-deleted
    // Assert — only active categories returned
}
```

## Tests That Were Skipped in Unit Tests

The following unit tests were marked with `[Fact(Skip = "...")]` due to ILike. They should now be implemented as repository integration tests:

| Skipped Unit Test | Integration Test File |
|-------------------|----------------------|
| `CategoryRepositoryTests.GetBySlugAsync_WhenFound` | `CategoryRepositoryTests.cs` |
| `CustomerRepositoryTests.GetByEmailAsync_WhenFound` | `CustomerRepositoryTests.cs` |
| `ContentTypeByNameSpecification` (ILike eval) | `LookupRepositoryTests.cs` |
| `PricingTierByNameSpecification` (ILike eval) | `LookupRepositoryTests.cs` |
| `PromotionLevelByNameSpecification` (ILike eval) | `LookupRepositoryTests.cs` |
| `TagSearchSpecification` (ILike eval) | `TagRepositoryTests.cs` |

## Gotchas

### Entity Creation

Domain entities use private constructors and factory methods (`Create()`). You cannot use `new CategoryEntity()` directly. Always use the domain factory method, even in tests. The factories in `_116.Tests.Fixtures` may or may not work for integration tests because some use reflection to set navigation properties — with real PostgreSQL, navigation properties are loaded by EF Core, not set manually.

### Audit Fields

The `AuditableEntityInterceptor` auto-populates `created_at`, `updated_at`, `created_by`, `updated_by` on `SaveChangesAsync()`. In repository tests (without the full HTTP pipeline), there is no `HttpContext` — so `created_by` and `updated_by` will be null. This is expected. If you need to test audit behavior, use API integration tests where the authenticated user's ID flows through.

### Interceptor Registration

When creating DbContexts directly (not through DI), interceptors are not registered. This means:
- `AuditableEntityInterceptor` does not run
- `DispatchDomainEventsInterceptor` does not run

This is fine for repository tests (we're testing query behavior, not interceptors). To test interceptors, use API integration tests.
