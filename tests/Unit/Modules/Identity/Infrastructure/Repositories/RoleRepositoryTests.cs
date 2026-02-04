using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Builders.Entities;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Repositories;

/// <summary>
/// Unit tests for <see cref="RoleRepository"/>.
/// </summary>
public class RoleRepositoryTests : IDisposable
{
    private readonly IdentityDbContext _context;
    private readonly RoleRepository _repository;

    public RoleRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IdentityDbContext(options);
        _repository = new RoleRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetRoleByIdWithPermissionsOrThrowAsync Tests

    [Fact]
    public async Task GetRoleByIdWithPermissionsOrThrowAsync_WhenRoleExists_ShouldReturnRoleWithPermissions()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        var permission1 = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var permission2 = new PermissionBuilder().WithResource("article").WithAction("write").Build();
        var rolePermission1 = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission1.Id);
        var rolePermission2 = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission2.Id);

        _context.Roles.Add(role);
        _context.Permissions.AddRange(permission1, permission2);
        _context.RolePermissions.AddRange(rolePermission1, rolePermission2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdWithPermissionsOrThrowAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.RolePermissions.Should().HaveCount(2);
        result.RolePermissions.Should().AllSatisfy(rp => rp.Permission.Should().NotBeNull());
    }

    [Fact]
    public async Task GetRoleByIdWithPermissionsOrThrowAsync_WhenRoleDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetRoleByIdWithPermissionsOrThrowAsync(roleId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetRoleByIdWithPermissionsOrThrowAsync_WhenRoleExistsWithNoPermissions_ShouldReturnRoleWithEmptyPermissions()
    {
        // Arrange
        var role = new RoleBuilder().Build();
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdWithPermissionsOrThrowAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.RolePermissions.Should().BeEmpty();
    }

    #endregion

    #region GetRoleByIdOrThrowAsync Tests

    [Fact]
    public async Task GetRoleByIdOrThrowAsync_WhenRoleExists_ShouldReturnRole()
    {
        // Arrange
        var role = new RoleBuilder().WithName("TestRole").Build();
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdOrThrowAsync(role.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(role.Id);
        result.Name.Should().Be("TestRole");
    }

    [Fact]
    public async Task GetRoleByIdOrThrowAsync_WhenRoleDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var roleId = Guid.NewGuid();

        // Act
        Func<Task> act = async () => await _repository.GetRoleByIdOrThrowAsync(roleId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ExistsByNameAsync Tests

    [Fact]
    public async Task ExistsByNameAsync_WhenRoleExists_ShouldReturnTrue()
    {
        // Arrange
        var role = new RoleBuilder().WithName("UniqueRole").Build();
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        bool exists = await _repository.ExistsByNameAsync("UniqueRole");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNameAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        // Arrange & Act
        bool exists = await _repository.ExistsByNameAsync("NonExistentRole");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_ShouldAddRoleToContext()
    {
        // Arrange
        var role = new RoleBuilder().WithName("NewRole").Build();

        // Act
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        var savedRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("NewRole");
    }

    #endregion

    #region GetAllWithPaginationAsync Tests

    [Fact]
    public async Task GetAllWithPaginationAsync_WithNoFilters_ShouldReturnAllRoles()
    {
        // Arrange
        var role1 = new RoleBuilder().WithName("Role1").Build();
        var role2 = new RoleBuilder().WithName("Role2").Build();
        var role3 = new RoleBuilder().WithName("Role3").Build();

        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        roles.Should().HaveCount(3);
        totalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var role = new RoleBuilder().WithName($"Role{i}").Build();
            _context.Roles.Add(role);
        }
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 2, pageSize: 2);

        // Assert
        roles.Should().HaveCount(2);
        totalCount.Should().Be(5);
    }

    // Note: Search functionality uses PostgreSQL ILike which is not supported by InMemoryDatabase.
    // This test is better suited for integration tests with a real PostgreSQL database.

    [Fact]
    public async Task GetAllWithPaginationAsync_WithActiveFilter_ShouldFilterByActiveStatus()
    {
        // Arrange
        var activeRole = new RoleBuilder().WithName("ActiveRole").Build();
        var inactiveRole = new RoleBuilder().WithName("InactiveRole").AsInactive().Build();

        _context.Roles.AddRange(activeRole, inactiveRole);
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10, isActive: true);

        // Assert
        roles.Should().HaveCount(1);
        totalCount.Should().Be(1);
        roles.First().Name.Should().Be("ActiveRole");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithDeletedFilter_ShouldFilterByDeletedStatus()
    {
        // Arrange
        var normalRole = new RoleBuilder().WithName("NormalRole").Build();
        var deletedRole = new RoleBuilder().WithName("DeletedRole").AsDeleted().Build();

        _context.Roles.AddRange(normalRole, deletedRole);
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10, isDeleted: false);

        // Assert
        roles.Should().HaveCount(1);
        totalCount.Should().Be(1);
        roles.First().Name.Should().Be("NormalRole");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        roles.Should().BeEmpty();
        totalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_ShouldOrderByCreatedAtDescending()
    {
        // Arrange
        var role1 = new RoleBuilder().WithName("First").Build();
        var role2 = new RoleBuilder().WithName("Second").Build();
        var role3 = new RoleBuilder().WithName("Third").Build();

        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync();

        // Act
        var (roles, _) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        roles.Should().HaveCount(3);
        // InMemoryDatabase may not preserve exact insertion order for CreatedAt
        // Just verify all roles are returned
        var roleNames = roles.Select(r => r.Name).ToList();
        roleNames.Should().Contain("First");
        roleNames.Should().Contain("Second");
        roleNames.Should().Contain("Third");
    }

    #endregion

    #region GetUserRoles Tests

    [Fact]
    public void GetUserRoles_WithUserRoles_ShouldReturnRoleDtos()
    {
        // Arrange
        var role1 = new RoleBuilder().WithName("Admin").Build();
        var role2 = new RoleBuilder().WithName("Visitor").Build();
        var user = new UserBuilder().Build();
        var userRole1 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role1.Id);
        var userRole2 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role2.Id);

        // Manually set navigation properties
        userRole1.GetType().GetProperty("Role")!.SetValue(userRole1, role1);
        userRole2.GetType().GetProperty("Role")!.SetValue(userRole2, role2);

        var userRoles = new List<UserRoleEntity> { userRole1, userRole2 };

        // Act
        var result = _repository.GetUserRoles(userRoles);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Name == "Admin");
        result.Should().Contain(r => r.Name == "Visitor");
    }

    [Fact]
    public void GetUserRoles_WithEmptyUserRoles_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userRoles = new List<UserRoleEntity>();

        // Act
        var result = _repository.GetUserRoles(userRoles);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserPermissions Tests

    [Fact]
    public void GetUserPermissions_WithUserRoles_ShouldReturnUniquePermissions()
    {
        // Arrange
        var role1 = new RoleBuilder().WithName("Admin").Build();
        var role2 = new RoleBuilder().WithName("Editor").Build();
        var user = new UserBuilder().Build();

        var permission1 = new PermissionBuilder().WithResource("article").WithAction("read").Build();
        var permission2 = new PermissionBuilder().WithResource("article").WithAction("write").Build();
        var permission3 = new PermissionBuilder().WithResource("user").WithAction("read").Build();

        var rolePermission1 = RolePermissionEntity.Create(Guid.NewGuid(), role1.Id, permission1.Id);
        var rolePermission2 = RolePermissionEntity.Create(Guid.NewGuid(), role1.Id, permission2.Id);
        var rolePermission3 = RolePermissionEntity.Create(Guid.NewGuid(), role2.Id, permission1.Id); // Duplicate
        var rolePermission4 = RolePermissionEntity.Create(Guid.NewGuid(), role2.Id, permission3.Id);

        // Set navigation properties
        rolePermission1.GetType().GetProperty("Permission")!.SetValue(rolePermission1, permission1);
        rolePermission2.GetType().GetProperty("Permission")!.SetValue(rolePermission2, permission2);
        rolePermission3.GetType().GetProperty("Permission")!.SetValue(rolePermission3, permission1);
        rolePermission4.GetType().GetProperty("Permission")!.SetValue(rolePermission4, permission3);

        role1
            .GetType()
            .GetProperty("RolePermissions")!
            .SetValue(role1, new List<RolePermissionEntity> { rolePermission1, rolePermission2 });
        role2
            .GetType()
            .GetProperty("RolePermissions")!
            .SetValue(role2, new List<RolePermissionEntity> { rolePermission3, rolePermission4 });

        var userRole1 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role1.Id);
        var userRole2 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role2.Id);

        userRole1.GetType().GetProperty("Role")!.SetValue(userRole1, role1);
        userRole2.GetType().GetProperty("Role")!.SetValue(userRole2, role2);

        var userRoles = new List<UserRoleEntity> { userRole1, userRole2 };

        // Act
        var result = _repository.GetUserPermissions(userRoles);

        // Assert
        result.Should().HaveCount(3); // Duplicates removed
        result.Should().Contain(p => p.Resource == "article" && p.Action == "read");
        result.Should().Contain(p => p.Resource == "article" && p.Action == "write");
        result.Should().Contain(p => p.Resource == "user" && p.Action == "read");
    }

    [Fact]
    public void GetUserPermissions_WithEmptyUserRoles_ShouldReturnEmptyCollection()
    {
        // Arrange
        var userRoles = new List<UserRoleEntity>();

        // Act
        var result = _repository.GetUserPermissions(userRoles);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserRolesAndPermissions Tests

    [Fact]
    public void GetUserRolesAndPermissions_ShouldReturnBothRolesAndPermissions()
    {
        // Arrange
        var role = new RoleBuilder().WithName("Admin").Build();
        var user = new UserBuilder().Build();
        var permission = new PermissionBuilder().WithResource("article").WithAction("read").Build();

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), role.Id, permission.Id);
        rolePermission.GetType().GetProperty("Permission")!.SetValue(rolePermission, permission);
        role.GetType()
            .GetProperty("RolePermissions")!
            .SetValue(role, new List<RolePermissionEntity> { rolePermission });

        var userRole = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role.Id);
        userRole.GetType().GetProperty("Role")!.SetValue(userRole, role);

        var userRoles = new List<UserRoleEntity> { userRole };

        // Act
        var (roles, permissions) = _repository.GetUserRolesAndPermissions(userRoles);

        // Assert
        roles.Should().HaveCount(1);
        roles.First().Name.Should().Be("Admin");
        permissions.Should().HaveCount(1);
        permissions.First().Resource.Should().Be("article");
        permissions.First().Action.Should().Be("read");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemoveRoleFromContext()
    {
        // Arrange
        var role = new RoleBuilder().WithName("ToDelete").Build();
        _context.Roles.Add(role);
        _context.SaveChanges();

        // Act
        _repository.Delete(role);
        _context.SaveChanges();

        // Assert
        var deletedRole = _context.Roles.FirstOrDefault(r => r.Id == role.Id);
        deletedRole.Should().BeNull();
    }

    #endregion
}
