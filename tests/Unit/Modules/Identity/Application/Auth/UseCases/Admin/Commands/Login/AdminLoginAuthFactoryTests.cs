using _116.Identity.Application.Auth.Services;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login;
using _116.Identity.Application.Auth.UseCases.Admin.Commands.Login.Contracts;
using _116.Identity.Application.Shared.Repositories;
using _116.Identity.Domain.Entities;
using _116.Identity.Domain.ValueObjects;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Factories;
using _116.Unit.Tests.Common.Mocks.Repositories;
using _116.Unit.Tests.Common.Mocks.Services;
using AwesomeAssertions;
using Moq;
using Xunit;

namespace _116.Unit.Tests.Modules.Identity.Application.Auth.UseCases.Admin.Commands.Login;

/// <summary>
/// Unit tests for <see cref="AdminLoginAuthFactory"/>.
/// </summary>
public class AdminLoginAuthFactoryTests
{
    private readonly Mock<IAuthRepository> _authRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly AdminLoginAuthFactory _factory;

    public AdminLoginAuthFactoryTests()
    {
        _authRepositoryMock = MockAuthRepository.Create();
        _passwordServiceMock = MockPasswordService.Create();

        _factory = new AdminLoginAuthFactory(_authRepositoryMock.Object, _passwordServiceMock.Object);
    }

    #region Success Cases

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnAuthData()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        AdminLoginAuthData result = await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldFetchUserByEmail()
    {
        // Arrange
        string email = "admin@example.com";
        string password = "ValidPassword123!";
        UserEntity user = UserFactory.CreateAdmin();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

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

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

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

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

        // Act
        await _factory.AuthenticateAsync(email, password, CancellationToken.None);

        // Assert
        _authRepositoryMock.Verify(x => x.IsUserAdmin(It.IsAny<UserEntity>()), Times.Once);
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
        using CancellationTokenSource cts = new();

        _authRepositoryMock.SetupGetUserWithRolesByEmail(user);
        _passwordServiceMock.SetupVerifyReturnsTrue();
        _authRepositoryMock.SetupIsUserAdminReturnsTrue();

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
