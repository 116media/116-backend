using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Mappers;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.DTOs;
using _116.Tests.Fixtures.Factories;
using AwesomeAssertions;
using Mapster;
using MapsterMapper;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Shared.Mappers;

/// <summary>
/// Unit tests for <see cref="RoleMapper"/>.
/// </summary>
public class RoleMapperTests
{
    private readonly IMapper _mapper;

    public RoleMapperTests()
    {
        TypeAdapterConfig config = MappingRegistration.CreateConfiguration();
        _mapper = new Mapper(config);
    }

    #region Register

    [Fact]
    public void Register_ShouldNotThrowException()
    {
        // Arrange
        var config = new TypeAdapterConfig();

        // Act & Assert
        Action act = () => RoleMapper.Register(config);
        act.Should().NotThrow();
    }

    #endregion

    #region RoleDto — core fields

    [Fact]
    public void ToRoleDto_ShouldMapCoreFields()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.Id.Should().Be(role.Id);
        dto.Name.Should().Be(role.Name);
        dto.Description.Should().Be(role.Description);
        dto.IsActive.Should().Be(role.IsActive);
        dto.IsDeleted.Should().Be(role.IsDeleted);
    }

    #endregion

    #region RoleDto — AuditableDto fields

    [Fact]
    public void ToRoleDto_ShouldInheritAuditableDto()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToRoleDto_ShouldMapCreatedAtFromEntity()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-5);
        RoleEntity role = RoleFactory.Create();
        role.CreatedAt = createdAt;

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToRoleDto_ShouldMapUpdatedAtFromEntity()
    {
        // Arrange
        DateTime updatedAt = DateTime.UtcNow.AddHours(-1);
        RoleEntity role = RoleFactory.Create();
        role.UpdatedAt = updatedAt;

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void ToRoleDto_ShouldMapCreatedByFromEntity()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        role.CreatedBy = "system";

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.CreatedBy.Should().Be("system");
    }

    [Fact]
    public void ToRoleDto_ShouldMapUpdatedByFromEntity()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();
        role.UpdatedBy = "admin-user";

        // Act
        var dto = role.ToRoleDto(_mapper);

        // Assert
        dto.UpdatedBy.Should().Be("admin-user");
    }

    #endregion

    #region PermissionDto — core fields

    [Fact]
    public void ToPermissionDto_ShouldMapCoreFields()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        var dto = permission.ToPermissionDto(_mapper);

        // Assert
        dto.Id.Should().Be(permission.Id);
        dto.Resource.Should().Be(permission.Resource);
        dto.Action.Should().Be(permission.Action);
        dto.Description.Should().Be(permission.Description);
        dto.IsActive.Should().Be(permission.IsActive);
        dto.IsDeleted.Should().Be(permission.IsDeleted);
    }

    #endregion

    #region PermissionDto — AuditableDto fields

    [Fact]
    public void ToPermissionDto_ShouldInheritAuditableDto()
    {
        // Arrange
        PermissionEntity permission = PermissionFactory.Create();

        // Act
        var dto = permission.ToPermissionDto(_mapper);

        // Assert
        dto.Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToPermissionDto_ShouldMapAuditFields()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-3);
        DateTime updatedAt = DateTime.UtcNow.AddHours(-2);
        PermissionEntity permission = PermissionFactory.Create();
        permission.CreatedAt = createdAt;
        permission.UpdatedAt = updatedAt;
        permission.CreatedBy = "system";
        permission.UpdatedBy = "admin-user";

        // Act
        var dto = permission.ToPermissionDto(_mapper);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.CreatedBy.Should().Be("system");
        dto.UpdatedBy.Should().Be("admin-user");
    }

    #endregion

    #region RoleWithPermissionsDto — core fields

    [Fact]
    public void ToRoleWithPermissionsDto_ShouldMapCoreFields()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();

        // Act
        var dto = role.ToRoleWithPermissionsDto(_mapper);

        // Assert
        dto.Id.Should().Be(role.Id);
        dto.Name.Should().Be(role.Name);
        dto.Description.Should().Be(role.Description);
        dto.IsActive.Should().Be(role.IsActive);
        dto.IsDeleted.Should().Be(role.IsDeleted);
    }

    #endregion

    #region RoleWithPermissionsDto — AuditableDto fields

    [Fact]
    public void ToRoleWithPermissionsDto_ShouldInheritAuditableDto()
    {
        // Arrange
        RoleEntity role = RoleFactory.Create();

        // Act
        var dto = role.ToRoleWithPermissionsDto(_mapper);

        // Assert
        dto.Should().BeAssignableTo<AuditableDto>();
    }

    [Fact]
    public void ToRoleWithPermissionsDto_ShouldMapAuditFields()
    {
        // Arrange
        DateTime createdAt = DateTime.UtcNow.AddDays(-7);
        DateTime updatedAt = DateTime.UtcNow.AddHours(-3);
        RoleEntity role = RoleFactory.Create();
        role.CreatedAt = createdAt;
        role.UpdatedAt = updatedAt;
        role.CreatedBy = "system";
        role.UpdatedBy = "admin-user";

        // Act
        var dto = role.ToRoleWithPermissionsDto(_mapper);

        // Assert
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
        dto.CreatedBy.Should().Be("system");
        dto.UpdatedBy.Should().Be("admin-user");
    }

    #endregion

    #region RoleWithPermissionsDto — permissions mapping

    [Fact]
    public void ToRoleWithPermissionsDto_ShouldMapPermissions()
    {
        // Arrange
        List<PermissionEntity> permissions = PermissionFactory.CreateMany(3);
        RoleEntity role = RoleFactory.Create("TestRole", permissions);

        // Act
        var dto = role.ToRoleWithPermissionsDto(_mapper);

        // Assert
        dto.Permissions.Should().HaveCount(3);
        dto.Permissions.Select(p => p.Id).Should().BeEquivalentTo(permissions.Select(p => p.Id));
    }

    #endregion
}
