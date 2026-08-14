using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Shared.Errors;
using _116.Identity.Domain.Entities;
using _116.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Infrastructure.Persistence.Seeds.SuperAdmin;

/// <summary>
/// Unit tests for <see cref="SuperAdminEntityFactory"/>.
/// </summary>
[Collection("EnvironmentVariable")]
public class SuperAdminEntityFactoryTests : IDisposable
{
    private const string DefaultPasswordVariable = "DEFAULT_USER_PASSWORD";

    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly UserErrors _userErrors = TestErrorsFactory.CreateUserErrors();
    private readonly SuperAdminEntityFactory _factory;
    private readonly string? _originalPassword;

    public SuperAdminEntityFactoryTests()
    {
        _passwordServiceMock = new Mock<IPasswordService>();
        _factory = new SuperAdminEntityFactory(_passwordServiceMock.Object, TestErrorsFactory.CreateUserErrors());

        // Setup default password environment variable
        _originalPassword = Environment.GetEnvironmentVariable(DefaultPasswordVariable);
        if (string.IsNullOrWhiteSpace(_originalPassword))
        {
            Environment.SetEnvironmentVariable(DefaultPasswordVariable, "TestPassword123!");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Environment.SetEnvironmentVariable(DefaultPasswordVariable, _originalPassword);
        GC.SuppressFinalize(this);
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
        RoleEntity result = _factory.CreateSuperAdminRole();

        // Assert
        result.Name.Should().Be(SuperAdminConfiguration.RoleName);
    }

    [Fact]
    public void CreateSuperAdminRole_ShouldCreateRoleWithCorrectDescription()
    {
        // Act
        RoleEntity result = _factory.CreateSuperAdminRole();

        // Assert
        result.Description.Should().Be(SuperAdminConfiguration.RoleDescription);
    }

    [Fact]
    public void CreateSuperAdminRole_ShouldGenerateNewGuid()
    {
        // Act
        RoleEntity result = _factory.CreateSuperAdminRole();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSuperAdminRole_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Act
        RoleEntity role1 = _factory.CreateSuperAdminRole();
        RoleEntity role2 = _factory.CreateSuperAdminRole();

        // Assert
        role1.Id.Should().NotBe(role2.Id);
    }

    #endregion

    #region CreateSystemAllPermission Tests

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectResource()
    {
        // Act
        PermissionEntity result = _factory.CreateSystemAllPermission();

        // Assert
        result.Resource.Should().Be(SuperAdminConfiguration.PermissionResource);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectAction()
    {
        // Act
        PermissionEntity result = _factory.CreateSystemAllPermission();

        // Assert
        result.Action.Should().Be(SuperAdminConfiguration.PermissionAction);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldCreatePermissionWithCorrectDescription()
    {
        // Act
        PermissionEntity result = _factory.CreateSystemAllPermission();

        // Assert
        result.Description.Should().Be(SuperAdminConfiguration.PermissionDescription);
    }

    [Fact]
    public void CreateSystemAllPermission_ShouldGenerateNewGuid()
    {
        // Act
        PermissionEntity result = _factory.CreateSystemAllPermission();

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateSystemAllPermission_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Act
        PermissionEntity permission1 = _factory.CreateSystemAllPermission();
        PermissionEntity permission2 = _factory.CreateSystemAllPermission();

        // Assert
        permission1.Id.Should().NotBe(permission2.Id);
    }

    #endregion

    #region CreateUserRoleAssociation Tests

    [Fact]
    public void CreateUserRoleAssociation_ShouldCreateWithCorrectUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public void CreateUserRoleAssociation_ShouldCreateWithCorrectRoleId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void CreateUserRoleAssociation_ShouldGenerateNewGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        // Act
        UserRoleEntity result = SuperAdminEntityFactory.CreateUserRoleAssociation(userId, roleId);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateUserRoleAssociation_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

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
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.RoleId.Should().Be(roleId);
    }

    [Fact]
    public void CreateRolePermissionAssociation_ShouldCreateWithCorrectPermissionId()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.PermissionId.Should().Be(permissionId);
    }

    [Fact]
    public void CreateRolePermissionAssociation_ShouldGenerateNewGuid()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        // Act
        RolePermissionEntity result = SuperAdminEntityFactory.CreateRolePermissionAssociation(roleId, permissionId);

        // Assert
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateRolePermissionAssociation_MultipleInvocations_ShouldGenerateUniqueIds()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

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
