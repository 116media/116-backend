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
        primaryKey.Properties.Should().ContainSingle();
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
        emailProperty.Should().NotBeNull();
        emailProperty.GetMaxLength().Should().NotBeNull();
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
        userNameProperty.Should().NotBeNull();
        userNameProperty.IsNullable.Should().BeFalse();
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
        primaryKey.Properties.Should().ContainSingle();
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
        nameProperty.Should().NotBeNull();
        nameProperty.IsNullable.Should().BeFalse();
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
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
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
        resourceProperty.Should().NotBeNull();
        resourceProperty.IsNullable.Should().BeFalse();
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
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
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
        userIdProperty.Should().NotBeNull();
        userIdProperty.IsNullable.Should().BeFalse();
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
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
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
        codeProperty.Should().NotBeNull();
        codeProperty.IsNullable.Should().BeFalse();
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
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
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
        userIdProperty.Should().NotBeNull();
        userIdProperty.IsNullable.Should().BeFalse();
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
        roleIdProperty.Should().NotBeNull();
        roleIdProperty.IsNullable.Should().BeFalse();
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
        entityType.Should().NotBeNull();
        IKey? primaryKey = entityType.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey.Properties.Should().ContainSingle();
        primaryKey.Properties.First().Name.Should().Be("Id");
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
        roleIdProperty.Should().NotBeNull();
        roleIdProperty.IsNullable.Should().BeFalse();
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
        permissionIdProperty.Should().NotBeNull();
        permissionIdProperty.IsNullable.Should().BeFalse();
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
        context.Model.FindEntityType(typeof(UserEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(RoleEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(PermissionEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(SessionEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(OtpEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(UserRoleEntity))?.GetSchema().Should().Be("identity");
        context.Model.FindEntityType(typeof(RolePermissionEntity))?.GetSchema().Should().Be("identity");
    }

    #endregion
}
