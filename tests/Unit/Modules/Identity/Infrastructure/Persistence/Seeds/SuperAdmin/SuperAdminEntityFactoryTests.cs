using _116.Identity.Application.Auth.Services;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminEntityFactory"/>.
/// </summary>
public class SuperAdminEntityFactoryTests
{
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly SuperAdminEntityFactory _factory;

    public SuperAdminEntityFactoryTests()
    {
        _passwordServiceMock = new Mock<IPasswordService>();
        _factory = new SuperAdminEntityFactory(_passwordServiceMock.Object);

        // Setup default password environment variable
        string? originalPassword = Environment.GetEnvironmentVariable("DEFAULT_USER_PASSWORD");
        if (string.IsNullOrWhiteSpace(originalPassword))
        {
            Environment.SetEnvironmentVariable("DEFAULT_USER_PASSWORD", "TestPassword123!");
        }
    }

    #region CreateSuperAdminUser Tests

    [Fact]
    public void CreateSuperAdminUser_ShouldCreateUserWithCorrectEmail()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(SuperAdminConfiguration.Email);
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldCreateUserWithCorrectUsername()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.UserName.Should().Be(SuperAdminConfiguration.Username);
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldCallPasswordServiceWithConfiguredPassword()
    {
        // Arrange
        string expectedPassword = SuperAdminConfiguration.GetPassword();
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        _factory.CreateSuperAdminUser();

        // Assert
        _passwordServiceMock.Verify(x => x.Hash(expectedPassword), Times.Once);
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldSetHashedPassword()
    {
        // Arrange
        const string hashedPassword = "hashedPassword123";
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns(hashedPassword);

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.PasswordHash.Should().Be(hashedPassword);
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldMarkUserAsVerified()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldActivateUser()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateSuperAdminUser_ShouldGenerateNewGuid()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity result = _factory.CreateSuperAdminUser();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSuperAdminUser_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Arrange
        _passwordServiceMock.Setup(x => x.Hash(It.IsAny<string>())).Returns("hashedPassword");

        // Act
        UserEntity user1 = _factory.CreateSuperAdminUser();
        UserEntity user2 = _factory.CreateSuperAdminUser();

        // Assert
        user1.Id.Should().NotBe(user2.Id);
    }

    #endregion

    #region CreateSuperAdminRole Tests

    [Fact]
    public void CreateSuperAdminRole_ShouldCreateRoleWithCorrectName()
    {
        // Act
        RoleEntity result = SuperAdminEntityFactory.CreateSuperAdminRole();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(SuperAdminConfiguration.RoleName);
    }

    [Fact]
    public void CreateSuperAdminRole_ShouldCreateRoleWithCorrectDescription()
    {
        // Act
        RoleEntity result = SuperAdminEntityFactory.CreateSuperAdminRole();

        // Assert
        result.Description.Should().Be(SuperAdminConfiguration.RoleDescription);
    }

    [Fact]
    public void CreateSuperAdminRole_ShouldGenerateNewGuid()
    {
        // Act
        RoleEntity result = SuperAdminEntityFactory.CreateSuperAdminRole();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSuperAdminRole_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Act
        RoleEntity role1 = SuperAdminEntityFactory.CreateSuperAdminRole();
        RoleEntity role2 = SuperAdminEntityFactory.CreateSuperAdminRole();

        // Assert
        role1.Id.Should().NotBe(role2.Id);
    }

    #endregion

    #region CreateSystemAllPermission Tests

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectResource()
    {
        // Act
        PermissionEntity result = SuperAdminEntityFactory.CreateSystemAllPermission();

        // Assert
        result.Should().NotBeNull();
        result.Resource.Should().Be(SuperAdminConfiguration.PermissionResource);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectAction()
    {
        // Act
        PermissionEntity result = SuperAdminEntityFactory.CreateSystemAllPermission();

        // Assert
        result.Action.Should().Be(SuperAdminConfiguration.PermissionAction);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectDescription()
    {
        // Act
        PermissionEntity result = SuperAdminEntityFactory.CreateSystemAllPermission();

        // Assert
        result.Description.Should().Be(SuperAdminConfiguration.PermissionDescription);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldGenerateNewGuid()
    {
        // Act
        PermissionEntity result = SuperAdminEntityFactory.CreateSystemAllPermission();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSystemAllPermission_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Act
        PermissionEntity permission1 = SuperAdminEntityFactory.CreateSystemAllPermission();
        PermissionEntity permission2 = SuperAdminEntityFactory.CreateSystemAllPermission();

        // Assert
        permission1.Id.Should().NotBe(permission2.Id);
    }

    #endregion

    #region CreateUserRoleAssociation Tests

    [Fact]
    public void CreateUserRoleAssociation_ShouldCreateWithCorrectUserId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public void CreateUserRoleAssociation_ShouldCreateWithCorrectRoleId()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void CreateUserRoleAssociation_ShouldGenerateNewGuid()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateUserRoleAssociation_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid roleId = Guid.NewGuid();

        // Act
        UserRoleEntity association1 = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);
        UserRoleEntity association2 = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        association1.Id.Should().NotBe(association2.Id);
    }

    #endregion

    #region CreateRolePermissionAssociation Tests

    [Fact]
    public void CreateRolePermissionAssociation_ShouldCreateWithCorrectRoleId()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.Should().NotBeNull();
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void CreateRolePermissionAssociation_ShouldCreateWithCorrectPermissionId()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void CreateRolePermissionAssociation_ShouldGenerateNewGuid()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateRolePermissionAssociation_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Arrange
        Guid roleId = Guid.NewGuid();
        Guid permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity association1 = SuperAdminEntityFactory.CreateRolePermissionAssociation(
            roleId,
            permissionId
        );
        RolePermissionEntity association2 = SuperAdminEntityFactory.CreateRolePermissionAssociation(
            roleId,
            permissionId
        );

        // Assert
        association1.Id.Should().NotBe(association2.Id);
    }

    #endregion
}
