using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Integration tests for <see cref="IRoleRepository"/> verifying persistence behavior
/// against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class RoleRepositoryTests : BaseRepositoryTest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoleRepositoryTests"/> class
    /// with the shared database fixture.
    /// </summary>
    public RoleRepositoryTests(PostgresFixture postgres)
        : base(postgres) { }

    [Fact]
    public async Task GetAllWithPaginationAsync_ReturnsPaginatedResults()
    {
        var repo = Resolve<IRoleRepository>();
        var (_, baseCount) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 100);

        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var roles = RoleFactory.CreateMany(5);
        seedContext.Roles.AddRange(roles);
        await seedContext.SaveChangesAsync();

        var repoAfterSeed = Resolve<IRoleRepository>();
        var (result, totalCount) = await repoAfterSeed.GetAllWithPaginationAsync(page: 1, pageSize: 3);

        totalCount.Should().Be(baseCount + 5);
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetRoleByIdOrThrowAsync_WhenRoleExists_ReturnsEntity()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create("TestRole", "A test role");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRoleRepository>();

        // Act
        var result = await repo.GetRoleByIdOrThrowAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be("TestRole");
        result.Description.Should().Be("A test role");
    }

    [Fact]
    public async Task GetRoleByIdOrThrowAsync_WhenRoleDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        var repo = Resolve<IRoleRepository>();
        var nonExistentId = Guid.NewGuid();

        // Act
        var act = () => repo.GetRoleByIdOrThrowAsync(nonExistentId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetRoleByIdWithPermissionsOrThrowAsync_ReturnsRoleWithPermissions()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var role = RoleFactory.Create("PermRole", "Role with permissions");
        seedContext.Roles.Add(role);

        var permissions = PermissionFactory.CreateMany(3);
        seedContext.Permissions.AddRange(permissions);
        await seedContext.SaveChangesAsync();

        foreach (var permission in permissions)
        {
            var rolePermission = RolePermissionFactory.Create(role.Id, permission.Id);
            seedContext.RolePermissions.Add(rolePermission);
        }

        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRoleRepository>();

        // Act
        var result = await repo.GetRoleByIdWithPermissionsOrThrowAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be("PermRole");
    }

    [Fact]
    public async Task AddAsync_PersistsRoleToDatabase()
    {
        // Arrange
        var (repo, db) = CreateScopedRepository<IRoleRepository, IdentityDbContext>();
        var role = RoleFactory.Create("NewRole", "A newly added role");

        // Act
        await repo.AddAsync(role);
        await db.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var persisted = await verifyContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);

        persisted.Should().NotBeNull();
        persisted!.Name.Should().Be("NewRole");
        persisted.Description.Should().Be("A newly added role");
    }

    [Fact]
    public async Task Delete_RemovesRoleFromDatabase()
    {
        // Arrange
        var (repo, db) = CreateScopedRepository<IRoleRepository, IdentityDbContext>();
        var role = RoleFactory.Create("ToDelete", "Will be deleted");
        await repo.AddAsync(role);
        await db.SaveChangesAsync();

        // Act
        repo.Delete(role);
        await db.SaveChangesAsync();

        // Assert
        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNameExists_ReturnsTrue()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create("UniqueRole", "Exists check");
        seedContext.Roles.Add(role);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRoleRepository>();

        // Act
        var exists = await repo.ExistsByNameAsync("UniqueRole");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenNameDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repo = Resolve<IRoleRepository>();

        // Act
        var exists = await repo.ExistsByNameAsync("NonExistentRole");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithSearchTerm_FiltersResults()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        seedContext.Roles.AddRange(
            RoleFactory.Create("Moderator", "Content moderator"),
            RoleFactory.Create("Editor", "Content editor"),
            RoleFactory.Create("Reviewer", "Content reviewer")
        );
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRoleRepository>();

        // Act
        var (result, totalCount) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 10, search: "Editor");

        // Assert
        totalCount.Should().Be(1);
        result.Should().ContainSingle();
        result[0].Name.Should().Be("Editor");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithIsActiveFilter_FiltersResults()
    {
        // Arrange
        await using var seedContext = CreateDbContext<IdentityDbContext>();

        var activeRole = RoleFactory.Create("ActiveRole", "An active role");
        var inactiveRole = RoleFactory.CreateInactive("InactiveRole");

        seedContext.Roles.AddRange(activeRole, inactiveRole);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRoleRepository>();

        // Act
        var (result, totalCount) = await repo.GetAllWithPaginationAsync(page: 1, pageSize: 10, isActive: true);

        // Assert
        totalCount.Should().Be(1);
        result.Should().OnlyContain(r => r.IsActive);
    }
}
