using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Unit tests for all Identity entity configurations.
/// Validates EF Core entity type configurations for database schema mapping.
/// </summary>
public class EntityConfigurationsTests
{
    private DbContextOptions<IdentityDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region UserConfiguration Tests

    [Fact]
    public void UserConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserEntity));

        // Assert
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        Assert.Single(primaryKey.Properties);
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void UserConfiguration_EmailProperty_ShouldHaveCorrectMaxLength()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserEntity));
        IProperty? emailProperty = entityType?.FindProperty("Email");

        // Assert
        Assert.NotNull(emailProperty);
        Assert.NotNull(emailProperty.GetMaxLength());
    }

    [Fact]
    public void UserConfiguration_UserNameProperty_ShouldBeRequired()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserEntity));
        IProperty? userNameProperty = entityType?.FindProperty("UserName");

        // Assert
        Assert.NotNull(userNameProperty);
        Assert.False(userNameProperty.IsNullable);
    }

    #endregion

    #region RoleConfiguration Tests

    [Fact]
    public void RoleConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(RoleEntity));

        // Assert
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        Assert.Single(primaryKey.Properties);
        primaryKey.Properties.First().Name.Should().Be("Id");
    }

    [Fact]
    public void RoleConfiguration_NameProperty_ShouldBeRequired()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(RoleEntity));
        IProperty? nameProperty = entityType?.FindProperty("Name");

        // Assert
        Assert.NotNull(nameProperty);
        Assert.False(nameProperty.IsNullable);
    }

    #endregion

    #region PermissionConfiguration Tests

    [Fact]
    public void PermissionConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(PermissionEntity));

        // Assert
        Assert.NotNull(entityType);
        IKey? primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void PermissionConfiguration_ResourceProperty_ShouldBeRequired()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(PermissionEntity));
        IProperty? resourceProperty = entityType?.FindProperty("Resource");

        // Assert
        Assert.NotNull(resourceProperty);
        Assert.False(resourceProperty.IsNullable);
    }

    #endregion

    #region SessionConfiguration Tests

    [Fact]
    public void SessionConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(SessionEntity));

        // Assert
        Assert.NotNull(entityType);
        IKey? primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void SessionConfiguration_ShouldHaveUserIdProperty()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(SessionEntity));
        IProperty? userIdProperty = entityType?.FindProperty("UserId");

        // Assert
        Assert.NotNull(userIdProperty);
        Assert.False(userIdProperty.IsNullable);
    }

    #endregion

    #region OtpConfiguration Tests

    [Fact]
    public void OtpConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(OtpEntity));

        // Assert
        Assert.NotNull(entityType);
        IKey? primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void OtpConfiguration_CodeProperty_ShouldBeRequired()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(OtpEntity));
        IProperty? codeProperty = entityType?.FindProperty("Code");

        // Assert
        Assert.NotNull(codeProperty);
        Assert.False(codeProperty.IsNullable);
    }

    #endregion

    #region UserRoleConfiguration Tests

    [Fact]
    public void UserRoleConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserRoleEntity));

        // Assert
        Assert.NotNull(entityType);
        IKey? primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void UserRoleConfiguration_ShouldHaveUserIdProperty()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserRoleEntity));
        IProperty? userIdProperty = entityType?.FindProperty("UserId");

        // Assert
        Assert.NotNull(userIdProperty);
        Assert.False(userIdProperty.IsNullable);
    }

    [Fact]
    public void UserRoleConfiguration_ShouldHaveRoleIdProperty()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(UserRoleEntity));
        IProperty? roleIdProperty = entityType?.FindProperty("RoleId");

        // Assert
        Assert.NotNull(roleIdProperty);
        Assert.False(roleIdProperty.IsNullable);
    }

    #endregion

    #region RolePermissionConfiguration Tests

    [Fact]
    public void RolePermissionConfiguration_ShouldHavePrimaryKey()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(RolePermissionEntity));

        // Assert
        Assert.NotNull(entityType);
        IKey? primaryKey = entityType.FindPrimaryKey();
        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal("Id", primaryKey.Properties.First().Name);
    }

    [Fact]
    public void RolePermissionConfiguration_ShouldHaveRoleIdProperty()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(RolePermissionEntity));
        IProperty? roleIdProperty = entityType?.FindProperty("RoleId");

        // Assert
        Assert.NotNull(roleIdProperty);
        Assert.False(roleIdProperty.IsNullable);
    }

    [Fact]
    public void RolePermissionConfiguration_ShouldHavePermissionIdProperty()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act
        IEntityType? entityType = context.Model.FindEntityType(typeof(RolePermissionEntity));
        IProperty? permissionIdProperty = entityType?.FindProperty("PermissionId");

        // Assert
        Assert.NotNull(permissionIdProperty);
        Assert.False(permissionIdProperty.IsNullable);
    }

    #endregion

    #region Schema Tests

    [Fact]
    public void AllEntityConfigurations_ShouldUseIdentitySchema()
    {
        // Arrange
        DbContextOptions<IdentityDbContext> options = CreateOptions();
        using var context = new IdentityDbContext(options);

        // Act & Assert
        Assert.Equal("identity", context.Model.FindEntityType(typeof(UserEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(RoleEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(PermissionEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(SessionEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(OtpEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(UserRoleEntity))?.GetSchema());
        Assert.Equal("identity", context.Model.FindEntityType(typeof(RolePermissionEntity))?.GetSchema());
    }

    #endregion
}
