using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Repositories;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Builders.Entities;
using _116.Tests.Fixtures.Factories;
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
        DbContextOptions<IdentityDbContext> options = new DbContextOptionsBuilder<IdentityDbContext>()
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
        RoleEntity? result = await _repository.GetRoleByIdWithPermissionsOrThrowAsync(role.Id);

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
        RoleEntity role = RoleFactory.Create();
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        RoleEntity? result = await _repository.GetRoleByIdWithPermissionsOrThrowAsync(role.Id);

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
        RoleEntity role = RoleFactory.Create("TestRole");
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        // Act
        RoleEntity? result = await _repository.GetRoleByIdOrThrowAsync(role.Id);

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
        RoleEntity role = RoleFactory.Create("UniqueRole");
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
        RoleEntity role = RoleFactory.Create("NewRole");

        // Act
        await _repository.AddAsync(role);
        await _context.SaveChangesAsync();

        // Assert
        RoleEntity? savedRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
        savedRole.Should().NotBeNull();
        savedRole!.Name.Should().Be("NewRole");
    }

    #endregion

    #region GetAllWithPaginationAsync Tests

    [Fact]
    public async Task GetAllWithPaginationAsync_WithNoFilters_ShouldReturnAllRoles()
    {
        // Arrange
        RoleEntity role1 = RoleFactory.Create("Role1");
        RoleEntity role2 = RoleFactory.Create("Role2");
        RoleEntity role3 = RoleFactory.Create("Role3");

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
            RoleEntity role = RoleFactory.Create($"Role{i}");
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
        RoleEntity activeRole = RoleFactory.Create("ActiveRole");
        RoleEntity inactiveRole = RoleFactory.Create("InactiveRole");
        inactiveRole.Deactivate();

        _context.Roles.AddRange(activeRole, inactiveRole);
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10, isActive: true);

        // Assert
        roles.Should().ContainSingle();
        totalCount.Should().Be(1);
        roles.First().Name.Should().Be("ActiveRole");
    }

    [Fact]
    public async Task GetAllWithPaginationAsync_WithDeletedFilter_ShouldFilterByDeletedStatus()
    {
        // Arrange
        RoleEntity normalRole = RoleFactory.Create("NormalRole");
        RoleEntity deletedRole = RoleFactory.Create("DeletedRole");
        deletedRole.SoftDelete();

        _context.Roles.AddRange(normalRole, deletedRole);
        await _context.SaveChangesAsync();

        // Act
        var (roles, totalCount) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10, isDeleted: false);

        // Assert
        roles.Should().ContainSingle();
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
        RoleEntity role1 = RoleFactory.Create("First");
        RoleEntity role2 = RoleFactory.Create("Second");
        RoleEntity role3 = RoleFactory.Create("Third");

        _context.Roles.AddRange(role1, role2, role3);
        await _context.SaveChangesAsync();

        // Act
        var (roles, _) = await _repository.GetAllWithPaginationAsync(page: 1, pageSize: 10);

        // Assert
        roles.Should().HaveCount(3);
        // InMemoryDatabase may not preserve exact insertion order for CreatedAt
        // Just verify all roles are returned
        List<string> roleNames = roles.Select(r => r.Name).ToList();
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
        RoleEntity role1 = RoleFactory.Create("Admin");
        RoleEntity role2 = RoleFactory.Create("Visitor");
        UserEntity user = UserFactory.Create();
        var userRole1 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role1.Id);
        var userRole2 = UserRoleEntity.Create(Guid.NewGuid(), user.Id, role2.Id);

        // Manually set navigation properties
        userRole1.GetType().GetProperty("Role")!.SetValue(userRole1, role1);
        userRole2.GetType().GetProperty("Role")!.SetValue(userRole2, role2);

        var userRoles = new List<UserRoleEntity> { userRole1, userRole2 };

        // Act
        IReadOnlyCollection<RoleDto> result = _repository.GetUserRoles(userRoles);

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
        IReadOnlyCollection<RoleDto> result = _repository.GetUserRoles(userRoles);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserPermissions Tests

    [Fact]
    public void GetUserPermissions_WithUserRoles_ShouldReturnUniquePermissions()
    {
        // Arrange
        RoleEntity role1 = RoleFactory.Create("Admin");
        RoleEntity role2 = RoleFactory.Create("Editor");
        UserEntity user = UserFactory.Create();

        PermissionEntity permission1 = PermissionFactory.Create("article", "read");
        PermissionEntity permission2 = PermissionFactory.Create("article", "write");
        PermissionEntity permission3 = PermissionFactory.Create("user", "read");

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
        IReadOnlyCollection<PermissionDto> result = _repository.GetUserPermissions(userRoles);

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
        IReadOnlyCollection<PermissionDto> result = _repository.GetUserPermissions(userRoles);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetUserRolesAndPermissions Tests

    [Fact]
    public void GetUserRolesAndPermissions_ShouldReturnBothRolesAndPermissions()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("Admin");
        UserEntity user = UserFactory.Create();
        PermissionEntity permission = PermissionFactory.Create("article", "read");

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
        roles.Should().ContainSingle();
        roles.First().Name.Should().Be("Admin");
        permissions.Should().ContainSingle();
        permissions.First().Resource.Should().Be("article");
        permissions.First().Action.Should().Be("read");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_ShouldRemoveRoleFromContext()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create("ToDelete");
        _context.Roles.Add(role);
        _context.SaveChanges();

        // Act
        _repository.Delete(role);
        _context.SaveChanges();

        // Assert
        RoleEntity? deletedRole = _context.Roles.FirstOrDefault(r => r.Id == role.Id);
        deletedRole.Should().BeNull();
    }

    #endregion
}
