using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="RolePermissionRepository"/>.
/// </summary>
public class RolePermissionRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly RolePermissionRepository _repository;

    public RolePermissionRepositoryTests()
    {
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new RolePermissionRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    #region ExistsByRoleAndPermissionAsync Tests

    [Fact]
    public async Task ExistsByRoleAndPermissionAsync_WhenRolePermissionExists_ShouldReturnTrue()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        PermissionEntity permission = PermissionFactory.Create();
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByRoleAndPermissionAsync(role.Id, permission.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByRoleAndPermissionAsync_WhenRolePermissionDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        bool exists = await _repository.ExistsByRoleAndPermissionAsync(roleId, permissionId);

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region GetByRoleAndPermissionAsync Tests

    [Fact]
    public async Task GetByRoleAndPermissionAsync_WhenRolePermissionExists_ShouldReturnRolePermission()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        PermissionEntity permission = PermissionFactory.Create();
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        await _context.SaveChangesAsync();

        // Act
        RolePermissionEntity? result = await _repository.GetByRoleAndPermissionAsync(role.Id, permission.Id);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(role.Id);
        result.PermissionId.Should().Be(permission.Id);
    }

    [Fact]
    public async Task GetByRoleAndPermissionAsync_WhenRolePermissionDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity? result = await _repository.GetByRoleAndPermissionAsync(roleId, permissionId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetPermissionIdsByRoleIdAsync Tests

    [Fact]
    public async Task GetPermissionIdsByRoleIdAsync_WhenRoleHasPermissions_ShouldReturnPermissionIds()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        PermissionEntity permission1 = PermissionFactory.Create("article", "read");
        PermissionEntity permission2 = PermissionFactory.Create("article", "write");
        var rolePermission1 = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission1.Id);
        var rolePermission2 = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission2.Id);

        _context.Roles.Add(role);
        _context.Permissions.AddRange(permission1, permission2);
        _context.RolePermissions.AddRange(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        // Act
        List<Guid> result = await _repository.GetPermissionIdsByRoleIdAsync(role.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(permission1.Id);
        result.Should().Contain(permission2.Id);
    }

    [Fact]
    public async Task GetPermissionIdsByRoleIdAsync_WhenRoleHasNoPermissions_ShouldReturnEmptyList()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        List<Guid> result = await _repository.GetPermissionIdsByRoleIdAsync(roleId);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddRolePermissionToContext()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        PermissionEntity permission = PermissionFactory.Create();
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        await _context.SaveChangesAsync();

        // Act
        await _repository.AddAsync(rolePermission);
        await _context.SaveChangesAsync();

        // Assert
        RolePermissionEntity? saved = await _context.RolePermissions.FirstOrDefaultAsync(rp =>
            rp.RoleId == role.Id && rp.PermissionId == permission.Id
        );
        saved.Should().NotBeNull();
        saved.RoleId.Should().Be(role.Id);
        saved.PermissionId.Should().Be(permission.Id);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemoveRolePermissionFromContext()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        PermissionEntity permission = PermissionFactory.Create();
        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);

        _context.Roles.Add(role);
        _context.Permissions.Add(permission);
        _context.RolePermissions.Add(rolePermission);
        _context.SaveChanges();

        // Act
        _repository.Delete(rolePermission);
        _context.SaveChanges();

        // Assert
        RolePermissionEntity? deleted = _context.RolePermissions.FirstOrDefault(rp =>
            rp.RoleId == role.Id && rp.PermissionId == permission.Id
        );
        deleted.Should().BeNull();
    }

    #endregion
}
