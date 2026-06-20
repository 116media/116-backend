using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Tests.Fixtures.Factories.Identity;

namespace _116.Integration.Tests.Identity.Repositories;

/// <summary>
/// Integration tests for <see cref="IRolePermissionRepository"/> verifying
/// junction record operations against a real PostgreSQL database.
/// </summary>
[Collection("Database")]
public class RolePermissionRepositoryTests(PostgresFixture postgres) : BaseRepositoryTest(postgres)
{
    [Fact]
    public async Task ExistsByRoleAndPermissionAsync_ExistingAssociation_ShouldReturnTrue()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var permission = PermissionFactory.Create("rp_test_a", "read");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        seedContext.RolePermissions.Add(rolePermission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRolePermissionRepository>();

        var exists = await repo.ExistsByRoleAndPermissionAsync(role.Id, permission.Id);

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByRoleAndPermissionAsync_NonExistentAssociation_ShouldReturnFalse()
    {
        var repo = Resolve<IRolePermissionRepository>();

        var exists = await repo.ExistsByRoleAndPermissionAsync(Guid.NewGuid(), Guid.NewGuid());

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByRoleAndPermissionAsync_ExistingAssociation_ShouldReturnEntity()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var permission = PermissionFactory.Create("rp_test_b", "create");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        seedContext.RolePermissions.Add(rolePermission);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRolePermissionRepository>();

        var result = await repo.GetByRoleAndPermissionAsync(role.Id, permission.Id);

        result.Should().NotBeNull();
        result!.RoleId.Should().Be(role.Id);
        result.PermissionId.Should().Be(permission.Id);
    }

    [Fact]
    public async Task GetByRoleAndPermissionAsync_NonExistentAssociation_ShouldReturnNull()
    {
        var repo = Resolve<IRolePermissionRepository>();

        var result = await repo.GetByRoleAndPermissionAsync(Guid.NewGuid(), Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistJunctionRecord()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var permission = PermissionFactory.Create("rp_test_c", "delete");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IRolePermissionRepository, IdentityDbContext>();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        await repo.AddAsync(rolePermission);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var saved = await verifyContext.RolePermissions.FindAsync(rolePermission.Id);

        saved.Should().NotBeNull();
        saved!.RoleId.Should().Be(role.Id);
        saved.PermissionId.Should().Be(permission.Id);
    }

    [Fact]
    public async Task Delete_ShouldRemoveJunctionRecord()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var permission = PermissionFactory.Create("rp_test_d", "update");
        seedContext.Roles.Add(role);
        seedContext.Permissions.Add(permission);
        await seedContext.SaveChangesAsync();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        seedContext.RolePermissions.Add(rolePermission);
        await seedContext.SaveChangesAsync();

        var (repo, db) = CreateScopedRepository<IRolePermissionRepository, IdentityDbContext>();
        var toDelete = await db.RolePermissions.FindAsync(rolePermission.Id);
        toDelete.Should().NotBeNull();

        repo.Delete(toDelete!);
        await db.SaveChangesAsync();

        await using var verifyContext = CreateDbContext<IdentityDbContext>();
        var deleted = await verifyContext.RolePermissions.FindAsync(rolePermission.Id);

        deleted.Should().BeNull();
    }

    [Fact]
    public async Task GetPermissionIdsByRoleIdAsync_ShouldReturnCorrectPermissionIds()
    {
        await using var seedContext = CreateDbContext<IdentityDbContext>();
        var role = RoleFactory.Create();
        var permissionA = PermissionFactory.Create("rp_test_e", "read");
        var permissionB = PermissionFactory.Create("rp_test_f", "create");
        var permissionC = PermissionFactory.Create("rp_test_g", "delete");
        seedContext.Roles.Add(role);
        seedContext.Permissions.AddRange(permissionA, permissionB, permissionC);
        await seedContext.SaveChangesAsync();

        var rpA = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permissionA.Id);
        var rpB = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permissionB.Id);
        var rpC = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permissionC.Id);
        seedContext.RolePermissions.AddRange(rpA, rpB, rpC);
        await seedContext.SaveChangesAsync();

        var repo = Resolve<IRolePermissionRepository>();

        var permissionIds = await repo.GetPermissionIdsByRoleIdAsync(role.Id);

        permissionIds.Should().HaveCount(3);
        permissionIds.Should().Contain(permissionA.Id);
        permissionIds.Should().Contain(permissionB.Id);
        permissionIds.Should().Contain(permissionC.Id);
    }
}
