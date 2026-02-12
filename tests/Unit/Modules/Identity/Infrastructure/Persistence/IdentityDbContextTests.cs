using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
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
        result.Should().NotBeNull();
    }

    [Fact]
    public void OnModelCreating_ShouldApplyConfigurationsFromAssembly()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IModel model = context.Model;

        // Assert
        IEntityType? userEntityType = model.FindEntityType(typeof(UserEntity));
        userEntityType.Should().NotBeNull();
        userEntityType.GetSchema().Should().Be("identity");
    }

    [Fact]
    public void Context_ShouldSetDefaultSchemaToIdentity()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IModel model = context.Model;

        // Assert
        IEntityType? roleEntityType = model.FindEntityType(typeof(RoleEntity));
        roleEntityType.Should().NotBeNull();
        roleEntityType.GetSchema().Should().Be("identity");
    }

    [Fact]
    public void Context_ShouldHaveAllEntityTypesConfigured()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IModel model = context.Model;

        // Assert
        model.FindEntityType(typeof(UserEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(RoleEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(PermissionEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(UserRoleEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(RolePermissionEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(OtpEntity)).Should().NotBeNull();
        model.FindEntityType(typeof(SessionEntity)).Should().NotBeNull();
    }
}
