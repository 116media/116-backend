using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Unit.Tests.Common.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.Admin.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginAuthFactory"/>.
/// </summary>
public class AdminLoginAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly AdminLoginAuthFactory _factory;

    public AdminLoginAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _roleRepositoryMock = MockRoleRepository.Create();

        _factory = new AdminLoginAuthFactory(
            _authRepositoryMock.Object,
            _passwordServiceMock.Object,
            _roleRepositoryMock.Object
        );
    }

    #region Success Cases

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAuthData()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [new RoleDto(Guid.NewGuid(), "Admin", "Admin role", true, false, null)];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        AdminLoginAuthData result = await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
        result.Roles.Should().ContainSingle();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByEmail()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x =>
                x.GetUserWithRolesAndPermissionsByEmailOrThrow(
                    It.Is<Email>(e => e.Value == email),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyPassword()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Verify(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyUserIsAdmin()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldExtractRolesAndPermissions()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [new RoleDto(Guid.NewGuid(), "Admin", "Admin role", true, false, null)];
        List<PermissionDto> permissions =
        [
            new PermissionDto(Guid.NewGuid(), "users", "read", "Read users", true, false, null),
        ];

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        AdminLoginAuthData result = await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Permissions.Should().ContainSingle();
        _roleRepositoryMock.Verify(
            x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()),
            Times.Once
        );
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByEmailNotFound(new Email(email));

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsFalse();

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldNotCheckAdminStatus()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsFalse();

        // Act
        try
        {
            await _factory.AuthenticateAsync(email, password, CancellationToken.None);
        }
        catch (AuthenticationException)
        {
            // Expected
        }

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Never);
    }

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotAdmin_ShouldThrowAuthorizationException()
    {
        // Arrange
        string email = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminThrowsAuthorizationException();

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthorizationException>();
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task AuthenticateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(email, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByEmailOrThrow(It.IsAny<Email>(), cts.Token),
            Times.Once
        );
    }

    #endregion
}
