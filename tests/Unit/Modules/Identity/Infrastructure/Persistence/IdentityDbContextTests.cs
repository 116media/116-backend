using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit tests for <see cref="IdentityDbContext"/>.
/// </summary>
public class IdentityDbContextTests
{
    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public void Users_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<UserEntity> result = context.Users;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Roles_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<RoleEntity> result = context.Roles;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Permissions_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<PermissionEntity> result = context.Permissions;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void UserRoles_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<UserRoleEntity> result = context.UserRoles;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void RolePermissions_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<RolePermissionEntity> result = context.RolePermissions;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Otps_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<OtpEntity> result = context.Otps;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Sessions_ShouldReturnDbSet()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        DbSet<SessionEntity> result = context.Sessions;

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        var model = context.Model;

        // Assert
        var userEntityType = model.FindEntityType(typeof(UserEntity));
        Assert.NotNull(userEntityType);
        Assert.Equal("identity", userEntityType.GetSchema());
    }

    [Fact]
    public void Context_ShouldSetDefaultSchemaToIdentity()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        var model = context.Model;

        // Assert
        var roleEntityType = model.FindEntityType(typeof(RoleEntity));
        Assert.NotNull(roleEntityType);
        Assert.Equal("identity", roleEntityType.GetSchema());
    }

    [Fact]
    public void Context_ShouldHaveAllEntityTypesConfigured()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        var model = context.Model;

        // Assert
        Assert.NotNull(model.FindEntityType(typeof(UserEntity)));
        Assert.NotNull(model.FindEntityType(typeof(RoleEntity)));
        Assert.NotNull(model.FindEntityType(typeof(PermissionEntity)));
        Assert.NotNull(model.FindEntityType(typeof(UserRoleEntity)));
        Assert.NotNull(model.FindEntityType(typeof(RolePermissionEntity)));
        Assert.NotNull(model.FindEntityType(typeof(OtpEntity)));
        Assert.NotNull(model.FindEntityType(typeof(SessionEntity)));
    }
}
