using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Public.Commands.Login.Contracts;
using _116.Identity.Application.Shared.DTOs;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Tests.Fixtures.Helpers;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Public.Commands.Login;

/// <summary>
/// Unit tests for <see cref="PublicLoginAuthFactory"/>.
/// </summary>
public class PublicLoginAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly PublicLoginAuthFactory _factory;

    public PublicLoginAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();
        _roleRepositoryMock = MockRoleRepository.Create();

        _factory = new PublicLoginAuthFactory(
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
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(name: "Visitor", description: "Visitor role")];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
        result.Roles.Should().ContainSingle();
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByCredentials()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldVerifyPassword()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        _passwordServiceMock.Verify(x => x.Verify(password, user.PasswordHash), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldExtractRolesAndPermissions()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [AuthTestHelpers.CreateRoleDto(name: "Visitor", description: "Visitor role")];
        List<PermissionDto> permissions =
        [
            AuthTestHelpers.CreatePermissionDto(resource: "articles", description: "Read articles"),
        ];

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.Roles.Should().ContainSingle();
        result.Permissions.Should().ContainSingle();
        _roleRepositoryMock.Verify(
            x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WithUsername_ShouldWork()
    {
        // Arrange
        string credentials = "testuser";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        PublicLoginAuthData result = await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
    }

    #endregion

    #region Failure Cases

    [Fact]
    public async Task AuthenticateAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        string credentials = "nonexistent@example.com";
        string password = "ValidPassword123!";

        _authRepositoryMock.SetupGetUserWithRolesByCredentialsNotFound(credentials);

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldThrowAuthenticationException()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsFalse();

        // Act
        Func<Task> act = async () => await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<AuthenticationException>();
    }

    [Fact]
    public async Task AuthenticateAsync_WhenPasswordInvalid_ShouldNotExtractRoles()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "WrongPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsFalse();

        // Act
        try
        {
            await _factory.AuthenticateAsync(credentials, password, CancellationToken.None);
        }
        catch (AuthenticationException)
        {
            // Expected
        }

        // Assert
        _roleRepositoryMock.Verify(
            x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()),
            Times.Never
        );
    }

    #endregion

    #region Cancellation Token Tests

    [Fact]
    public async Task AuthenticateAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        string credentials = "user@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateVerifiedActive();
        List<RoleDto> roles = [];
        List<PermissionDto> permissions = [];
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByCredentials(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _roleRepositoryMock
            .Setup(x => x.GetUserRolesAndPermissions(It.IsAny<ICollection<UserRoleEntity>>()))
            .Returns((roles, permissions));

        // Act
        await _factory.AuthenticateAsync(credentials, password, cts.Token);

        // Assert
        _authRepositoryMock.Verify(
            x => x.GetUserWithRolesAndPermissionsByCredentialsOrThrow(credentials, cts.Token),
            Times.Once
        );
    }

    #endregion
}
