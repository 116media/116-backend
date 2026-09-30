using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Identity.TestData.Factories;
using _116.Tests.TestData.Helpers;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace _116.Identity.Unit.Tests.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminRepositoryManager"/>.
/// </summary>
public class SuperAdminRepositoryManagerTests
{
    private readonly Mock<ILogger<SuperAdminRepositoryManager>> _loggerMock;

    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();

    public SuperAdminRepositoryManagerTests()
    {
        _loggerMock = new Mock<ILogger<SuperAdminRepositoryManager>>();
    }

    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region SuperAdminExistsAsync Tests

    [Fact]
    public async Task SuperAdminExistsAsync_WhenSuperAdminDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.SuperAdminExistsAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SuperAdminExistsAsync_WhenSuperAdminExists_ShouldReturnTrue()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var superAdminUser = UserFactory.Create(SuperAdminConfiguration.Email, "testuser");
        await context.Users.AddAsync(superAdminUser);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.SuperAdminExistsAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SuperAdminExistsAsync_WhenDifferentUserExists_ShouldReturnFalse()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var otherUser = UserFactory.Create("other@example.com", "otheruser");
        await context.Users.AddAsync(otherUser);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.SuperAdminExistsAsync();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region FindPermissionAsync Tests

    [Fact]
    public async Task FindPermissionAsync_WhenPermissionExists_ShouldReturnPermission()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var permission = PermissionFactory.Create("system", "all", "Test permission");
        await context.Permissions.AddAsync(permission);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        PermissionEntity? result = await manager.FindPermissionAsync("system", "all");

        // Assert
        result.Should().NotBeNull();
        result.Resource.Should().Be("system");
        result.Action.Should().Be("all");
    }

    [Fact]
    public async Task FindPermissionAsync_WhenPermissionDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        PermissionEntity? result = await manager.FindPermissionAsync("nonexistent", "action");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindPermissionAsync_WhenResourceMatchesButActionDoesNot_ShouldReturnNull()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var permission = PermissionFactory.Create("system", "read", "Test permission");
        await context.Permissions.AddAsync(permission);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        PermissionEntity? result = await manager.FindPermissionAsync("system", "all");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region FindRoleAsync Tests

    [Fact]
    public async Task FindRoleAsync_WhenRoleExists_ShouldReturnRole()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var role = RoleFactory.Create("SuperAdmin", "Test role");
        await context.Roles.AddAsync(role);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        RoleEntity? result = await manager.FindRoleAsync("SuperAdmin");

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task FindRoleAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        RoleEntity? result = await manager.FindRoleAsync("NonExistentRole");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region RolePermissionExistsAsync Tests

    [Fact]
    public async Task RolePermissionExistsAsync_WhenAssociationExists_ShouldReturnTrue()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        var rolePermission = RolePermissionFactory.Create(roleId, permissionId);
        await context.RolePermissions.AddAsync(rolePermission);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.RolePermissionExistsAsync(roleId, permissionId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RolePermissionExistsAsync_WhenAssociationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.RolePermissionExistsAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UserRoleExistsAsync Tests

    [Fact]
    public async Task UserRoleExistsAsync_WhenAssociationExists_ShouldReturnTrue()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userRole = UserRoleFactory.Create(userId, roleId);
        await context.UserRoles.AddAsync(userRole);
        await context.SaveChangesAsync();

        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.UserRoleExistsAsync(userId, roleId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task UserRoleExistsAsync_WhenAssociationDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        bool result = await manager.UserRoleExistsAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AddPermission Tests

    [Fact]
    public void AddPermission_ShouldAddPermissionToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var permission = PermissionFactory.Create("test", "create", "Test permission");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddPermission(permission);

        // Assert
        context.Permissions.Local.Should().Contain(permission);
    }

    [Fact]
    public void AddPermission_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var permission = PermissionFactory.Create("test", "create", "Test permission");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddPermission(permission);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added permission")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region AddRole Tests

    [Fact]
    public void AddRole_ShouldAddRoleToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var role = RoleFactory.Create("TestRole", "Test role description");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddRole(role);

        // Assert
        context.Roles.Local.Should().Contain(role);
    }

    [Fact]
    public void AddRole_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var role = RoleFactory.Create("TestRole", "Test role description");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddRole(role);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added role")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region AddUser Tests

    [Fact]
    public void AddUser_ShouldAddUserToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var user = UserFactory.Create("test@example.com", "testuser");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddUser(user);

        // Assert
        context.Users.Local.Should().Contain(user);
    }

    [Fact]
    public void AddUser_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var user = UserFactory.Create("test@example.com", "testuser");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddUser(user);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added user")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region AddRolePermission Tests

    [Fact]
    public void AddRolePermission_ShouldAddAssociationToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), Guid.NewGuid());
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddRolePermission(rolePermission);

        // Assert
        context.RolePermissions.Local.Should().Contain(rolePermission);
    }

    [Fact]
    public void AddRolePermission_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var rolePermission = RolePermissionEntity.Create(Guid.NewGuid(), Guid.NewGuid());
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddRolePermission(rolePermission);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added role-permission association")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region AddUserRole Tests

    [Fact]
    public void AddUserRole_ShouldAddAssociationToContext()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var userRole = UserRoleEntity.Create(Guid.NewGuid(), Guid.NewGuid());
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddUserRole(userRole);

        // Assert
        context.UserRoles.Local.Should().Contain(userRole);
    }

    [Fact]
    public void AddUserRole_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        var userRole = UserRoleEntity.Create(Guid.NewGuid(), Guid.NewGuid());
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        manager.AddUserRole(userRole);

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Added user-role association")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChangesToDatabase()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);

        var role = RoleFactory.Create("TestRole", "Test role");
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);
        manager.AddRole(role);

        // Act
        await manager.SaveChangesAsync();

        // Assert
        int roleCount = await context.Roles.CountAsync();
        roleCount.Should().Be(1);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldLogDebugMessage()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        await using var context = new IdentityDbContext(options);
        var manager = new SuperAdminRepositoryManager(context, _loggerMock.Object);

        // Act
        await manager.SaveChangesAsync();

        // Assert
        _loggerMock.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Saved changes to database")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    // Note: BeginTransactionAsync tests are not included because InMemory database doesn't support transactions
    // and throws InvalidOperationException. Transaction functionality should be tested with integration tests using a real database.
}
